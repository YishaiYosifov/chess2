import { render, screen } from "@testing-library/react";
import { StoreApi } from "zustand";

import preloadAll from "@/lib/testUtils/dynamicImportMock";
import { SessionContext } from "@/features/auth/contexts/sessionContext";
import {
    createSessionStore,
    SessionStore,
} from "@/features/auth/stores/sessionStore";
import { editProfileSettings, PrivateUser } from "@/lib/apiClient";
import { createFakePrivateUser } from "@/lib/testUtils/fakers/userFaker";
import ProfileSettingsForm from "../ProfileSettingsForm";
import userEvent from "@testing-library/user-event";

vi.mock("@/lib/apiClient");

describe("ProfileSettingsForm", () => {
    let store: StoreApi<SessionStore>;
    let userMock: PrivateUser;
    const editProfileSettingsMock = vi.mocked(editProfileSettings);

    beforeAll(() => preloadAll());

    beforeEach(async () => {
        userMock = createFakePrivateUser();
        store = createSessionStore({ user: userMock });
        editProfileSettingsMock.mockResolvedValue({
            data: undefined,
            response: new Response(),
        });
    });

    it("should render the form with current about and country", () => {
        render(
            <SessionContext.Provider value={store}>
                <ProfileSettingsForm />
            </SessionContext.Provider>,
        );

        const aboutField = screen.getByTestId("aboutMeSetting");
        expect(aboutField).toBeInTheDocument();
        expect(aboutField).toHaveValue(userMock.about);

        const countryField = screen.getByTestId("countrySelector");
        expect(countryField).toBeInTheDocument();
        expect(countryField).toHaveValue(userMock.countryCode);

        expect(screen.getByTestId("submitFormButton")).toBeInTheDocument();
    });

    it("should allow profile changes to be submitted", async () => {
        const user = userEvent.setup();

        render(
            <SessionContext.Provider value={store}>
                <ProfileSettingsForm />
            </SessionContext.Provider>,
        );

        const aboutField = screen.getByTestId("aboutMeSetting");
        const countryField = screen.getByTestId("countrySelector");
        const submitButton = screen.getByTestId("submitFormButton");

        await user.clear(aboutField);
        await user.type(aboutField, "New about text");
        await user.selectOptions(countryField, "US");
        await user.click(submitButton);

        expect(editProfileSettingsMock).toHaveBeenCalledWith({
            body: { about: "New about text", countryCode: "US" },
        });
        expect((store.getState().user as PrivateUser).about).toBe(
            "New about text",
        );
        expect((store.getState().user as PrivateUser).countryCode).toBe("US");
    });

    it("should display status message on API failure", async () => {
        const user = userEvent.setup();
        editProfileSettingsMock.mockResolvedValueOnce({
            data: undefined,
            error: { extensions: {}, errors: [] },
            response: new Response(),
        });

        render(
            <SessionContext.Provider value={store}>
                <ProfileSettingsForm />
            </SessionContext.Provider>,
        );

        const aboutField = screen.getByTestId("aboutMeSetting");
        const submitButton = screen.getByTestId("submitFormButton");

        await user.clear(aboutField);
        await user.type(aboutField, "Failed attempt");
        await user.click(submitButton);

        const status = await screen.findByText(
            "Failed to save profile settings",
        );
        expect(status).toBeInTheDocument();
        expect(store.getState().user).toBe(userMock);
    });
});
