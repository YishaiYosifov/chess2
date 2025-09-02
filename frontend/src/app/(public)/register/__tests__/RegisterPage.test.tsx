import { render, screen } from "@testing-library/react";
import { redirect } from "next/navigation";

import { mockNextCookies } from "@/lib/testUtils/mocks/mockCookies";
import constants from "@/lib/constants";
import RegisterPage from "../page";

vi.mock("next/navigation");
vi.mock("next/headers");
vi.mock("next/image");

describe("AuthPage", () => {
    const redirectMock = vi.mocked(redirect);

    it("should render the logo text image", async () => {
        render(await RegisterPage());

        const logoImg = screen.getByAltText(/logo/i);
        expect(logoImg).toBeInTheDocument();
        expect(redirectMock).not.toHaveBeenCalled();
    });

    it("should render Google OAuth button", async () => {
        render(await RegisterPage());

        expect(screen.getByText(/Continue with Google/i)).toBeInTheDocument();
        expect(screen.getByAltText(/Google Icon/i)).toBeInTheDocument();
    });

    it("should render Discord OAuth button", async () => {
        render(await RegisterPage());

        expect(screen.getByText(/Continue with Discord/i)).toBeInTheDocument();
        expect(screen.getByAltText(/Discord Icon/i)).toBeInTheDocument();
    });

    it("should redirect when logged in", async () => {
        mockNextCookies(constants.COOKIES.IS_LOGGED_IN);

        render(await RegisterPage());

        expect(redirectMock).toHaveBeenCalledExactlyOnceWith("/");
    });
});
