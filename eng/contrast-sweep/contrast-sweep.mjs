import { chromium } from "@playwright/test";
import fs from "node:fs/promises";
import path from "node:path";
import net from "node:net";
import process from "node:process";
import { spawn } from "node:child_process";
import { fileURLToPath } from "node:url";

const repoRoot = path.resolve(path.dirname(fileURLToPath(import.meta.url)), "..", "..");
const demoBaseUrl = "http://127.0.0.1:5079";
const reportPath = path.join(repoRoot, "docs", "CONTRAST-SWEEP.md");
const allowlistPath = path.join(repoRoot, "eng", "contrast-sweep", "allowlist.json");
const routeRoot = path.join(repoRoot, "samples", "Agterhuis.Ui.Demo", "Components", "Pages");
const themeSourcePath = path.join(repoRoot, "src", "Agterhuis.Ui", "Theming", "AgtTheme.cs");
const checkpointDefaultPath = path.join(repoRoot, "eng", "contrast-sweep", "contrast-sweep.checkpoint.json");
const viewport = { width: 1600, height: 1000 };

function delay(milliseconds) {
  return new Promise((resolve) => setTimeout(resolve, milliseconds));
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
      ASPNETCORE_URLS: demoBaseUrl
    },
    stdio: ["ignore", "pipe", "pipe"]
  });

  child.stdout.on("data", (data) => process.stdout.write(`[demo] ${data}`));
  child.stderr.on("data", (data) => process.stderr.write(`[demo] ${data}`));

  return child;
}

async function readJson(filePath, fallback) {
  try {
    const text = await fs.readFile(filePath, "utf8");
    return JSON.parse(text);
  } catch (error) {
    if (error.code === "ENOENT") {
      return fallback;
    }

    throw error;
  }
}

function parseThemes(source) {
  const themes = [];
  const themePattern = /new\("([^"]+)",\s*"([^"]+)",\s*"([^"]+)",\s*"([^"]+)"(?:,\s*"[^"]+"){0,3}\)/g;
  for (const match of source.matchAll(themePattern)) {
    themes.push({
      name: match[1],
      displayName: match[2],
      lightVariantId: match[3],
      darkVariantId: match[4]
    });
  }

  return themes;
}

async function getThemes() {
  const source = await fs.readFile(themeSourcePath, "utf8");
  return parseThemes(source);
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
    const match = text.match(/^@page\s+"([^"]+)"/m);
    if (!match) {
      continue;
    }

    const route = match[1].replace(/\{[^}]+\}/g, "1");
    if (route === "/Error" || route === "/not-found") {
      continue;
    }

    routes.push({
      route,
      filePath: toWorkspacePath(path.relative(repoRoot, filePath))
    });
  }

  routes.sort((left, right) => left.route.localeCompare(right.route));
  return routes;
}

function parseColor(value) {
  if (!value || value === "transparent" || value === "none") {
    return null;
  }

  const rgbaMatch = value.match(/rgba?\(([^)]+)\)/i);
  if (rgbaMatch) {
    const parts = rgbaMatch[1].split(",").map((part) => part.trim());
    const [r, g, b] = parts.slice(0, 3).map((part) => Number.parseFloat(part));
    const alpha = parts[3] ? Number.parseFloat(parts[3]) : 1;
    return { r, g, b, a: Number.isFinite(alpha) ? alpha : 1 };
  }

  const hexMatch = value.match(/^#([0-9a-f]{3,8})$/i);
  if (hexMatch) {
    const hex = hexMatch[1];
    if (hex.length === 3) {
      return {
        r: Number.parseInt(hex[0] + hex[0], 16),
        g: Number.parseInt(hex[1] + hex[1], 16),
        b: Number.parseInt(hex[2] + hex[2], 16),
        a: 1
      };
    }

    if (hex.length === 4) {
      return {
        r: Number.parseInt(hex[0] + hex[0], 16),
        g: Number.parseInt(hex[1] + hex[1], 16),
        b: Number.parseInt(hex[2] + hex[2], 16),
        a: Number.parseInt(hex[3] + hex[3], 16) / 255
      };
    }

    if (hex.length === 6 || hex.length === 8) {
      return {
        r: Number.parseInt(hex.slice(0, 2), 16),
        g: Number.parseInt(hex.slice(2, 4), 16),
        b: Number.parseInt(hex.slice(4, 6), 16),
        a: hex.length === 8 ? Number.parseInt(hex.slice(6, 8), 16) / 255 : 1
      };
    }
  }

  return null;
}

