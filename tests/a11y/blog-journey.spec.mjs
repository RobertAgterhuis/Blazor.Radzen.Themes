import { test, expect } from "@playwright/test";

async function primeBlogState(page) {
  await page.addInitScript(() => {
    localStorage.setItem("agt-ui-theme", "volt-dark");
    localStorage.setItem("blog-read-mode", "off");
  });
}

async function runBlogJourney(page, viewportName) {
  await page.goto("/blog", { waitUntil: "networkidle" });

  await expect(page.getByRole("heading", { name: /projecten die werken in de praktijk/i })).toBeVisible();

  const readModeToggle = page.locator(".blog-read-toggle").first();
  await expect(readModeToggle).toBeVisible();
  await expect(page.locator("#blog-shell")).toHaveAttribute("data-blog-read", "false");
  await readModeToggle.click();
  await expect(page.locator("#blog-shell")).toHaveAttribute("data-blog-read", "true");
  await readModeToggle.click();
  await expect(page.locator("#blog-shell")).toHaveAttribute("data-blog-read", "false");

  if (viewportName === "mobile") {
    const tabBar = page.locator(".blog-tabbar");
    await expect(tabBar).toBeVisible();
    await expect(tabBar.getByRole("link")).toHaveCount(5);
    const tabHeight = await tabBar.getByRole("link").first().evaluate((element) => element.getBoundingClientRect().height);
    expect(tabHeight).toBeGreaterThanOrEqual(44);
  } else {
    await expect(page.locator(".blog-nav")).toBeVisible();
    await expect(page.locator(".blog-tabbar")).toBeHidden();
  }

  await page.goto("/blog/projecten", { waitUntil: "networkidle" });
  await expect(page.getByRole("heading", { name: /projectreel/i })).toBeVisible();
  await expect(page.locator(".blog-reel__track [data-blog-reel], .blog-reel__track")).toBeVisible();

  await page.locator(".blog-reel__track .blog-project-card").first().click();
  await expect(page.getByRole("heading", { level: 1 })).toContainText(/volt journal shell|project/i);

  await page.goto("/blog/agents", { waitUntil: "networkidle" });
  await expect(page.getByRole("heading", { name: /agent cards/i })).toBeVisible();
  await expect(page.locator("[data-blog-terminal]").first()).toBeVisible();

  await page.goto("/blog/prompts", { waitUntil: "networkidle" });
  await expect(page.getByRole("heading", { name: /dogfood prompt library/i })).toBeVisible();
  await expect(page.locator("text=42-volt-blog-showcase.md").first()).toBeVisible();
  await page.locator("#blog-prompt-search").fill("volt");
  await expect(page.locator(".blog-prompt-card").first()).toBeVisible();
  await expect(page.locator(".blog-prompt-card").first()).toContainText(/volt/i);
  await expect(page.getByRole("button", { name: /kopieer prompt/i })).toBeVisible();

  await page.goto("/blog/skills", { waitUntil: "networkidle" });
  await expect(page.getByRole("heading", { name: /skill matrix/i })).toBeVisible();
  const skillMeterCount = await page.locator(".blog-skill-meter__fill").count();
  expect(skillMeterCount).toBeGreaterThan(10);

  await page.goto("/blog/artikel/van-wrapper-naar-workflow", { waitUntil: "networkidle" });
  await expect(page.locator(".blog-progress")).toBeVisible();
  await expect(page.getByText(/geschatte leestijd/i)).toBeVisible();
  if (viewportName === "desktop") {
    await expect(page.locator(".blog-article-toc")).toBeVisible();
  }

  await page.goto("/blog/over", { waitUntil: "networkidle" });
  await expect(page.getByRole("heading", { name: /compact contactpunt/i })).toBeVisible();
}

test.describe("blog showcase journey", () => {
  test("mobile 360 flow", async ({ page }) => {
    test.setTimeout(90000);
    await page.setViewportSize({ width: 360, height: 800 });
    await primeBlogState(page);
    await runBlogJourney(page, "mobile");
  });

  test("desktop 1440 flow", async ({ page }) => {
    test.setTimeout(90000);
    await page.setViewportSize({ width: 1440, height: 960 });
    await primeBlogState(page);
    await runBlogJourney(page, "desktop");
  });
});
