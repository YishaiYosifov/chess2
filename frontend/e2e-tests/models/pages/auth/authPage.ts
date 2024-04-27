import type { Locator, Page } from "@playwright/test";

export class AuthPage {
    private readonly submitButton: Locator;

    constructor(protected readonly page: Page) {
        this.submitButton = page.getByTestId("submitFormButton");
    }

    async submit() {
        await this.submitButton.click();
    }
}
