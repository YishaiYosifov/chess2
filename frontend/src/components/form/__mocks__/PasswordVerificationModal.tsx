import {
    ForwardRefRenderFunction,
    useImperativeHandle,
    forwardRef,
} from "react";

import { PasswordVerificationRefProps } from "../PasswordVerificationModal";

export const verifyPasswordMock = vi.fn();

const PasswordVerificationModalMock: ForwardRefRenderFunction<
    PasswordVerificationRefProps,
    {}
> = (_, ref) => {
    useImperativeHandle(
        ref,
        () => ({
            verifyPassword: verifyPasswordMock,
        }),
        []
    );

    return (
        <div data-testid="passwordVerificationModal">
            Password Verification Modal
        </div>
    );
};
export default forwardRef(PasswordVerificationModalMock);
