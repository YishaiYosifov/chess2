import { render, screen } from "@testing-library/react";

import DailyQuestCardLoggedOut from "../DailyQuestCardLoggedOut";
import constants from "@/lib/constants";

describe("DailyQuestCardLoggedOut", () => {
    it("should render the title", () => {
        render(<DailyQuestCardLoggedOut />);

        expect(screen.getByTestId("dailyQuestTitle")).toHaveTextContent(
            "Daily Quest",
        );
    });

    it("should render the login/register message", () => {
        render(<DailyQuestCardLoggedOut />);

        expect(screen.getByTestId("dailyQuestMessage")).toHaveTextContent(
            /register to start completing daily quests/i,
        );
    });

    it("should link the button to the register page", () => {
        render(<DailyQuestCardLoggedOut />);

        const link = screen.getByTestId("dailyQuestRegisterLink");
        expect(link).toHaveAttribute("href", constants.PATHS.REGISTER);
        expect(link).toHaveTextContent("Register");
    });
});
