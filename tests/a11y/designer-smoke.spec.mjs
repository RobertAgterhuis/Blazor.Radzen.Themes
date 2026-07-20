import { test, expect } from "@playwright/test";

test.describe("designer smoke", () => {
  test("start screen links into editor and renders template", async ({ page }) => {
    await page.goto("/designer", { waitUntil: "networkidle" });

    const templateButton = page.locator(".designer-startscreen__patterns button").first();

    await expect(templateButton).toBeVisible();
    await templateButton.click();

    await expect(page).toHaveURL(/\/designer\/edit\?template=/);
    await expect(page.locator(".designer-toolbar")).toBeVisible();
    await expect(page.locator(".designer-canvas-node").first()).toBeVisible();
  });
});
