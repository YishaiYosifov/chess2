import { render, screen, waitFor } from "@testing-library/react";
import { Mock } from "vitest";

import { setAuthedProfile } from "@/components/contexts/__mocks__/AuthContext";
import { createFormRenderer } from "@/lib/utils/testUtils";
import { profileMock } from "@/mockUtils/profileMock";
import { revalidateUser } from "@/app/actions";
import countries from "@/data/countries.json";
import { EditableProfile } from "@/client";
import { settingsApi } from "@/lib/apis";
import constants from "@/lib/constants";

import ProfileSettings from "../ProfileSettings";

vi.mock("@/components/contexts/AuthContext");
vi.mock("@/app/actions");
vi.mock("@/lib/apis");

describe("ProfileSettings", () => {
    const defaultFieldValues = {
        location: "a",
        countryAlpha3: "USA",
        about: "b",
        firstName: "c",
        lastName: "d",
    };
    const renderAndFillForm = createFormRenderer<EditableProfile>(
        <ProfileSettings />,
        defaultFieldValues
    );

    it("should successfully render", () => {
        render(<ProfileSettings />);

        const country =
            screen.getByLabelText<HTMLSelectElement>("countryAlpha3");

        expect(country.nodeName.toLowerCase()).toBe("select");
        expect(country.children.length).toBe(Object.keys(countries).length);
        expect(country).toHaveValue(profileMock.countryAlpha3);

        expect(screen.getByLabelText("firstName")).toHaveValue(
            profileMock.firstName
        );

        expect(screen.getByLabelText("lastName")).toHaveValue(
            profileMock.lastName
        );

        expect(screen.getByLabelText("location")).toHaveValue(
            profileMock.location
        );

        expect(screen.getByLabelText("about")).toHaveValue(profileMock.about);
    });

    it("should submit the form with updated values", async () => {
        const returnedProfile = { test: "ing" };
        (settingsApi.updateProfile as Mock).mockResolvedValue(returnedProfile);

        await renderAndFillForm();
        expect(revalidateUser).toHaveBeenCalledOnce();
        expect(revalidateUser).toHaveBeenCalledWith(profileMock.username);

        expect(setAuthedProfile).toHaveBeenCalledOnce();
        expect(setAuthedProfile).toHaveBeenCalledWith(returnedProfile);
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
