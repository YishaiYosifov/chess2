import { test as setup, expect } from "./fixtures";
import { NavbarItem } from "./models/constants";

const authFile = "e2e-tests/.auth/user.json";
setup("authenticate", async ({ page, loginPage, navbar }) => {
    await loginPage.login();

    expect(page).toHaveURL("/");

    const logoutExists = await navbar.itemExists(NavbarItem.logout);
    const settingsExists = await navbar.itemExists(NavbarItem.settings);
    expect(logoutExists).toBeTruthy();
    expect(settingsExists).toBeTruthy();

    await page.context().storageState({ path: authFile });
});
