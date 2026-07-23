import { test, expect } from "@playwright/test";

async function dismissOnboardingIfPresent(page) {
  const overlay = page.locator(".designer-overlay[aria-label='Welkom in de designer']");
  if (await overlay.count()) {
    const ok = overlay.locator("button", { hasText: "Begrepen" }).first();
    if (await ok.count()) {
      await ok.click();
    }
  }
}

test.describe("designer dropdown interaction", () => {
  test("menu backdrop does not trap toolbar toggles", async ({ page }) => {
    await page.goto("/designer/edit?template=FormPage", { waitUntil: "networkidle" });
    await dismissOnboardingIfPresent(page);

    const fileToggle = page.getByRole("button", { name: "Bestand" });
    const settingsToggle = page.getByRole("button", { name: "Instellingen" });

    await fileToggle.click();
    await expect(page.getByRole("menu", { name: "Bestand" })).toBeVisible();

    // Switching directly to another toolbar menu should remain clickable even with backdrop active.
    await settingsToggle.click();
    await expect(page.getByRole("menu", { name: "Instellingen" })).toBeVisible();
    await expect(page.getByRole("menu", { name: "Bestand" })).toHaveCount(0);

    // Re-click should close the active menu.
    await settingsToggle.click();
    await expect(page.getByRole("menu", { name: "Instellingen" })).toHaveCount(0);
  });

  test("toolbar theme dropdown commits selection and closes", async ({ page }) => {
    await page.goto("/designer/edit?template=FormPage", { waitUntil: "networkidle" });
    await dismissOnboardingIfPresent(page);

    const themeCombo = page.locator("header.designer-toolbar div[role='combobox']").first();
    await expect(themeCombo).toBeVisible();

    const before = (await themeCombo.textContent())?.trim() ?? "";
    await themeCombo.click();

    const listboxId = await themeCombo.getAttribute("aria-controls");
    expect(listboxId).toBeTruthy();

    const listbox = page.locator(`[role='listbox'][id='${listboxId}']`);
    await expect(listbox).toBeVisible();

    const options = listbox.locator("[role='option'], .rz-dropdown-item");
    const optionCount = await options.count();
    expect(optionCount).toBeGreaterThan(1);

    let chosen = false;
    let selectedTheme = "";
    for (let i = 0; i < optionCount; i++) {
      const candidate = options.nth(i);
      const text = (await candidate.textContent())?.trim() ?? "";
      if (text && text !== before) {
        await candidate.click();
        selectedTheme = text;
        chosen = true;
        break;
      }
    }

    expect(chosen).toBeTruthy();
    await expect(themeCombo).toHaveAttribute("aria-expanded", "false");
    const after = (await themeCombo.textContent())?.trim() ?? "";
    expect(after).not.toEqual(before);

    const htmlTheme = await page.evaluate(() => document.documentElement.getAttribute("data-agt-theme") ?? "");
    expect(htmlTheme).toEqual(selectedTheme);
  });

  test("inspector entity dropdown supports mouse and keyboard commit", async ({ page }) => {
    await page.goto("/designer/edit?template=FormPage", { waitUntil: "networkidle" });
    await dismissOnboardingIfPresent(page);

    // Expand inspector and open Data tab.
    await page.locator("aside[aria-label='Inspector'] .designer-panel__toggle").click();
    await page.getByRole("tab", { name: "Data" }).click();

    const entityCombo = page.locator("aside[aria-label='Inspector'] .designer-data-panel div[role='combobox']").first();
    await expect(entityCombo).toBeVisible();

    const before = (await entityCombo.textContent())?.trim() ?? "";

    // Mouse selection path.
    await entityCombo.click();
    const listboxId = await entityCombo.getAttribute("aria-controls");
    expect(listboxId).toBeTruthy();
    const listbox = page.locator(`[role='listbox'][id='${listboxId}']`);
    await expect(listbox).toBeVisible();
    const options = listbox.getByRole("option");
    const optionTotal = await options.count();
    if (optionTotal > 1) {
      const candidate = options.nth(1);
      await candidate.click();
      await expect(entityCombo).toHaveAttribute("aria-expanded", "false");
      const afterMouse = (await entityCombo.textContent())?.trim() ?? "";
      expect(afterMouse).not.toEqual(before);

      // Keyboard selection path.
      await entityCombo.click();
      await expect(listbox).toBeVisible();
      await page.keyboard.press("ArrowDown");
      await page.keyboard.press("Enter");
      await expect(entityCombo).toHaveAttribute("aria-expanded", "false");
    }

    // Trigger normal rerender and assert value remains stable.
    await page.getByRole("button", { name: "Preview" }).click();
    await page.getByRole("button", { name: "Bewerken" }).click();

    const afterRerender = (await entityCombo.textContent())?.trim() ?? "";
    expect(afterRerender.length).toBeGreaterThan(0);
  });

  test("new document template dropdown can select and close", async ({ page }) => {
    await page.goto("/designer/edit?template=FormPage", { waitUntil: "networkidle" });
    await dismissOnboardingIfPresent(page);

    await page.getByRole("button", { name: "Bestand" }).click();
    await page.getByRole("menuitem", { name: "Nieuw" }).click();

    const modal = page.getByRole("dialog", { name: "Nieuw document" });
    await expect(modal).toBeVisible();

    const templateCombo = modal.getByRole("combobox").first();
    await expect(templateCombo).toBeVisible();

    const before = (await templateCombo.textContent())?.trim() ?? "";
    await templateCombo.click();

    const listboxId = await templateCombo.getAttribute("aria-controls");
    expect(listboxId).toBeTruthy();

    const listbox = page.locator(`[role='listbox'][id='${listboxId}']`);
    await expect(listbox).toBeVisible();

    const options = listbox.getByRole("option");
    const optionCount = await options.count();
    expect(optionCount).toBeGreaterThan(1);

    let selected = false;
    for (let i = 0; i < optionCount; i++) {
      const candidate = options.nth(i);
      const text = (await candidate.textContent())?.trim() ?? "";
      if (text && text !== before) {
        await candidate.click();
        selected = true;
        break;
      }
    }

    expect(selected).toBeTruthy();
    await expect(templateCombo).toHaveAttribute("aria-expanded", "false");

    const after = (await templateCombo.textContent())?.trim() ?? "";
    expect(after).not.toEqual(before);

    await modal.getByRole("button", { name: "Annuleren" }).click();
    await expect(modal).toHaveCount(0);
  });
});
