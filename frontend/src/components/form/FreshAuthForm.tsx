import { FormEvent, useRef } from "react";

import CollapsibleForm, { CollapsibleFormProps } from "./CollapsibleForm";
import PasswordVerificationModal, {
    PasswordVerificationRefProps,
} from "./PasswordVerificationModal";

/**
 * A form with built in password verification.
 */
const FreshAuthForm = ({
    onSubmit,
    children,
    ...formProps
}: CollapsibleFormProps) => {
    const formRef = useRef<HTMLFormElement>(null);
    const modalRef = useRef<PasswordVerificationRefProps>(null);

    /**
     * Check if the password has been verified then submit the form
     */
    function verifyPasswordBeforeSubmit(event: FormEvent<HTMLFormElement>) {
        event.preventDefault();
        if (onSubmit && modalRef.current?.verifyPassword()) onSubmit(event);
    }

    return (
        <CollapsibleForm
            {...formProps}
            onSubmit={verifyPasswordBeforeSubmit}
            ref={formRef}
        >
            {children}
            <PasswordVerificationModal ref={modalRef} formRef={formRef} />
        </CollapsibleForm>
    );
};
export default FreshAuthForm;
