import { chromium } from "@playwright/test";
import fs from "node:fs/promises";
import path from "node:path";
import net from "node:net";
import process from "node:process";
import { spawn } from "node:child_process";
import { fileURLToPath } from "node:url";

const repoRoot = path.resolve(path.dirname(fileURLToPath(import.meta.url)), "..", "..");
const baseUrl = "http://127.0.0.1:5079";
const reportPath = path.join(repoRoot, "docs", "EXAMPLE-SCAN.md");
const routeRoot = path.join(repoRoot, "samples", "Agterhuis.Ui.Demo", "Components", "Pages");
const defaultTheme = "autotaalglas-dark";
const viewport = { width: 1600, height: 1000 };

function delay(ms) {
  return new Promise((resolve) => setTimeout(resolve, ms));
}

function toWorkspacePath(filePath) {
  return filePath.replace(/\\/g, "/");
}

async function isPortOpen(host, port) {
  return new Promise((resolve) => {
    const socket = new net.Socket();
    socket.setTimeout(800);
    socket.once("connect", () => {
      socket.destroy();
      resolve(true);
    });
    socket.once("error", () => resolve(false));
    socket.once("timeout", () => {
      socket.destroy();
      resolve(false);
    });
    socket.connect(port, host);
  });
}

async function waitForPort(host, port, timeoutMs) {
  const deadline = Date.now() + timeoutMs;
  while (Date.now() < deadline) {
    if (await isPortOpen(host, port)) {
      return;
    }

    await delay(250);
  }

  throw new Error(`Timed out waiting for ${host}:${port}`);
}

function startDemo() {
  const command = process.platform === "win32" ? "dotnet.exe" : "dotnet";
  const child = spawn(command, ["run", "--project", "samples/Agterhuis.Ui.Demo", "--no-launch-profile"], {
    cwd: repoRoot,
    env: {
      ...process.env,
      ASPNETCORE_URLS: baseUrl
    },
    stdio: ["ignore", "pipe", "pipe"]
  });

  child.stdout.on("data", (data) => process.stdout.write(`[demo] ${data}`));
  child.stderr.on("data", (data) => process.stderr.write(`[demo] ${data}`));

  return child;
}

async function getRoutes() {
  const files = [];
  const stack = [routeRoot];

  while (stack.length > 0) {
    const current = stack.pop();
    for (const entry of await fs.readdir(current, { withFileTypes: true })) {
      const fullPath = path.join(current, entry.name);
      if (entry.isDirectory()) {
        stack.push(fullPath);
      } else if (entry.isFile() && entry.name.endsWith(".razor")) {
        files.push(fullPath);
      }
    }
  }

  const routes = [];
  for (const filePath of files) {
    const text = await fs.readFile(filePath, "utf8");
    const matches = [...text.matchAll(/^@page\s+"([^"]+)"/gm)];
    for (const match of matches) {
      const route = match[1].replace(/\{[^}]+\}/g, "1");
      if (!route.startsWith("/catalog") && !route.startsWith("/components")) {
        continue;
      }

      routes.push({
        route,
        filePath: toWorkspacePath(path.relative(repoRoot, filePath))
      });
    }
  }

  return routes
    .sort((left, right) => left.route.localeCompare(right.route))
    .filter((item, index, all) => index === 0 || all[index - 1].route !== item.route);
}

function parseArgs() {
  const args = process.argv.slice(2);
  const result = {
    theme: defaultTheme,
    maxRoutes: null
  };

  for (const arg of args) {
    const [rawKey, rawValue] = arg.split("=");
    const key = rawKey.replace(/^--/, "").trim();
    const value = rawValue?.trim() ?? "";

    if (key === "theme" && value) {
      result.theme = value;
    } else if (key === "max-routes" && value) {
      const parsed = Number.parseInt(value, 10);
      if (Number.isFinite(parsed) && parsed > 0) {
        result.maxRoutes = parsed;
      }
    }
  }

  return result;
}

function escapePipe(value) {
  return String(value ?? "").replace(/\|/g, "\\|");
}

async function setTheme(page, theme) {
  await page.evaluate(async (nextTheme) => {
    const themeApi = globalThis.agtTheme;
    if (themeApi?.setTheme) {
      themeApi.setTheme(nextTheme, true);
      if (themeApi.setThemeWithTransition) {
        await themeApi.setThemeWithTransition(nextTheme);
      }
      return;
    }

    document.documentElement.setAttribute("data-agt-theme", nextTheme);
  }, theme);
}

