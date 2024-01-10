"use client";

import { BsEyeFill, BsEyeSlashFill, BsUnlockFill } from "react-icons/bs";
import { useState } from "react";

import { FormikInput } from "./FormikElements";
import FormField from "./FormField";

const PasswordField = () => {
    const [isShowingPassword, setIsShowingPassword] = useState(false);
    const EyeToggle = isShowingPassword ? BsEyeFill : BsEyeSlashFill;

    return (
        <FormField hasValidation>
            <FormikInput
                fieldName="password"
                type={isShowingPassword ? "text" : "password"}
                placeholder="Password"
                icon={
                    <>
                        <BsUnlockFill />
                        <EyeToggle
                            onClick={() =>
                                setIsShowingPassword(!isShowingPassword)
                            }
                            role="button"
                        />
                    </>
                }
            />
        </FormField>
    );
};
export default PasswordField;
