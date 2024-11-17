"use client";

import { useState } from "react";

import { EyeIcon as EyeIconOutline } from "@heroicons/react/24/outline";
import { EyeIcon as EyeIconSolid } from "@heroicons/react/24/solid";

const Input = ({
    className,
    ...inputProps
}: React.InputHTMLAttributes<HTMLInputElement>) => {
    return (
        <input
            className={`${className ?? ""} w-full rounded-md p-1 text-black`}
            {...inputProps}
        />
    );
};
export default Input;

export const PasswordInput = ({
    name,
    placeholder,
    ...inputProps
}: React.InputHTMLAttributes<HTMLInputElement>) => {
    const [isShowingPassword, setIsShowingPassword] = useState(false);
    const EyeToggle = isShowingPassword ? EyeIconSolid : EyeIconOutline;

    return (
        <div className="relative">
            <Input
                aria-label={name}
                type={isShowingPassword ? "text" : "password"}
                placeholder={placeholder ?? "Password"}
                name={name}
                {...inputProps}
            />
            <EyeToggle
                className="absolute right-2 top-1/2 size-7 translate-y-[-50%] cursor-pointer"
                onClick={() => setIsShowingPassword(!isShowingPassword)}
            />
        </div>
    );
};
