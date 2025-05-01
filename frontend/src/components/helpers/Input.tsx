"use client";

import React, { useId, useState } from "react";

import { EyeIcon as EyeIconOutline } from "@heroicons/react/24/outline";
import { EyeIcon as EyeIconSolid } from "@heroicons/react/24/solid";
import clsx from "clsx";

type InputProps = React.InputHTMLAttributes<HTMLInputElement> & {
    label?: string;
    icon?: React.ReactNode;
};

const Input = ({
    className,
    label,
    icon,
    "aria-label": ariaLabel,
    ...inputProps
}: InputProps) => {
    const id = useId();
    return (
        <div className={clsx("w-full", className)}>
            {label && (
                <label className="text-text/90 font-medium" htmlFor={id}>
                    {label}
                </label>
            )}
            <div className="relative">
                <input
                    id={id}
                    aria-label={ariaLabel ?? label}
                    className={clsx(
                        className,
                        "w-full rounded-md bg-white p-1 text-black",
                    )}
                    {...inputProps}
                />
                {icon && (
                    <span className="absolute top-1/2 right-2 size-7 -translate-y-1/2 cursor-pointer text-black">
                        {icon}
                    </span>
                )}
            </div>
        </div>
    );
};
export default Input;

export const PasswordInput = ({
    name,
    placeholder,
    ...inputProps
}: InputProps) => {
    const [isShowingPassword, setIsShowingPassword] = useState(false);
    const EyeToggle = isShowingPassword ? EyeIconSolid : EyeIconOutline;

    return (
        <Input
            aria-label={name}
            type={isShowingPassword ? "text" : "password"}
            placeholder={placeholder ?? "Enter a password"}
            name={name}
            icon={
                <EyeToggle
                    data-testid="togglePasswordVisibility"
                    onClick={() => {
                        setIsShowingPassword(!isShowingPassword);
                    }}
                />
            }
            {...inputProps}
        />
    );
};
