import { Button, Modal } from "react-bootstrap";
import {
    ForwardRefRenderFunction,
    useImperativeHandle,
    forwardRef,
    useState,
    RefObject,
} from "react";

import { useAuthedProfile } from "../contexts/AuthContext";
import constants from "@/lib/constants";
import { authApi } from "@/lib/apis";

import { PasswordInput } from "./FormElements";
import FormField from "./FormField";

export interface PasswordVerificationRefProps {
    verifyPassword: () => boolean;
}

const PasswordVerificationModal: ForwardRefRenderFunction<
    PasswordVerificationRefProps,
    { formRef: RefObject<HTMLFormElement> }
> = ({ formRef }, ref) => {
    const [password, setPassword] = useState<string>("");
    const [status, setStatus] = useState<string>("");
    const [isOpen, setIsOpen] = useState(false);

    const { username } = useAuthedProfile();

    /**
     * Checks if the user needs a password confirm
     * (if the user has a fresh access token)
     *
     * @returns whether the user needs to confirm their password
     */
    function needsPasswordConfirm(): boolean {
        const lastLogin = new Date(localStorage.getItem("lastLogin")!);
        if (isNaN(lastLogin.valueOf())) return true;

        const currDate = new Date();
        return (
            (currDate.valueOf() - lastLogin.valueOf()) / 1000 >
            constants.ACCESS_TOKEN_EXPIRES_SECONDS
        );
    }

    /**
     * Verifies that the provided password is correct.
     * If it is it submits the form.
     */
    async function confirmPasswordNSubmit(): Promise<void> {
        if (!password) {
            setStatus("Could not verify password");
            return;
        }

        try {
            await authApi.login({ username, password });
        } catch {
            setStatus("Could not verify password");
            return;
        }

        localStorage.setItem(
            constants.LAST_LOGIN_LOCAL_STORAGE,
            new Date().toUTCString()
        );
        formRef.current?.dispatchEvent(
            new Event("submit", { cancelable: true, bubbles: true })
        );
        closePopup();
    }

    const showPopup = () => setIsOpen(true);
    const closePopup = () => setIsOpen(false);

    useImperativeHandle(
        ref,
        () => ({
            verifyPassword: () => {
                if (needsPasswordConfirm()) {
                    showPopup();
                    return false;
                }
                return true;
            },
        }),
        []
    );

    return (
        <Modal
            show={isOpen}
            centered
            onHide={closePopup}
            data-testid="passwordModal"
        >
            <Modal.Header closeButton>
                <Modal.Title>Confirm your password</Modal.Title>
            </Modal.Header>

            <Modal.Body>
                For security purposes, please confirm your password
                <FormField>
                    <PasswordInput
                        value={password}
                        onChange={(e) => setPassword(e.target.value)}
                    />
                </FormField>
                <span className="text-invalid">{status}</span>
            </Modal.Body>

            <Modal.Footer>
                <Button
                    variant="secondary"
                    onClick={confirmPasswordNSubmit}
                    disabled={false}
                >
                    Verify
                </Button>
            </Modal.Footer>
        </Modal>
    );
};
export default forwardRef(PasswordVerificationModal);
