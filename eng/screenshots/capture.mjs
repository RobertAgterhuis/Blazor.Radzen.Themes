import { chromium } from "@playwright/test";
import sharp from "sharp";
import fs from "node:fs/promises";
import path from "node:path";
import net from "node:net";
import process from "node:process";
import { spawn } from "node:child_process";
import { fileURLToPath } from "node:url";

const repoRoot = path.resolve(path.dirname(fileURLToPath(import.meta.url)), "..", "..");
const outputDir = path.join(repoRoot, "docs", "assets");
const baseUrl = "http://127.0.0.1:5079";
const maxFileSizeBytes = 300 * 1024;

const requiredCaptures = [
  {
    key: "home-plum-dark",
    route: "/",
    theme: "plum-dark",
    selector: "h2",
    description: "Home hero in plum-dark"
  },
  {
    key: "home-hoth-light",
    route: "/",
    theme: "hoth-light",
    selector: "h2",
    description: "Home hero in hoth-light"
  },
  {
    key: "catalog-buttons-imperial-dark",
    route: "/catalog/buttons",
    theme: "imperial-dark",
    selector: "h1",
    description: "Catalog Buttons in imperial-dark"
  },
  {
    key: "werkorders-dashboard-autotaalglas-light",
    route: "/app/werkorders",
    theme: "autotaalglas-light",
    selector: "h1",
    description: "Werkorders dashboard in autotaalglas-light"
  },
  {
    key: "werkorders-grid-detail-dialog",
    route: "/app/werkorders",
    theme: "autotaalglas-light",
    selector: "table, .rz-datatable-tablewrapper, .rz-grid-table",
    beforeCapture: async (page) => {
      const detailButton = page.getByRole("button", { name: /Details/i }).first();
      await detailButton.waitFor({ state: "visible", timeout: 15000 });
      await detailButton.click();
      await page.waitForSelector('.rz-dialog, [role="dialog"]', { timeout: 10000 });
    },
    description: "Werkorders grid with detail dialog open"
  },
  {
    key: "planning-scheduler-plum-dark",
    route: "/app/planning",
    theme: "plum-dark",
    selector: ".rz-scheduler, text=Planbord",
    description: "Planning scheduler"
  },
  {
    key: "designer-start-plum-dark",
    route: "/designer",
    theme: "plum-dark",
    selector: ".designer-page",
    description: "Designer start screen"
  },
  {
    key: "designer-canvas-plum-dark",
    route: "/designer",
    theme: "plum-dark",
    selector: ".designer-canvas",
    description: "Designer canvas"
  }
];

const familyGalleryCaptures = [
  { key: "family-plum", theme: "plum-dark" },
  { key: "family-ocean", theme: "ocean-dark" },
  { key: "family-dagobah", theme: "dagobah-dark" },
  { key: "family-dathomir", theme: "dathomir-dark" },
  { key: "family-hoth", theme: "hoth-dark" },
  { key: "family-tatooine", theme: "tatooine-dark" },
  { key: "family-imperial", theme: "imperial-dark" },
  { key: "family-autotaalglas", theme: "autotaalglas-light" },
  { key: "family-autotaalglas-contrast", theme: "autotaalglas-contrast-light" },
  { key: "family-autotaalglas-portal", theme: "autotaalglas-portal-light" },
  { key: "family-autotaalglas-mono", theme: "autotaalglas-mono-light" }
].map((item) => ({
  ...item,
  route: "/",
  selector: "h2",
  description: `Family gallery snapshot (${item.theme})`
}));

const allCaptures = [...requiredCaptures, ...familyGalleryCaptures];

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

  child.stdout.on("data", (data) => {
    process.stdout.write(`[demo] ${data}`);
  });
  child.stderr.on("data", (data) => {
    process.stderr.write(`[demo] ${data}`);
  });

  return child;
}

