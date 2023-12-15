"use client";

import { BsEyeFill, BsEyeSlashFill, BsUnlockFill } from "react-icons/bs";
import { InputGroup } from "react-bootstrap";

import { useState } from "react";

import { FormikField } from "./FormField";

const PasswordField = () => {
    const [isShowingPassword, setIsShowingPassword] = useState(false);
    const EyeToggle = isShowingPassword ? BsEyeFill : BsEyeSlashFill;

    return (
        <FormikField
            data-testid="passwordField"
            fieldName="password"
            placeholder="Password"
            type={isShowingPassword ? "text" : "password"}
        >
            <InputGroup.Text>
                <BsUnlockFill />
                <EyeToggle
                    onClick={() => setIsShowingPassword(!isShowingPassword)}
                    role="button"
                />
            </InputGroup.Text>
        </FormikField>
    );
};
export default PasswordField;