function composite(foreground, background) {
  const alpha = foreground.a + background.a * (1 - foreground.a);
  if (alpha === 0) {
    return { r: 255, g: 255, b: 255, a: 0 };
  }

  return {
    r: Math.round((foreground.r * foreground.a + background.r * background.a * (1 - foreground.a)) / alpha),
    g: Math.round((foreground.g * foreground.a + background.g * background.a * (1 - foreground.a)) / alpha),
    b: Math.round((foreground.b * foreground.a + background.b * background.a * (1 - foreground.a)) / alpha),
    a: alpha
  };
}

function relativeLuminance(color) {
  const convert = (channel) => {
    const normalized = channel / 255;
    return normalized <= 0.03928 ? normalized / 12.92 : ((normalized + 0.055) / 1.055) ** 2.4;
  };

  return 0.2126 * convert(color.r) + 0.7152 * convert(color.g) + 0.0722 * convert(color.b);
}

function contrastRatio(foreground, background) {
  const fg = foreground.a < 1 ? composite(foreground, background) : foreground;
  const bg = background.a < 1 ? composite(background, { r: 255, g: 255, b: 255, a: 1 }) : background;
  const fgLum = relativeLuminance(fg);
  const bgLum = relativeLuminance(bg);
  const lighter = Math.max(fgLum, bgLum);
  const darker = Math.min(fgLum, bgLum);
  return (lighter + 0.05) / (darker + 0.05);
}

function formatColor(color) {
  const alpha = Number.isFinite(color.a) ? color.a : 1;
  return alpha < 1
    ? `rgba(${color.r}, ${color.g}, ${color.b}, ${alpha.toFixed(3).replace(/0+$/, "").replace(/\.$/, "")})`
    : `rgb(${color.r}, ${color.g}, ${color.b})`;
}

function isTextLikeElement(element) {
  if (!element || element.nodeType !== Node.ELEMENT_NODE) {
    return false;
  }

  const tagName = element.tagName;
  if (["SCRIPT", "STYLE", "TEMPLATE", "NOSCRIPT", "SVG", "PATH", "DEFS"].includes(tagName)) {
    return false;
  }

  return true;
}

function buildSelector(element) {
  const parts = [];
  let current = element;
  let depth = 0;

  while (current && current.nodeType === Node.ELEMENT_NODE && depth < 4) {
    let part = current.tagName.toLowerCase();
    if (current.id) {
      part += `#${current.id}`;
      parts.unshift(part);
      break;
    }

    const classes = Array.from(current.classList || [])
      .filter((className) => className && !className.startsWith("rz-"))
      .slice(0, 2);
    if (classes.length > 0) {
      part += `.${classes.join(".")}`;
    }

    const parent = current.parentElement;
    if (parent) {
      const siblings = Array.from(parent.children).filter((candidate) => candidate.tagName === current.tagName);
      if (siblings.length > 1) {
        part += `:nth-of-type(${siblings.indexOf(current) + 1})`;
      }
    }

    parts.unshift(part);
    current = current.parentElement;
    depth += 1;
  }

  return parts.join(" > ");
}

