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

        expect(challengeField).toHaveAttribute(
            "data-selected",
            InteractionLevel.EVERYONE.toString(),
        );
        expect(chatField).toHaveAttribute("data-selected", "true");

        expect(screen.getByTestId("submitFormButton")).toBeInTheDocument();
    });

    it("should allow preferences to be submitted", async () => {
        const user = userEvent.setup();
        render(<PrivacyForm initialPreferences={preferencesMock} />);

        await user.click(
            screen.getByTestId(`selector-${InteractionLevel.STARRED}`),
        );
        await user.click(screen.getByTestId("selector-false")); // showChat: false

        await user.click(screen.getByTestId("submitFormButton"));

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

        await user.click(
            screen.getByTestId(`selector-${InteractionLevel.STARRED}`),
        );
        await user.click(screen.getByTestId("submitFormButton"));

        const status = await screen.findByText("Failed to update preferences");
        expect(status).toBeInTheDocument();
    });

    it("should render all options for challengePreference and showChat", () => {
        render(<PrivacyForm initialPreferences={preferencesMock} />);

        const challengeValues = [
            InteractionLevel.NO_ONE,
            InteractionLevel.STARRED,
            InteractionLevel.LOGGED_IN,
            InteractionLevel.EVERYONE,
        ];

        challengeValues.forEach((value) => {
            expect(screen.getByTestId(`selector-${value}`)).toBeInTheDocument();
        });

        const chatValues = [true, false];

        chatValues.forEach((value) => {
            expect(screen.getByTestId(`selector-${value}`)).toBeInTheDocument();
        });
    });
});
