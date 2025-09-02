import { render, screen } from "@testing-library/react";
import { StoreApi } from "zustand";

import { SessionContext } from "@/features/auth/contexts/sessionContext";
import {
    createSessionStore,
    SessionStore,
} from "@/features/auth/stores/sessionStore";
import { editUsername, PrivateUser } from "@/lib/apiClient";
import { createFakePrivateUser } from "@/lib/testUtils/fakers/userFaker";
import UsernameSettingsForm from "../UsernameSettingsForm";
import userEvent from "@testing-library/user-event";

vi.mock("@/lib/apiClient");

describe("UsernameSettingsForm", () => {
    let store: StoreApi<SessionStore>;
    let userMock: PrivateUser;
    const editUsernameMock = vi.mocked(editUsername);

    beforeEach(() => {
        userMock = createFakePrivateUser();
        store = createSessionStore({ user: userMock, fetchAttempted: true });
        editUsernameMock.mockResolvedValue({
            data: undefined,
            response: new Response(),
        });
    });

    it("should render the form with current username", () => {
        render(
            <SessionContext.Provider value={store}>
                <UsernameSettingsForm />
            </SessionContext.Provider>,
        );

        const input = screen.getByTestId("usernameSettingField");
        expect(input).toBeInTheDocument();
        expect(input).toHaveValue(userMock.userName);

        expect(screen.getByTestId("submitFormButton")).toBeInTheDocument();
    });

    it("should disable the input if cooldown is active", () => {
        const now = Math.floor(Date.now() / 1000);
        userMock.usernameLastChangedSeconds = now;
        store.getState().setUser(userMock);

        render(
            <SessionContext.Provider value={store}>
                <UsernameSettingsForm />
            </SessionContext.Provider>,
        );

        expect(screen.getByTestId("usernameSettingField")).toBeDisabled();
    });

    it("should allow username change when cooldown expired", async () => {
        const user = userEvent.setup();
        userMock.usernameLastChangedSeconds = null;
        store.getState().setUser(userMock);

        render(
            <SessionContext.Provider value={store}>
                <UsernameSettingsForm />
            </SessionContext.Provider>,
        );

        const input = screen.getByTestId("usernameSettingField");
        const submitButton = screen.getByTestId("submitFormButton");

        await user.clear(input);
        await user.type(input, "newUsername");
        await user.click(submitButton);

        expect(editUsernameMock).toHaveBeenCalledWith({
            body: { username: "newUsername" },
        });
        expect((store.getState().user as PrivateUser).userName).toBe(
            "newUsername",
        );
    });

    it("should display status message on API failure", async () => {
        const user = userEvent.setup();
        editUsernameMock.mockResolvedValueOnce({
            data: undefined,
            error: { extensions: {}, errors: [] },
            response: new Response(),
        });

        render(
            <SessionContext.Provider value={store}>
                <UsernameSettingsForm />
            </SessionContext.Provider>,
        );

        const input = screen.getByTestId<HTMLInputElement>(
            "usernameSettingField",
        );
        const submitButton = screen.getByTestId("submitFormButton");

        await user.clear(input);
        await user.type(input, "newUsername");
        await user.click(submitButton);

        const status = await screen.findByText("Failed to edit username");
        expect(status).toBeInTheDocument();
        expect((store.getState().user as PrivateUser).userName).toBe(
            userMock.userName,
        );
    });
});
