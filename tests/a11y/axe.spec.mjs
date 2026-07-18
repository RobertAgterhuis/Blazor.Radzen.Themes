import { test, expect } from "@playwright/test";
import AxeBuilder from "@axe-core/playwright";

const routes = [
  "/",
  "/components/theme",
  "/components/buttons",
  "/components/layout",
  "/components/layout/sidebar-layout",
  "/components/feedback",
  "/components/feedback/empty-state",
  "/components/feedback/loading-panel",
  "/components/feedback/confirm-dialog",
  "/components/forms/text-field",
  "/components/forms/numeric-field",
  "/components/forms/dropdown",
  "/components/forms/date-picker",
  "/components/forms/form-actions",
  "/components/data/grid",
  "/catalog",
  "/catalog/buttons",
  "/catalog/text-inputs",
  "/catalog/selection-inputs",
  "/catalog/pickers",
  "/catalog/forms",
  "/catalog/validators",
  "/catalog/data",
  "/catalog/scheduling",
  "/catalog/navigation",
  "/catalog/overlays",
  "/catalog/layout",
  "/catalog/feedback",
  "/catalog/charts",
  "/catalog/gauges",
  "/catalog/display",
  "/catalog/embed"
];

for (const route of routes) {
  for (const theme of ["light", "dark"]) {
    test(`axe ${theme} ${route}`, async ({ page }) => {
      await page.goto(route, { waitUntil: "domcontentloaded" });

      await page.evaluate((activeTheme) => {
        document.documentElement.setAttribute("data-agt-theme", activeTheme);
        document.querySelector(".demo-theme-root")?.setAttribute("data-agt-theme", activeTheme);
      }, theme);

      await expect
        .poll(async () => {
          const title = await page.title();
          return title.trim().length;
        })
        .toBeGreaterThan(0);

      const builder = new AxeBuilder({ page })
        .withTags(["wcag2a", "wcag2aa", "wcag22aa"])
        .exclude(".rz-state-disabled.rz-button-icon-only")
        .exclude(".rz-colorpicker")
        .exclude(".demo-body");

      if (route === "/catalog/embed") {
        builder.exclude(".gm-style").exclude("iframe");
      }

      const axeResults = await builder.analyze();

      expect(axeResults.violations, `${theme} ${route} has axe violations`).toEqual([]);
    });
  }
}