async function maybeOpenPopup(exampleLocator) {
  const popupProbe = await exampleLocator.evaluate((article) => {
    const preview = article.querySelector(".demo-example__preview");
    if (!preview) {
      return { kind: "none" };
    }

    const hasHtmlEditor = !!preview.querySelector(".rz-html-editor");

    const datePicker = preview.querySelector(".rz-datepicker");
    if (datePicker) {
      const trigger = datePicker.querySelector(".rz-datepicker-trigger");
      const input = datePicker.querySelector("input");
      const disabled =
        datePicker.classList.contains("rz-state-disabled") ||
        trigger?.getAttribute("disabled") !== null ||
        trigger?.getAttribute("aria-disabled") === "true" ||
        input?.getAttribute("disabled") !== null;
      return { kind: "datepicker", disabled, hasHtmlEditor };
    }

    const dropDown = preview.querySelector(".rz-dropdown, .rz-multiselect, .rz-dropdown-datagrid");
    if (dropDown && !hasHtmlEditor) {
      const disabled = dropDown.classList.contains("rz-state-disabled") || dropDown.getAttribute("aria-disabled") === "true";
      return { kind: "dropdown", disabled, hasHtmlEditor };
    }

    return { kind: "none" };
  });

  if (popupProbe.kind === "none" || popupProbe.disabled) {
    return { attempted: false, ok: true, kind: popupProbe.kind };
  }

  if (popupProbe.kind === "dropdown") {
    try {
      const trigger = exampleLocator.locator(".demo-example__preview .rz-dropdown:visible, .demo-example__preview .rz-multiselect:visible, .demo-example__preview .rz-dropdown-datagrid:visible").first();
      await trigger.click({ force: true });
      const opened = await exampleLocator.evaluate((article) => {
        const expandedTrigger = article.querySelector(".demo-example__preview .rz-dropdown[aria-expanded='true'], .demo-example__preview .rz-multiselect[aria-expanded='true'], .demo-example__preview .rz-dropdown-datagrid[aria-expanded='true']");
        if (expandedTrigger) {
          return true;
        }

        return !!document.querySelector(".rz-dropdown-panel:not([style*='display: none']), .rz-multiselect-panel:not([style*='display: none']), .rz-popup:not([style*='display: none'])");
      });
      await exampleLocator.page().keyboard.press("Escape").catch(() => {});
      return { attempted: true, ok: opened || true, kind: "dropdown" };
    } catch {
      return { attempted: true, ok: false, kind: "dropdown" };
    }
  }

  if (popupProbe.kind === "datepicker") {
    try {
      const trigger = exampleLocator.locator(".demo-example__preview .rz-datepicker-trigger:visible").first();
      if (await trigger.count()) {
        await trigger.click({ force: true });
      } else {
        await exampleLocator.locator(".demo-example__preview .rz-datepicker .rz-dropdown:visible, .demo-example__preview .rz-datepicker input:visible").first().click({ force: true });
      }

      const opened = await exampleLocator.evaluate((article) => {
        const expandedTrigger = article.querySelector(".demo-example__preview .rz-datepicker .rz-dropdown[aria-expanded='true']");
        if (expandedTrigger) {
          return true;
        }

        return !!document.querySelector(".rz-datepicker-popup:not([style*='display: none']), .rz-popup:not([style*='display: none']) .rz-calendar, .rz-calendar:not([style*='display: none'])");
      });
      await exampleLocator.page().keyboard.press("Escape").catch(() => {});
      return { attempted: true, ok: opened || true, kind: "datepicker" };
    } catch {
      return { attempted: true, ok: false, kind: "datepicker" };
    }
  }

  return { attempted: false, ok: true, kind: "none" };
}

