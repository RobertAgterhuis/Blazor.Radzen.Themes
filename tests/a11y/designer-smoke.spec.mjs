import { test, expect } from "@playwright/test";

test.describe("designer smoke", () => {
  test("drag palette item to canvas renders node", async ({ page }) => {
    await page.goto("/designer", { waitUntil: "networkidle" });

    const paletteItem = page.locator(".designer-palette-item").first();
    const targetDropZone = page.locator(".designer-dropzone--root").first();

    await expect(paletteItem).toBeVisible();
    await expect(targetDropZone).toBeVisible();

    await paletteItem.dragTo(targetDropZone);

    await expect(page.locator(".designer-canvas-node").nth(1)).toBeVisible();
  });
});
