import userEvent, { UserEvent } from "@testing-library/user-event";
import { render, screen } from "@testing-library/react";
import { RefObject, createRef } from "react";
import { Mock } from "vitest";

import constants from "@/lib/constants";
import { authApi } from "@/lib/apis";

import PasswordVerificationModal, {
    PasswordVerificationRefProps,
} from "../PasswordVerificationModal";

vi.mock("@/lib/apis");
vi.mock("@/components/contexts/AuthContext");

describe("PasswordVerificationModal", () => {
    const modalBodyText = "For security purposes, please confirm your password";
    const mockFormRef = {
        current: { dispatchEvent: vi.fn() },
    } as unknown as RefObject<HTMLFormElement>;

    /**
     * Render the password verification modal and provide a ref
     *
     * @returns the modal ref and render results
     */
    function renderModal() {
        const ref = createRef<PasswordVerificationRefProps>();
        return {
            ref,
            ...render(
                <PasswordVerificationModal formRef={mockFormRef} ref={ref} />
            ),
        };
    }

    /**
     * Open the modal, type a password and submit the modal
     *
     * @param user - the userevent object
     * @param ref - the modal ref
     */
    async function submitModal(
        user: UserEvent,
        ref: RefObject<PasswordVerificationRefProps>
    ): Promise<void> {
        ref.current?.verifyPassword();

        const passwordInput = await screen.findByTestId("modalPasswordInput");
        const submitButton = await screen.findByTestId("modalVerifyButton");
        await user.type(passwordInput, "a");
        await user.click(submitButton);
    }

    it("shouldn't render the modal before prompted", () => {
        renderModal();
        expect(screen.queryByText(modalBodyText)).not.toBeInTheDocument();
    });

    it.each([null, "12/1/2023"])(
        "should open modal when necessary",
        async (lastLogin) => {
            vi.setSystemTime(new Date(2024, 1, 1));
            if (lastLogin)
                localStorage.setItem(
                    constants.LAST_LOGIN_LOCAL_STORAGE,
                    lastLogin
                );

            const { ref } = renderModal();
            expect(ref.current?.verifyPassword()).toBe(false);
            expect(await screen.findByText(modalBodyText)).toBeInTheDocument();
        }
    );

    it("should not submit when verification fails", async () => {
        const loginMock = authApi.login as Mock;
        loginMock.mockRejectedValue(new Error());

        const user = userEvent.setup();
        const { ref } = renderModal();
        await submitModal(user, ref);

        expect(
            screen.queryByText("Could not verify password")
        ).toBeInTheDocument();
    });

    it("should submit when verification succeeds", async () => {
        const loginMock = authApi.login as Mock;
        loginMock.mockResolvedValue(null);

        const user = userEvent.setup();
        const { ref } = renderModal();
        await submitModal(user, ref);

        expect(mockFormRef.current?.dispatchEvent).toHaveBeenCalledOnce();
    });

    it("should disable the submit button when the password is not provided", async () => {
        const { ref } = renderModal();
        ref.current?.verifyPassword();
        expect(await screen.findByTestId("modalVerifyButton")).toBeDisabled();
    });
});
