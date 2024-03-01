import { render, screen, waitFor } from "@testing-library/react";
import ProfileSettings from "../ProfileSettings";
import { Mock } from "vitest";

import { createFormRenderer } from "@/lib/utils/testUtils";
import { profileMock } from "@/mockUtils/profileMock";
import countries from "@/data/countries.json";
import { EditableProfile } from "@/client";
import { settingsApi } from "@/lib/apis";
import constants from "@/lib/constants";

vi.mock("@/components/contexts/AuthContext");
vi.mock("@/lib/apis");

describe("ProfileSettings", () => {
    const defaultFieldValues = { countryAlpha3: "USA", about: "a" };
    const renderAndFillForm = createFormRenderer<EditableProfile>(
        <ProfileSettings />,
        defaultFieldValues
    );

    it("should successfully render", () => {
        render(<ProfileSettings />);

        const country = screen.queryByTestId<HTMLSelectElement>(
            "profileSettingsCountry"
        );
        const about = screen.queryByTestId<HTMLTextAreaElement>(
            "profileSettingsAbout"
        );

        expect(country).toBeInTheDocument();
        expect(country?.nodeName.toLowerCase()).toBe("select");
        expect(country?.children.length).toBe(Object.keys(countries).length);
        expect(country?.value).toBe(profileMock.countryAlpha3);

        expect(about).toBeInTheDocument();
        expect(about?.nodeName.toLowerCase()).toBe("textarea");
        expect(about?.maxLength).toBe(300);
        expect(about?.value).toBe(profileMock.about);
    });

    it("should submit the form with updated values", async () => {
        await renderAndFillForm();
    });

    it("should display error message on form submission faliure", () => {
        (settingsApi.updateProfile as Mock).mockRejectedValue(new Error());

        render(<ProfileSettings />);
        screen.getByRole<HTMLFormElement>("form").requestSubmit();

        waitFor(() =>
            expect(
                screen.getByText(constants.GENERIC_ERROR)
            ).toBeInTheDocument()
        );
    });
});