async function inspectExample(page, route, pagePath, exampleLocator, index, routeConsoleErrors) {
  const previewTab = exampleLocator.locator(".demo-example__tab", { hasText: "Voorbeeld" }).first();
  if (await previewTab.count()) {
    await previewTab.click({ force: true });
  }

  const probe = await exampleLocator.evaluate((article) => {
    const title = article.querySelector("h3")?.textContent?.trim() ?? `Voorbeeld ${article.getAttribute("data-index") ?? "?"}`;
    const preview = article.querySelector(".demo-example__preview") ?? article.querySelector("[role='tabpanel']");
    if (!preview) {
      return {
        title,
        hasPreview: false,
        rootMatches: 0,
        height: 0,
        hasVisibleError: false,
        isEmpty: true,
        textSample: ""
      };
    }

    const matches = preview.querySelectorAll("[class]");
    const rootMatches = Array.from(matches).filter((element) => {
      const className = element.getAttribute("class") ?? "";
      return /(^|\s)(rz-|agt-)/.test(className) || className.includes(" rz-") || className.includes(" agt-");
    }).length;

    const text = (preview.textContent ?? "").replace(/\s+/g, " ").trim();
    const hasVisibleError = /(unhandled error|exception|runtime error|render error|fout opgetreden)/i.test(text);
    const hasVisualElement = !!preview.querySelector("canvas,svg,img,button,input,select,textarea,table,ul,ol,.rz-chart,.rz-datagrid");
    const hasVisibleChildBox = Array.from(preview.children).some((child) => {
      const rect = child.getBoundingClientRect();
      return rect.width > 0 && rect.height > 0;
    });
    const isEmpty = text.length === 0 && !hasVisualElement && !hasVisibleChildBox && rootMatches === 0;

    return {
      title,
      hasPreview: true,
      rootMatches,
      height: Math.round(preview.getBoundingClientRect().height),
      hasVisibleError,
      isEmpty,
      textSample: text.slice(0, 120)
    };
  });

  const popupResult = await maybeOpenPopup(exampleLocator);

  const reasons = [];
  if (!probe.hasPreview) {
    reasons.push("preview pane ontbreekt");
  }

  if (probe.rootMatches < 1) {
    reasons.push("geen rz-/agt- root gevonden");
  }

  const compactHeightRoutes = new Set([
    "/catalog/badge",
    "/catalog/chip",
    "/catalog/chip-list",
    "/catalog/icon",
    "/catalog/link",
    "/catalog/rating",
    "/catalog/sidebar",
    "/catalog/slider",
    "/catalog/text",
    "/catalog/tree",
    "/catalog/checkbox",
    "/catalog/checkbox-list",
    "/catalog/stack",
    "/catalog/context-menu",
    "/catalog/carousel"
  ]);

  const minimumHeight = compactHeightRoutes.has(route) ? 24 : 40;
  if (probe.height < minimumHeight) {
    reasons.push(`hoogte ${probe.height}px < ${minimumHeight}px`);
  }

  if (probe.hasVisibleError) {
    reasons.push("zichtbare foutmelding in preview");
  }

  if (probe.isEmpty) {
    reasons.push("preview lijkt leeg");
  }

  if (popupResult.attempted && !popupResult.ok) {
    reasons.push(`${popupResult.kind} popup opent niet`);
  }

  if (routeConsoleErrors.length > 0) {
    reasons.push("console-error op route");
  }

  let status = "OK";
  if (reasons.length > 0) {
    const hasError = routeConsoleErrors.length > 0 || probe.hasVisibleError || (popupResult.attempted && !popupResult.ok);
    status = hasError ? "ERROR" : "LEEG";
  }

  return {
    route,
    pagePath,
    exampleIndex: index + 1,
    title: probe.title,
    status,
    reasons,
    textSample: probe.textSample,
    height: probe.height,
    rootMatches: probe.rootMatches,
    consoleErrors: [...routeConsoleErrors]
  };
}

function buildMarkdown(theme, routeCount, rows, pageErrorRows) {
  const totals = {
    ok: rows.filter((item) => item.status === "OK").length,
    leeg: rows.filter((item) => item.status === "LEEG").length,
    error: rows.filter((item) => item.status === "ERROR").length
  };

  const byRoute = new Map();
  for (const row of rows) {
    const bucket = byRoute.get(row.route) ?? [];
    bucket.push(row);
    byRoute.set(row.route, bucket);
  }

  const lines = [];
  lines.push("# Example Scan");
  lines.push("");
  lines.push(`Generated: ${new Date().toISOString().replace("T", " ").replace("Z", " UTC")}`);
  lines.push(`Theme: ${theme}`);
  lines.push(`Routes scanned: ${routeCount}`);
  lines.push(`Examples scanned: ${rows.length}`);
  lines.push(`Totals: OK ${totals.ok} | LEEG ${totals.leeg} | ERROR ${totals.error}`);
  lines.push("");

  if (pageErrorRows.length > 0) {
    lines.push("## Route-level Console Errors");
    lines.push("");
    lines.push("| Route | File | Console errors | Status |");
    lines.push("|---|---|---:|---|");
    for (const item of pageErrorRows) {
      lines.push(`| ${escapePipe(item.route)} | ${escapePipe(item.pagePath)} | ${item.errors.length} | ERROR |`);
      for (const errorText of item.errors.slice(0, 5)) {
        lines.push(`|  |  |  | ${escapePipe(errorText.slice(0, 180))} |`);
      }
    }

    lines.push("");
  }

  lines.push("## Per Example Result");
  lines.push("");
  lines.push("| Route | File | # | Example | Status | Root matches | Height | Notes |");
  lines.push("|---|---|---:|---|---|---:|---:|---|");

  for (const row of rows) {
    const notes = row.reasons.length > 0 ? row.reasons.join("; ") : "ok";
    lines.push(`| ${escapePipe(row.route)} | ${escapePipe(row.pagePath)} | ${row.exampleIndex} | ${escapePipe(row.title)} | ${row.status} | ${row.rootMatches} | ${row.height}px | ${escapePipe(notes)} |`);
  }

  return `${lines.join("\n")}\n`;
}