async function collectCandidates(page, stateLabel) {
  return page.evaluate((currentStateLabel) => {
    const textElements = [];

    const parseColor = (value) => {
      if (!value || value === "transparent" || value === "none") {
        return null;
      }

      const rgbaMatch = value.match(/rgba?\(([^)]+)\)/i);
      if (rgbaMatch) {
        const parts = rgbaMatch[1].split(",").map((part) => part.trim());
        const [r, g, b] = parts.slice(0, 3).map((part) => Number.parseFloat(part));
        const alpha = parts[3] ? Number.parseFloat(parts[3]) : 1;
        return { r, g, b, a: Number.isFinite(alpha) ? alpha : 1 };
      }

      return null;
    };

    const composite = (foreground, background) => {
      const alpha = foreground.a + background.a * (1 - foreground.a);
      if (alpha === 0) {
        return { r: 255, g: 255, b: 255, a: 0 };
      }

      return {
        r: Math.round((foreground.r * foreground.a + background.r * background.a * (1 - foreground.a)) / alpha),
        g: Math.round((foreground.g * foreground.a + background.g * background.a * (1 - foreground.a)) / alpha),
        b: Math.round((foreground.b * foreground.a + background.b * background.a * (1 - foreground.a)) / alpha),
        a: alpha
      };
    };

    const isVisible = (element) => {
      if (!element || element.nodeType !== Node.ELEMENT_NODE) {
        return false;
      }

      if (element.closest(".rzi, .notranslate") || element.getAttribute("aria-hidden") === "true") {
        return false;
      }

      const style = window.getComputedStyle(element);
      if (style.display === "none" || style.visibility === "hidden" || style.visibility === "collapse") {
        return false;
      }

      if (style.opacity === "0") {
        return false;
      }

      const rect = element.getBoundingClientRect();
      return rect.width >= 1 && rect.height >= 1;
    };

    const isDisabled = (element) => {
      if (!element || element.nodeType !== Node.ELEMENT_NODE) {
        return false;
      }

      if (element.matches(":disabled")) {
        return true;
      }

      if (element.getAttribute("aria-disabled") === "true") {
        return true;
      }

      const disabledAncestor = element.closest("[aria-disabled='true']");
      return Boolean(disabledAncestor);
    };

    const buildSelector = (element) => {
      const parts = [];
      let current = element;
      let depth = 0;

      while (current && current.nodeType === Node.ELEMENT_NODE && depth < 4) {
        let part = current.tagName.toLowerCase();
        if (current.id) {
          part += `#${current.id}`;
          parts.unshift(part);
          break;
        }

        const classes = Array.from(current.classList || [])
          .filter((className) => className && !className.startsWith("rz-"))
          .slice(0, 2);
        if (classes.length > 0) {
          part += `.${classes.join(".")}`;
        }

        const parent = current.parentElement;
        if (parent) {
          const siblings = Array.from(parent.children).filter((candidate) => candidate.tagName === current.tagName);
          if (siblings.length > 1) {
            part += `:nth-of-type(${siblings.indexOf(current) + 1})`;
          }
        }

        parts.unshift(part);
        current = current.parentElement;
        depth += 1;
      }

      return parts.join(" > ");
    };

    const extractText = (element) => {
      const segments = [];
      for (const node of element.childNodes) {
        if (node.nodeType === Node.TEXT_NODE) {
          const text = node.textContent?.replace(/\s+/g, " ").trim();
          if (text) {
            segments.push(text);
          }
        }
      }

      if (segments.length === 0 && (element.matches("input, textarea, select") || element.hasAttribute("placeholder"))) {
        const placeholder = element.getAttribute("placeholder");
        if (placeholder) {
          segments.push(placeholder.trim());
        }
      }

      return segments.join(" ").trim();
    };

    const getLayerCandidates = (element) => {
      const style = window.getComputedStyle(element);
      const candidates = [];

      if (style.backgroundImage && style.backgroundImage !== "none") {
        const matches = style.backgroundImage.match(/rgba?\([^)]*\)/gi) ?? [];
        for (const match of matches) {
          const color = parseColor(match);
          if (color && color.a > 0) {
            candidates.push(color);
          }
        }
      }

      const backgroundColor = parseColor(style.backgroundColor);
      if (backgroundColor && backgroundColor.a > 0) {
        candidates.push(backgroundColor);
      }

      return candidates;
    };

    const resolveBackdropCandidates = (element) => {
      if (!element || element === document.documentElement) {
        const body = document.body;
        const bodyBackdrop = body ? getLayerCandidates(body) : [];
        return bodyBackdrop.length > 0 ? bodyBackdrop : [{ r: 255, g: 255, b: 255, a: 1 }];
      }

      const parentCandidates = resolveBackdropCandidates(element.parentElement);
      const layerCandidates = getLayerCandidates(element);

      if (layerCandidates.length === 0) {
        return parentCandidates;
      }

      const combined = [];
      for (const layer of layerCandidates) {
        for (const parent of parentCandidates) {
          combined.push(composite(layer, parent));
        }
      }

      return combined.slice(0, 12);
    };

    const isTextBearing = (element) => {
      const text = extractText(element);
      if (text) {
        return true;
      }

      if (element.matches("button, label, a, th, td, h1, h2, h3, h4, h5, h6, li, summary, option, legend, [role='button'], [role='tab'], [role='gridcell'], [role='columnheader'], [role='rowheader'], [role='link'], [role='status'], [role='alert'], [role='menuitem']")) {
        return true;
      }

      return false;
    };

    for (const element of document.querySelectorAll("body *")) {
      if (!isVisible(element) || !isTextBearing(element)) {
        continue;
      }

      const rect = element.getBoundingClientRect();
      if (rect.width < 1 || rect.height < 1) {
        continue;
      }

      const text = extractText(element);
      if (!text && !element.matches("input, textarea, select")) {
        continue;
      }

      const style = window.getComputedStyle(element);
      const selector = buildSelector(element);
      const backdropCandidates = resolveBackdropCandidates(element);
      const colors = backdropCandidates.map((color) => ({ r: color.r, g: color.g, b: color.b, a: color.a }));

      textElements.push({
        selector,
        text,
        state: currentStateLabel,
        tagName: element.tagName.toLowerCase(),
        fontSize: style.fontSize,
        fontWeight: style.fontWeight,
        color: style.color,
        backgroundCandidates: colors,
        disabled: isDisabled(element),
        rect: {
          width: rect.width,
          height: rect.height
        }
      });
    }

    return textElements;
  }, stateLabel);
}

