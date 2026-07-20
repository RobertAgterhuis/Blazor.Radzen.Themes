import { chromium } from "@playwright/test";
import sharp from "sharp";
import fs from "node:fs/promises";
import path from "node:path";
import net from "node:net";
import process from "node:process";
import { spawn } from "node:child_process";
import { fileURLToPath } from "node:url";

const repoRoot = path.resolve(path.dirname(fileURLToPath(import.meta.url)), "..", "..");
const configPath = path.join(repoRoot, "eng", "visual-regression", "visual-regression.config.json");
const baselineDir = path.join(repoRoot, "eng", "visual-regression", "baselines");
const reportDir = path.join(repoRoot, "eng", "visual-regression", "reports");
const baseUrl = "http://127.0.0.1:5079";
const mode = process.argv.includes("--approve") ? "approve" : "test";

function delay(milliseconds) {
  return new Promise((resolve) => setTimeout(resolve, milliseconds));
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

async function loadConfig() {
  const raw = await fs.readFile(configPath, "utf8");
  return JSON.parse(raw);
}

function sanitizeName(value) {
  return value.replace(/[^a-z0-9.-]+/gi, "-").replace(/-+/g, "-").replace(/^-|-$/g, "");
}

async function optimizePng(buffer, outputPath) {
  await sharp(buffer)
    .png({ compressionLevel: 9, palette: true, quality: 85, effort: 10 })
    .toFile(outputPath);
}

async function decodePng(imagePath) {
  return sharp(await fs.readFile(imagePath)).ensureAlpha().raw().toBuffer({ resolveWithObject: true });
}

function createDiffBuffer(baseline, current, width, height, tolerance) {
  const diff = Buffer.alloc(width * height * 4);
  let diffPixels = 0;

  for (let pixel = 0; pixel < width * height; pixel += 1) {
    const offset = pixel * 4;
    const redDelta = Math.abs(baseline[offset] - current[offset]);
    const greenDelta = Math.abs(baseline[offset + 1] - current[offset + 1]);
    const blueDelta = Math.abs(baseline[offset + 2] - current[offset + 2]);
    const changed = Math.max(redDelta, greenDelta, blueDelta) > tolerance;

    if (changed) {
      diffPixels += 1;
      diff[offset] = 220;
      diff[offset + 1] = 40;
      diff[offset + 2] = 40;
      diff[offset + 3] = 255;
    }
    else {
      diff[offset] = 0;
      diff[offset + 1] = 0;
      diff[offset + 2] = 0;
      diff[offset + 3] = 0;
    }
  }

  return { diff, diffPixels };
}

async function renderCapture(page, capture, viewportName, theme) {
  await page.goto(baseUrl, { waitUntil: "networkidle" });
  await page.evaluate((value) => {
    localStorage.setItem("agt-ui-theme", value);
    document.documentElement.setAttribute("data-agt-theme", value);
  }, theme);
  await page.goto(`${baseUrl}${capture.route}`, { waitUntil: "networkidle" });
  await page.addStyleTag({
    content: `
      *, *::before, *::after {
        animation-duration: 0s !important;
        animation-delay: 0s !important;
        animation-iteration-count: 1 !important;
        transition-duration: 0s !important;
        transition-delay: 0s !important;
      }
    `
  });
  await page.evaluate(async () => {
    if (document.fonts && document.fonts.ready) {
      await document.fonts.ready;
    }
  });

  await page.waitForSelector(capture.selector, { state: "visible", timeout: 15000 });
  await delay(900);

  const buffer = await page.screenshot({ type: "png", fullPage: false });
  const fileName = `${sanitizeName(capture.key)}.${viewportName}.${theme}.png`;
  return { buffer, fileName };
}

async function writeManifest(manifest) {
  await fs.writeFile(path.join(reportDir, "manifest.json"), JSON.stringify(manifest, null, 2));
}

async function main() {
  const config = await loadConfig();
  await fs.mkdir(baselineDir, { recursive: true });
  await fs.mkdir(reportDir, { recursive: true });

  let demoProcess = null;
  let externalDemo = false;

  if (await isPortOpen("127.0.0.1", 5079)) {
    externalDemo = true;
    console.log("Port 5079 already in use. Reusing existing demo instance.");
  }
  else {
    console.log("Starting demo app...");
    demoProcess = startDemo();
  }

  try {
    await waitForPort("127.0.0.1", 5079, 60000);

    const browser = await chromium.launch({ headless: true });
    const results = [];
    const failures = [];

    for (const viewport of config.viewports) {
      const context = await browser.newContext({ viewport, reducedMotion: "reduce" });
      await context.addInitScript(() => {
        const originalMatchMedia = window.matchMedia.bind(window);
        window.matchMedia = (query) => {
          if (query.includes("prefers-reduced-motion")) {
            return {
              matches: true,
              media: query,
              onchange: null,
              addListener() {},
              removeListener() {},
              addEventListener() {},
              removeEventListener() {},
              dispatchEvent() {
                return false;
              }
            };
          }

          return originalMatchMedia(query);
        };
      });
      const page = await context.newPage();

      for (const theme of config.themes) {
        for (const capture of config.captures) {
          const { buffer, fileName } = await renderCapture(page, capture, viewport.name, theme);
          const baselinePath = path.join(baselineDir, fileName);
          const reportPath = path.join(reportDir, fileName.replace(/\.png$/, ""));

          if (mode === "approve" || !(await exists(baselinePath))) {
            await optimizePng(buffer, baselinePath);
            results.push({ fileName, status: mode === "approve" ? "updated" : "created" });
            continue;
          }

          const currentPath = path.join(reportPath, "current.png");
          const baselineCopyPath = path.join(reportPath, "baseline.png");
          const diffPath = path.join(reportPath, "diff.png");

          await fs.mkdir(reportPath, { recursive: true });
          await fs.writeFile(currentPath, buffer);
          await fs.copyFile(baselinePath, baselineCopyPath);

          const baselineImage = await decodePng(baselinePath);
          const currentImage = await sharp(buffer).ensureAlpha().raw().toBuffer({ resolveWithObject: true });

          if (baselineImage.info.width !== currentImage.info.width || baselineImage.info.height !== currentImage.info.height) {
            failures.push(`${fileName}: size mismatch ${baselineImage.info.width}x${baselineImage.info.height} != ${currentImage.info.width}x${currentImage.info.height}`);
            continue;
          }

          const tolerance = 6;
          const { diff, diffPixels } = createDiffBuffer(baselineImage.data, currentImage.data, baselineImage.info.width, baselineImage.info.height, tolerance);
          const diffRatio = diffPixels / (baselineImage.info.width * baselineImage.info.height);

          await sharp(diff, { raw: { width: baselineImage.info.width, height: baselineImage.info.height, channels: 4 } })
            .png({ compressionLevel: 9, palette: true, quality: 85, effort: 10 })
            .toFile(diffPath);

          results.push({ fileName, status: diffRatio <= config.threshold ? "ok" : "diff", diffRatio });

          if (diffRatio > config.threshold) {
            failures.push(`${fileName}: ${Math.round(diffRatio * 100000) / 1000}% different (threshold ${(config.threshold * 100).toFixed(2)}%)`);
          }
        }
      }

      await context.close();
    }

    await browser.close();
    await writeManifest({ generatedAtUtc: new Date().toISOString(), mode, threshold: config.threshold, results });

    if (failures.length > 0) {
      console.error("Visual regression failures:");
      for (const failure of failures) {
        console.error(`- ${failure}`);
      }
      process.exitCode = 1;
      return;
    }

    console.log(`Visual regression ${mode} complete. ${results.length} captures processed.`);
  }
  finally {
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

async function exists(filePath) {
  try {
    await fs.stat(filePath);
    return true;
  }
  catch {
    return false;
  }
}

main().catch((error) => {
  console.error(error);
  process.exitCode = 1;
});