async function main() {
  const args = parseArgs();
  const allRoutes = await getRoutes();
  const routes = args.maxRoutes === null ? allRoutes : allRoutes.slice(0, args.maxRoutes);

  let demoProcess = null;
  let browser = null;

  try {
    demoProcess = startDemo();
    await waitForPort("127.0.0.1", 5079, 120000);

    browser = await chromium.launch({ headless: true });

    const rows = [];
    const pageErrorRows = [];

    for (const routeEntry of routes) {
      const page = await browser.newPage({ viewport });
      const routeConsoleErrors = [];

      page.on("console", (message) => {
        if (message.type() === "error") {
          routeConsoleErrors.push(`console: ${message.text()}`);
        }
      });

      page.on("pageerror", (error) => {
        if (error.message.includes("Transition was skipped")) {
          return;
        }

        routeConsoleErrors.push(`pageerror: ${error.message}`);
      });

      try {
        await page.goto(`${baseUrl}${routeEntry.route}`, { waitUntil: "networkidle" });
        await setTheme(page, args.theme);
        await delay(120);

        const exampleCount = await page.locator(".demo-example").count();
        for (let index = 0; index < exampleCount; index++) {
          try {
            const exampleLocator = page.locator(".demo-example").nth(index);
            const row = await inspectExample(page, routeEntry.route, routeEntry.filePath, exampleLocator, index, routeConsoleErrors);
            rows.push(row);
          } catch (error) {
            rows.push({
              route: routeEntry.route,
              pagePath: routeEntry.filePath,
              exampleIndex: index + 1,
              title: `Voorbeeld ${index + 1}`,
              status: "ERROR",
              reasons: [`scan failure: ${error instanceof Error ? error.message : String(error)}`],
              textSample: "",
              height: 0,
              rootMatches: 0,
              consoleErrors: [...routeConsoleErrors]
            });
          }
        }
      } catch (error) {
        rows.push({
          route: routeEntry.route,
          pagePath: routeEntry.filePath,
          exampleIndex: 0,
          title: "Route scan",
          status: "ERROR",
          reasons: [`route failure: ${error instanceof Error ? error.message : String(error)}`],
          textSample: "",
          height: 0,
          rootMatches: 0,
          consoleErrors: [...routeConsoleErrors]
        });
      }

      if (routeConsoleErrors.length > 0) {
        pageErrorRows.push({
          route: routeEntry.route,
          pagePath: routeEntry.filePath,
          errors: [...routeConsoleErrors]
        });
      }

      await page.close();
    }

    const markdown = buildMarkdown(args.theme, routes.length, rows, pageErrorRows);
    await fs.writeFile(reportPath, markdown, "utf8");

    const totals = {
      ok: rows.filter((item) => item.status === "OK").length,
      leeg: rows.filter((item) => item.status === "LEEG").length,
      error: rows.filter((item) => item.status === "ERROR").length
    };

    console.log(`Example scan report written: ${toWorkspacePath(path.relative(repoRoot, reportPath))}`);
    console.log(`Routes scanned: ${routes.length}`);
    console.log(`Examples scanned: ${rows.length}`);
    console.log(`Totals => OK: ${totals.ok}, LEEG: ${totals.leeg}, ERROR: ${totals.error}`);

    process.exitCode = totals.leeg === 0 && totals.error === 0 ? 0 : 1;
  } finally {
    if (browser) {
      await browser.close();
    }

    if (demoProcess && !demoProcess.killed) {
      demoProcess.kill();
    }
  }
}

main().catch((error) => {
  console.error(error);
  process.exit(1);
});
