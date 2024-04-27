import { test as base } from "@playwright/test";

import { LoginPage } from "./models/pages/auth/loginPage";
import { Navbar } from "./models/components/navbar";

interface Fixtures {
    loginPage: LoginPage;
    navbar: Navbar;
}

export const test = base.extend<Fixtures>({
    loginPage: async ({ page }, use) => {
        await use(new LoginPage(page));
    },

    navbar: async ({ page }, use) => {
        await use(new Navbar(page));
    },
});

export { expect } from "@playwright/test";
