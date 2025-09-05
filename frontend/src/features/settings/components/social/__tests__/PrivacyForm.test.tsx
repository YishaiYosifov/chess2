import { render, screen } from "@testing-library/react";

import {
    getPreferences,
    InteractionLevel,
    Preferences,
    setPreferences,
} from "@/lib/apiClient";
import PrivacyForm from "../PrivacyForm";
import userEvent from "@testing-library/user-event";

vi.mock("@/lib/apiClient/definition");

describe("PrivacyForm", () => {
    const preferencesMock: Preferences = {
        challengePreference: InteractionLevel.EVERYONE,
        showChat: true,
    };

    const getPreferencesMock = vi.mocked(getPreferences);
    const setPreferencesMock = vi.mocked(setPreferences);

    beforeEach(() => {
        getPreferencesMock.mockResolvedValue({
            data: preferencesMock,
            response: new Response(),
        });
        setPreferencesMock.mockResolvedValue({
            data: undefined,
            response: new Response(),
        });
    });

    it("should render the form with current preferences", () => {
        render(<PrivacyForm initialPreferences={preferencesMock} />);

        const challengeField = screen.getByTestId("challengePreference");
        const chatField = screen.getByTestId("showChat");

        // disabled == selected
        expect(
            challengeField.querySelector("button[disabled]")?.textContent,
        ).toBe("Always");
        expect(chatField.querySelector("button[disabled]")?.textContent).toBe(
            "Yes",
        );

        expect(screen.getByTestId("submitFormButton")).toBeInTheDocument();
    });

    it("should allow preferences to be submitted", async () => {
        const user = userEvent.setup();
        render(<PrivacyForm initialPreferences={preferencesMock} />);

        const challengeField = screen.getByTestId("challengePreference");
        const chatField = screen.getByTestId("showChat");
        const submitButton = screen.getByTestId("submitFormButton");

        const challengeButtons = challengeField.querySelectorAll("button");
        await user.click(challengeButtons[1]); // only Stars

        const chatButtons = chatField.querySelectorAll("button");
        await user.click(chatButtons[1]); // no

        await user.click(submitButton);

        expect(setPreferencesMock).toHaveBeenCalledWith({
            body: {
                challengePreference: InteractionLevel.STARRED,
                showChat: false,
            },
        });
    });

    it("should display status message on API failure", async () => {
        setPreferencesMock.mockResolvedValueOnce({
            data: undefined,
            response: new Response(),
            error: { errors: [], extensions: {} },
        });

        const user = userEvent.setup();
        render(<PrivacyForm initialPreferences={preferencesMock} />);

        const challengeField = screen.getByTestId("challengePreference");
        const submitButton = screen.getByTestId("submitFormButton");

        const challengeButtons = challengeField.querySelectorAll("button");
        await user.click(challengeButtons[1]);
        await user.click(submitButton);

        const status = await screen.findByText("Failed to update preferences");
        expect(status).toBeInTheDocument();
    });
});
