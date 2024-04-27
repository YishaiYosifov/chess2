import { test, expect } from "./fixtures";

test("test", async ({ page }) => {
    await page.goto("/user");
});