async function optimizePng(buffer, outputPath) {
  await sharp(buffer)
    .png({ compressionLevel: 9, palette: true, quality: 85, effort: 10 })
    .toFile(outputPath);

  const stats = await fs.stat(outputPath);
  if (stats.size > maxFileSizeBytes) {
    await sharp(buffer)
      .png({ compressionLevel: 9, palette: true, quality: 70, effort: 10, colors: 96 })
      .toFile(outputPath);
  }

  const finalStats = await fs.stat(outputPath);
  if (finalStats.size > maxFileSizeBytes) {
    throw new Error(`Capture exceeds ${maxFileSizeBytes} bytes: ${path.basename(outputPath)} (${finalStats.size})`);
  }

  return finalStats.size;
}

function validateNotBlank(rawBuffer, width, height, key) {
  let total = 0;
  let sum = 0;
  const luminances = new Array(width * height);

  for (let i = 0; i < rawBuffer.length; i += 3) {
    const r = rawBuffer[i];
    const g = rawBuffer[i + 1];
    const b = rawBuffer[i + 2];
    const lum = 0.2126 * r + 0.7152 * g + 0.0722 * b;
    luminances[total] = lum;
    sum += lum;
    total += 1;
  }

  const mean = sum / total;
  let variance = 0;
  let nearIdentical = 0;

  for (let i = 0; i < luminances.length; i += 1) {
    const delta = luminances[i] - mean;
    variance += delta * delta;
    if (Math.abs(delta) <= 3) {
      nearIdentical += 1;
    }
  }

  variance /= total;
  const nearIdenticalRatio = nearIdentical / total;

  if (nearIdenticalRatio > 0.95 || variance < 12) {
    throw new Error(
      `Blank-guard failed for ${key}: near-identical=${(nearIdenticalRatio * 100).toFixed(2)}%, variance=${variance.toFixed(2)}`
    );
  }
}

async function captureOne(page, capture) {
  await page.goto(baseUrl, { waitUntil: "networkidle" });
  await page.evaluate((theme) => {
    localStorage.setItem("agt-ui-theme", theme);
    document.documentElement.setAttribute("data-agt-theme", theme);
  }, capture.theme);
  await page.goto(`${baseUrl}${capture.route}`, { waitUntil: "networkidle" });

  const selectorParts = capture.selector.split(",").map((s) => s.trim()).filter(Boolean);
  let selectorReady = false;
  for (const selector of selectorParts) {
    try {
      await page.waitForSelector(selector, { timeout: 8000, state: "visible" });
      selectorReady = true;
      break;
    } catch {
      // Try the next selector.
    }
  }

  if (!selectorReady) {
    throw new Error(`None of the selectors became visible for ${capture.key}: ${capture.selector}`);
  }

  if (capture.beforeCapture) {
    await capture.beforeCapture(page);
  }

  await delay(500);

  const pngBuffer = await page.screenshot({
    type: "png",
    fullPage: false
  });

  const raw = await sharp(pngBuffer).removeAlpha().raw().toBuffer({ resolveWithObject: true });
  validateNotBlank(raw.data, raw.info.width, raw.info.height, capture.key);

  const outputPath = path.join(outputDir, `${capture.key}.png`);
  const bytes = await optimizePng(pngBuffer, outputPath);

  return {
    key: capture.key,
    route: capture.route,
    theme: capture.theme,
    selector: capture.selector,
    bytes,
    output: toWorkspacePath(path.relative(repoRoot, outputPath))
  };
}

async function writeManifest(results) {
  const manifestPath = path.join(outputDir, "captures-manifest.json");
  await fs.writeFile(
    manifestPath,
    JSON.stringify(
      {
        generatedAtUtc: new Date().toISOString(),
        viewport: { width: 1600, height: 900 },
        total: results.length,
        captures: results
      },
      null,
      2
    )
  );
}

async function main() {
  await fs.mkdir(outputDir, { recursive: true });

  let demoProcess = null;
  let externalDemo = false;

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
    const context = await browser.newContext({ viewport: { width: 1600, height: 900 } });
    const page = await context.newPage();

    const results = [];
    for (const capture of allCaptures) {
      console.log(`Capturing ${capture.key} (${capture.theme} @ ${capture.route})`);
      const result = await captureOne(page, capture);
      results.push(result);
      console.log(`  -> ${result.output} (${result.bytes} bytes)`);
    }

    await writeManifest(results);
    await browser.close();

    console.log("Screenshot capture complete.");
    console.log(`Captured ${results.length} images.`);
  } finally {
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
