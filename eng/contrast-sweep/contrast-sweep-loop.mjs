import fs from "node:fs/promises";
import path from "node:path";
import process from "node:process";
import { spawn } from "node:child_process";
import { fileURLToPath } from "node:url";

const repoRoot = path.resolve(path.dirname(fileURLToPath(import.meta.url)), "..", "..");
const sweepScriptPath = path.join(repoRoot, "eng", "contrast-sweep", "contrast-sweep.mjs");
const checkpointPath = path.join(repoRoot, "eng", "contrast-sweep", "contrast-sweep.checkpoint.json");

function parseCliArgs(argv) {
  const parsed = {};
  for (const token of argv) {
    if (!token.startsWith("--")) {
      continue;
    }

    const raw = token.slice(2);
    const equalsIndex = raw.indexOf("=");
    if (equalsIndex < 0) {
      parsed[raw] = "true";
      continue;
    }

    parsed[raw.slice(0, equalsIndex)] = raw.slice(equalsIndex + 1);
  }

  const toInt = (value, fallback) => {
    if (value === undefined) {
      return fallback;
    }

    const parsedValue = Number.parseInt(value, 10);
    return Number.isFinite(parsedValue) ? parsedValue : fallback;
  };

  const forwardedArgs = argv.filter((token) =>
    token.startsWith("--")
    && !token.startsWith("--chunk-size=")
    && !token.startsWith("--max-iterations=")
    && !token.startsWith("--checkpoint-every=")
    && !token.startsWith("--pause-ms="));

  return {
    chunkSize: Math.max(1, toInt(parsed["chunk-size"], 25)),
    maxIterations: Math.max(1, toInt(parsed["max-iterations"], 500)),
    checkpointEvery: Math.max(1, toInt(parsed["checkpoint-every"], 5)),
    pauseMs: Math.max(0, toInt(parsed["pause-ms"], 0)),
    forwardedArgs
  };
}

async function readCompletedCount() {
  try {
    const text = await fs.readFile(checkpointPath, "utf8");
    const parsed = JSON.parse(text);
    return Array.isArray(parsed.completedCombos) ? parsed.completedCombos.length : 0;
  } catch {
    return 0;
  }
}

function runChunk(args) {
  return new Promise((resolve, reject) => {
    const command = process.execPath;
    const child = spawn(command, args, {
      cwd: repoRoot,
      stdio: "inherit",
      env: process.env
    });

    child.once("error", reject);
    child.once("exit", (code) => {
      resolve(code ?? 1);
    });
  });
}

function delay(milliseconds) {
  if (milliseconds <= 0) {
    return Promise.resolve();
  }

  return new Promise((resolve) => setTimeout(resolve, milliseconds));
}

async function main() {
  const options = parseCliArgs(process.argv.slice(2));
  let previousCompleted = await readCompletedCount();

  console.log(`Starting looped contrast sweep with chunk size ${options.chunkSize}.`);

  for (let iteration = 1; iteration <= options.maxIterations; iteration++) {
    console.log(`\n[loop] Iteration ${iteration}/${options.maxIterations}`);

    const chunkArgs = [
      sweepScriptPath,
      "--resume=true",
      `--stop-after=${options.chunkSize}`,
      `--checkpoint-every=${options.checkpointEvery}`,
      ...options.forwardedArgs
    ];

    const exitCode = await runChunk(chunkArgs);
    if (exitCode !== 0) {
      throw new Error(`Chunk run failed with exit code ${exitCode}.`);
    }

    const completed = await readCompletedCount();
    const delta = completed - previousCompleted;
    console.log(`[loop] completed combos: ${completed} (delta ${delta})`);

    if (delta <= 0) {
      console.log("[loop] No new combinations completed. Assuming sweep is finished for the selected scope.");
      return;
    }

    previousCompleted = completed;

    if (delta < options.chunkSize) {
      console.log("[loop] Last chunk was partial. Sweep likely completed for the selected scope.");
      return;
    }

    await delay(options.pauseMs);
  }

  throw new Error(`Reached max iterations (${options.maxIterations}) before completion.`);
}

main().catch((error) => {
  console.error(error?.message ?? error);
  process.exitCode = 1;
});
