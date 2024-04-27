import type { Locator, Page } from "@playwright/test";
import { AuthPage } from "./authPage";

import { DEFAULT_USERNAME, DEFAULT_PASSWORD } from "../../constants";

export class LoginPage extends AuthPage {
    private readonly usernameField: Locator;
    private readonly passwordField: Locator;

    constructor(protected readonly page: Page) {
        super(page);

        this.usernameField = page.getByPlaceholder("Username");
        this.passwordField = page.getByPlaceholder("Password");
    }

    async goto() {
        await this.page.goto("/login");
    }

    async fillForm(
        username: string = DEFAULT_USERNAME,
        password: string = DEFAULT_PASSWORD
    ) {
        await this.usernameField.fill(username);
        await this.passwordField.fill(password);
    }

    async login(
        username: string = DEFAULT_USERNAME,
        password: string = DEFAULT_PASSWORD
    ) {
        await this.goto();

        await this.fillForm(username, password);
        await this.submit();

        // Wait until we are redirected to the home page after login
        await this.page.waitForURL("/");
    }
}