function isLargeText(fontSize, fontWeight) {
  const size = Number.parseFloat(fontSize);
  const numericWeight = Number.parseInt(fontWeight, 10);
  const boldEnough = Number.isFinite(numericWeight) ? numericWeight >= 700 : String(fontWeight).toLowerCase() === "bold";
  return size >= 24 || (size >= 18.6667 && boldEnough);
}

function classifyViolation(entry) {
  const foreground = parseColor(entry.color);
  if (!foreground) {
    return null;
  }

  const candidateRatios = entry.backgroundCandidates.map((candidate) => contrastRatio(foreground, candidate));
  const ratio = Math.min(...candidateRatios);
  const threshold = entry.disabled ? 0 : isLargeText(entry.fontSize, entry.fontWeight) ? 3 : 4.5;
  let status = "pass";

  if (entry.disabled) {
    status = ratio < 2 ? "onwaarneembaar" : "disabled-exempt";
  } else if (ratio < threshold) {
    status = "fail";
  }

  return {
    ...entry,
    foreground,
    effectiveBackground: entry.backgroundCandidates[0] ?? { r: 255, g: 255, b: 255, a: 1 },
    ratio,
    threshold,
    status
  };
}

function matchesAllowlist(violation, allowlist) {
  return allowlist.some((entry) => {
    if (entry.theme && entry.theme !== violation.theme) {
      return false;
    }

    if (entry.route && entry.route !== violation.route) {
      return false;
    }

    if (entry.state && entry.state !== violation.state) {
      return false;
    }

    if (entry.selectorContains && !violation.selector.includes(entry.selectorContains)) {
      return false;
    }

    if (entry.textContains && !violation.text.includes(entry.textContains)) {
      return false;
    }

    return true;
  });
}

