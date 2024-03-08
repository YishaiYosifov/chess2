import { render, screen, waitFor } from "@testing-library/react";
import { Mock } from "vitest";

import { setAuthedProfile } from "@/hooks/__mocks__/useAuthed";
import { profileMock } from "@/mockUtils/profileMock";
import { fillForm } from "@/lib/utils/testUtils";
import { revalidateUser } from "@/app/actions";
import countries from "@/data/countries.json";
import { settingsApi } from "@/lib/apis";
import constants from "@/lib/constants";

import userEvent from "@testing-library/user-event";
import ProfileSettings from "../ProfileSettings";

vi.mock("@/hooks/useAuthed");
vi.mock("@/app/actions");
vi.mock("@/lib/apis");

describe("ProfileSettings", () => {
    const editProfileValues = {
        location: "a",
        countryAlpha3: "USA",
        about: "b",
        firstName: "c",
        lastName: "d",
    };

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

        const user = userEvent.setup();
        render(<ProfileSettings />);

        // check the submit button is disabled before editing
        const submitButton = screen.getByText<HTMLButtonElement>("Save");
        expect(submitButton.disabled).toBeTruthy();

        await fillForm(user, editProfileValues);
        await user.click(submitButton);

        // check everything was updated correctly
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
