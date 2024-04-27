import type { Locator, Page } from "@playwright/test";
import { NavbarItem } from "../constants";

export class Navbar {
    private readonly navContainer: Locator;
    constructor(private readonly page: Page) {
        this.navContainer = page.getByTestId("navbar");
    }

    async goToPage(item: NavbarItem): Promise<void> {
        const button = this.navContainer.locator(`[href="${item}"]`);
        await button.click();
    }

    async itemExists(item: NavbarItem): Promise<boolean> {
        return await this.page.isVisible(`[href="${item}"]`);
    }
}