function buildReport(themes, results, violations, allowlist, options) {
  const lines = [];
  lines.push("# Contrast Sweep");
  lines.push("");
  lines.push("Generated by `npm run contrast:sweep`. The sweep measures visible text-bearing DOM nodes in the demo app across all discovered theme variants and routes.");
  lines.push("");
  lines.push("## Profile");
  lines.push("");
  lines.push(`- Themes: ${options.themes.length ? options.themes.join(", ") : "all"}`);
  lines.push(`- Variants: ${options.variants.join(", ")}`);
  lines.push(`- Routes: ${options.routes.length ? options.routes.join(", ") : "all"}`);
  lines.push("");
  lines.push("## Route Evidence Matrix");
  lines.push("");
  lines.push("| Theme Variant | Route | Visible text nodes | Violations | Allowlisted | Effective violations | Status |");
  lines.push("|---|---|---:|---:|---:|---:|---|");

  const baseStateRows = results.filter((item) => item.state === item.route);
  const groupedEvidence = new Map();

  for (const row of baseStateRows) {
    const key = `${row.theme}|${row.route}`;
    if (!groupedEvidence.has(key)) {
      groupedEvidence.set(key, {
        theme: row.theme,
        route: row.route,
        total: 0,
        violations: 0,
        allowlisted: 0,
        effective: 0
      });
    }

    const bucket = groupedEvidence.get(key);
    bucket.total += 1;

    if (row.status === "fail" || row.status === "onwaarneembaar") {
      bucket.violations += 1;
      if (matchesAllowlist(row, allowlist)) {
        bucket.allowlisted += 1;
      } else {
        bucket.effective += 1;
      }
    }
  }

  const evidenceRows = [...groupedEvidence.values()]
    .sort((left, right) => {
      const themeCompare = left.theme.localeCompare(right.theme);
      return themeCompare !== 0 ? themeCompare : left.route.localeCompare(right.route);
    });

  if (evidenceRows.length === 0) {
    lines.push("| No evaluated route rows | - | - | - | - | - | - |");
  } else {
    for (const row of evidenceRows) {
      const status = row.effective === 0 ? "Pass" : "Fail";
      lines.push(`| ${row.theme} | ${row.route} | ${row.total} | ${row.violations} | ${row.allowlisted} | ${row.effective} | ${status} |`);
    }
  }

  lines.push("");
  lines.push("## Summary Matrix");
  lines.push("");
  lines.push("| Theme | Violations | Allowlisted | Effective violations | Status |");
  lines.push("|---|---:|---:|---:|---|");

  for (const theme of themes) {
    const themeViolations = violations.filter((item) => item.theme === theme.lightVariantId || item.theme === theme.darkVariantId);
    const allowed = themeViolations.filter((item) => matchesAllowlist(item, allowlist)).length;
    const effective = themeViolations.length - allowed;
    const status = effective === 0 ? "Pass" : "Fail";
    lines.push(`| ${theme.name} | ${themeViolations.length} | ${allowed} | ${effective} | ${status} |`);
  }

  lines.push("");

  for (const theme of themes) {
    lines.push(`## ${theme.displayName}`);
    lines.push("");
    lines.push("| Element | Route | State | FG | Effective BG | Ratio | Threshold | Status | Allowlisted | Reason |");
    lines.push("|---|---|---|---|---|---:|---:|---|---|---|");

    const themeViolations = violations.filter((item) => item.theme === theme.lightVariantId || item.theme === theme.darkVariantId);
    if (themeViolations.length === 0) {
      lines.push("| No violations | - | - | - | - | - | - | Pass | - | - |");
      lines.push("");
      continue;
    }

    for (const violation of themeViolations) {
      const allowlisted = matchesAllowlist(violation, allowlist);
      const allowReason = allowlisted ? allowlist.find((entry) => {
        if (entry.theme && entry.theme !== violation.theme) {
          return false;
        }
        if (entry.route && entry.route !== violation.route) {
          return false;
        }
        if (entry.state && entry.state !== violation.state) {
          return false;
        }
        if (entry.selectorContains && !violation.selector.includes(entry.selectorContains)) {
          return false;
        }
        if (entry.textContains && !violation.text.includes(entry.textContains)) {
          return false;
        }
        return true;
      })?.reason ?? "" : "";

      lines.push(
        `| ${violation.selector} :: ${violation.text.replace(/\|/g, "\\|")} | ${violation.route} | ${violation.state} | ${formatColor(violation.foreground)} | ${formatColor(violation.effectiveBackground)} | ${violation.ratio.toFixed(2)} | ${violation.threshold.toFixed(1)} | ${violation.status} | ${allowlisted ? "Yes" : "No"} | ${allowReason.replace(/\|/g, "\\|")} |`
      );
    }

    lines.push("");
  }

  if (allowlist.length > 0) {
    lines.push("## Allowlist");
    lines.push("");
    lines.push("| Theme | Route | State | Selector contains | Text contains | Reason | Status |");
    lines.push("|---|---|---|---|---|---|---|");

    for (const entry of allowlist) {
      lines.push(`| ${entry.theme ?? "*"} | ${entry.route ?? "*"} | ${entry.state ?? "*"} | ${entry.selectorContains ?? "*"} | ${entry.textContains ?? "*"} | ${entry.reason ?? ""} | ${entry.status ?? "active"} |`);
    }
    lines.push("");
  }

  lines.push("## Notes");
  lines.push("");
  lines.push("- Disabled elements are tracked separately. They are exempt from SC 1.4.3, but the sweep still flags anything below a 2:1 floor as onwaarneembaar.");
  lines.push("- Gradient and glass backgrounds are evaluated from their rendered color stops, with the worst sampled stop used for reporting.");
  lines.push("- State checks run on representative buttons, navigation, and input routes with hover/focus where the page exposes a cheap interaction target.");

  return `${lines.join("\n")}\n`;
}

