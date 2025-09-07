import { render, screen } from "@testing-library/react";
import { StoreApi } from "zustand";

import { SessionContext } from "@/features/auth/contexts/sessionContext";
import {
    createSessionStore,
    SessionStore,
} from "@/features/auth/stores/sessionStore";
import { editUsername, ErrorCode, PrivateUser } from "@/lib/apiClient";
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
        const now = new Date().toISOString();
        userMock.usernameLastChanged = now;
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
        userMock.usernameLastChanged = null;
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

    it.each([
        { errors: [], expectedDescription: "Failed to edit username" },
        {
            errors: [
                {
                    errorCode: ErrorCode.PROFILE_USER_NAME_TAKEN,
                    description: "Username Taken",
                    metadata: {},
                },
            ],
            expectedDescription: "Username Taken",
        },
    ])(
        "should display status message on API failure",
        async ({ errors, expectedDescription }) => {
            const user = userEvent.setup();
            editUsernameMock.mockResolvedValueOnce({
                data: undefined,
                error: {
                    extensions: {},
                    errors,
                },
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

            const status = await screen.findByTestId("fieldError-userName");
            expect(status).toHaveTextContent(expectedDescription);
            expect((store.getState().user as PrivateUser).userName).toBe(
                userMock.userName,
            );
        },
    );
});
