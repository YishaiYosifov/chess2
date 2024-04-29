import { test, expect } from "./fixtures";

test("test", async ({ page, browser }) => {
    const context = await browser.newContext();
    const page2 = await context.newPage();

    await page.goto("/user");
    await page2.goto("/settings");
    await context.close();
});