function buildViolationKey(violation) {
  return [violation.theme, violation.route, violation.state, violation.selector, violation.text].join("|");
}

function parseCliArgs(argv) {
  const args = {};
  for (const token of argv) {
    if (!token.startsWith("--")) {
      continue;
    }

    const raw = token.slice(2);
    const equalsIndex = raw.indexOf("=");
    if (equalsIndex < 0) {
      args[raw] = "true";
      continue;
    }

    args[raw.slice(0, equalsIndex)] = raw.slice(equalsIndex + 1);
  }

  const toList = (value) => (value ? value.split(",").map((item) => item.trim()).filter(Boolean) : []);
  const toInt = (value, fallback = null) => {
    if (value === undefined) {
      return fallback;
    }

    const parsed = Number.parseInt(value, 10);
    return Number.isFinite(parsed) ? parsed : fallback;
  };

  const shardRaw = args.shard ?? "";
  const shardMatch = shardRaw.match(/^(\d+)\/(\d+)$/);

  return {
    resume: args.resume === "true" || args.resume === "1",
    checkpointPath: args.checkpoint ? path.resolve(repoRoot, args.checkpoint) : checkpointDefaultPath,
    themes: toList(args.themes),
    variants: toList(args.variants).map((item) => item.toLowerCase()).filter((item) => item === "light" || item === "dark"),
    routes: toList(args.routes),
    maxThemes: toInt(args["max-themes"]),
    maxRoutes: toInt(args["max-routes"]),
    stopAfter: toInt(args["stop-after"]),
    checkpointEvery: Math.max(1, toInt(args["checkpoint-every"], 5)),
    shardIndex: shardMatch ? Number.parseInt(shardMatch[1], 10) : null,
    shardCount: shardMatch ? Number.parseInt(shardMatch[2], 10) : null
  };
}

function normalizeVariants(options) {
  if (options.variants.length === 0) {
    options.variants = ["light"];
  }
}

function selectThemes(themes, options) {
  if (!options.themes.length) {
    return options.maxThemes ? themes.slice(0, options.maxThemes) : themes;
  }

  const requested = new Set(options.themes.map((item) => item.toLowerCase()));
  const filtered = themes.filter((theme) =>
    requested.has(theme.name.toLowerCase())
    || requested.has(theme.lightVariantId.toLowerCase())
    || requested.has(theme.darkVariantId.toLowerCase()));

  return options.maxThemes ? filtered.slice(0, options.maxThemes) : filtered;
}

function selectRoutes(routes, options) {
  if (!options.routes.length) {
    return options.maxRoutes ? routes.slice(0, options.maxRoutes) : routes;
  }

  const requested = new Set(options.routes);
  const filtered = routes.filter((item) => requested.has(item.route));
  return options.maxRoutes ? filtered.slice(0, options.maxRoutes) : filtered;
}

function applyShard(combos, options) {
  if (!options.shardIndex || !options.shardCount || options.shardCount < 2) {
    return combos;
  }

  const index = options.shardIndex;
  const total = options.shardCount;
  if (index < 1 || index > total) {
    throw new Error(`Invalid --shard value '${index}/${total}'. Index must be between 1 and shard count.`);
  }

  return combos.filter((_, comboIndex) => comboIndex % total === index - 1);
}

function comboKey(combo) {
  return `${combo.themeVariantId}|${combo.route}`;
}

function buildCombos(themes, routes, options) {
  const combos = [];
  for (const theme of themes) {
    for (const routeInfo of routes) {
      if (options.variants.includes("light")) {
        combos.push({
          themeName: theme.name,
          themeVariantId: theme.lightVariantId,
          route: routeInfo.route,
          sourceFile: routeInfo.filePath
        });
      }

      if (options.variants.includes("dark")) {
        combos.push({
          themeName: theme.name,
          themeVariantId: theme.darkVariantId,
          route: routeInfo.route,
          sourceFile: routeInfo.filePath
        });
      }
    }
  }

  return combos;
}

async function loadCheckpoint(checkpointPath) {
  return readJson(checkpointPath, null);
}

async function writeCheckpoint(checkpointPath, checkpoint) {
  await fs.mkdir(path.dirname(checkpointPath), { recursive: true });
  await fs.writeFile(checkpointPath, JSON.stringify(checkpoint, null, 2), "utf8");
}

async function runStateProbe(page, route, themeVariantId) {
  const entries = [];

  if (route !== "/components/buttons" && route !== "/catalog/buttons" && route !== "/components/layout" && route !== "/components/forms/text-field") {
    return entries;
  }

  const button = page.getByRole("button").first();
  const navigationLink = page.getByRole("link").first();
  const textInput = page.locator("input, textarea").first();

  const probes = [
    { label: "hover", action: async () => { if (await button.count()) await button.hover({ force: true }); } },
    { label: "focus-button", action: async () => { if (await button.count()) await button.focus(); } },
    { label: "focus-input", action: async () => { if (await textInput.count()) await textInput.focus(); } },
    { label: "hover-nav", action: async () => { if (await navigationLink.count()) await navigationLink.hover({ force: true }); } },
  ];

  for (const probe of probes) {
    try {
      await probe.action();
      await delay(150);
      const candidates = await collectCandidates(page, `${route} :: ${probe.label}`);
      entries.push(...candidates.map((candidate) => ({
        ...candidate,
        theme: themeVariantId,
        route,
        state: probe.label,
        isProbe: true
      })));
    } catch {
      // Ignore representative probes that are not available on this route.
    }
  }

  return entries;
}

async function captureRoute(page, themeVariantId, route, stateLabel, beforeCapture) {
  await page.goto(demoBaseUrl, { waitUntil: "networkidle" });
  await page.evaluate((variantId) => {
    localStorage.setItem("agt-ui-theme", variantId);
    document.documentElement.setAttribute("data-agt-theme", variantId);
  }, themeVariantId);

  await page.goto(`${demoBaseUrl}${route}`, { waitUntil: "networkidle" });

  if (beforeCapture) {
    await beforeCapture(page);
  }

  await delay(250);

  const candidates = await collectCandidates(page, stateLabel);
  const stateCandidates = await runStateProbe(page, route, themeVariantId);
  return [...candidates, ...stateCandidates];
}

async function main() {
  const options = parseCliArgs(process.argv.slice(2));
  normalizeVariants(options);
  const themes = await getThemes();
  const routes = await getRoutes();
  const allowlist = await readJson(allowlistPath, []);

  if (!Array.isArray(allowlist)) {
    throw new Error("Contrast allowlist must be a JSON array.");
  }

  await fs.mkdir(path.dirname(reportPath), { recursive: true });

  const selectedThemes = selectThemes(themes, options);
  const selectedRoutes = selectRoutes(routes, options);
  let plannedCombos = buildCombos(selectedThemes, selectedRoutes, options);
  plannedCombos = applyShard(plannedCombos, options);

  if (plannedCombos.length === 0) {
    throw new Error("No sweep combinations selected. Adjust --themes/--routes/--shard filters.");
  }

  let checkpoint = {
    version: 1,
    generatedAtUtc: new Date().toISOString(),
    options,
    completedCombos: [],
    results: []
  };

  if (options.resume) {
    const loaded = await loadCheckpoint(options.checkpointPath);
    if (loaded && Array.isArray(loaded.completedCombos) && Array.isArray(loaded.results)) {
      checkpoint = loaded;
      console.log(`Loaded checkpoint with ${checkpoint.completedCombos.length} completed combinations.`);
    }
  }

  const completedKeys = new Set(checkpoint.completedCombos);
  const results = Array.isArray(checkpoint.results) ? checkpoint.results : [];
  const combos = plannedCombos.filter((combo) => !completedKeys.has(comboKey(combo)));
  const hadPendingWork = combos.length > 0;

  console.log(`Planned combinations: ${plannedCombos.length}. Pending: ${combos.length}.`);

  let demoProcess = null;
  let externalDemo = false;
  let stopRequested = false;
  let stopReason = null;

  const onInterrupt = () => {
    stopRequested = true;
    stopReason = "signal";
    console.warn("Interrupt received. Finishing current combination and writing checkpoint...");
  };
  process.once("SIGINT", onInterrupt);
  process.once("SIGTERM", onInterrupt);

  if (await isPortOpen("127.0.0.1", 5079)) {
    externalDemo = true;
    console.log("Port 5079 already in use. Reusing existing demo instance.");
  } else {
    console.log("Starting demo app...");
    demoProcess = startDemo();
  }

  try {
    await waitForPort("127.0.0.1", 5079, 60000);

    const browser = await chromium.launch({ headless: true });
    const context = await browser.newContext({ viewport });
    const page = await context.newPage();

    const specialRoutes = new Map([
      ["/app/werkorders", async (currentPage) => {
        const detailButton = currentPage.getByRole("button", { name: /Details/i }).first();
        if (await detailButton.count()) {
          await detailButton.click();
          await currentPage.waitForSelector(".rz-dialog, [role='dialog']", { timeout: 10000 });
        }
      }]
    ]);

    let executed = 0;
    for (const combo of combos) {
      if (stopRequested) {
        break;
      }

      const stateLabel = combo.route;
      console.log(`Sweeping ${combo.themeVariantId} @ ${combo.route}`);
      const beforeCapture = specialRoutes.get(combo.route);
      const entries = await captureRoute(page, combo.themeVariantId, combo.route, stateLabel, beforeCapture);
      for (const entry of entries) {
        results.push({
          ...entry,
          theme: combo.themeVariantId,
          route: combo.route,
          sourceFile: combo.sourceFile
        });
      }

      executed++;
      console.log(`  collected ${entries.length} visible text elements`);

      const currentKey = comboKey(combo);
      completedKeys.add(currentKey);
      checkpoint.completedCombos = [...completedKeys];
      checkpoint.results = results;
      checkpoint.generatedAtUtc = new Date().toISOString();

      if (executed % options.checkpointEvery === 0 || stopRequested) {
        await writeCheckpoint(options.checkpointPath, checkpoint);
      }

      if (options.stopAfter && executed >= options.stopAfter) {
        console.log(`Stopping early after ${executed} combinations due to --stop-after.`);
        stopRequested = true;
        stopReason = "limit";
        break;
      }
    }

    await writeCheckpoint(options.checkpointPath, checkpoint);

    if (stopRequested) {
      console.log(`Checkpoint saved to ${toWorkspacePath(path.relative(repoRoot, options.checkpointPath))}. Resume with --resume=true.`);
    }

    const evaluated = results
      .map((entry) => classifyViolation(entry))
      .filter(Boolean);

    const violations = evaluated.filter((entry) => entry.status === "fail" || entry.status === "onwaarneembaar");
    const unallowlistedViolations = violations.filter((violation) => !matchesAllowlist(violation, allowlist));

    const report = buildReport(selectedThemes, evaluated, violations, allowlist, options);
    await fs.writeFile(reportPath, report, "utf8");
    await fs.writeFile(path.join(repoRoot, "eng", "contrast-sweep", "contrast-sweep-results.json"), JSON.stringify({ generatedAtUtc: new Date().toISOString(), viewport, total: evaluated.length, violations }, null, 2), "utf8");

    await browser.close();

    console.log(`Contrast sweep complete. Evaluated ${evaluated.length} elements, found ${violations.length} violations (${unallowlistedViolations.length} unallowlisted).`);

    if (!stopRequested && hadPendingWork && unallowlistedViolations.length > 0) {
      process.exitCode = 1;
    }

    if (stopReason === "signal") {
      process.exitCode = 130;
    }
  } finally {
    process.removeListener("SIGINT", onInterrupt);
    process.removeListener("SIGTERM", onInterrupt);

    if (demoProcess && !externalDemo) {
      console.log("Stopping demo app...");
      demoProcess.kill("SIGTERM");
      await delay(600);
      if (!demoProcess.killed) {
        demoProcess.kill("SIGKILL");
      }
    }
  }
}

main().catch((error) => {
  console.error(error);
  process.exitCode = 1;
});