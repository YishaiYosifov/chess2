"use client";

import React, { useState } from "react";

import { EyeIcon as EyeIconOutline } from "@heroicons/react/24/outline";
import { EyeIcon as EyeIconSolid } from "@heroicons/react/24/solid";
import clsx from "clsx";

type InputProps = React.InputHTMLAttributes<HTMLInputElement> & {
    label?: string;
    icon?: React.ReactNode;
};

const Input = ({ className, label, id, icon, ...inputProps }: InputProps) => {
    return (
        <>
            {label && (
                <label
                    className="text-sm font-medium text-text/90"
                    htmlFor={id}
                >
                    {label}
                </label>
            )}
            <div className="relative">
                <input
                    className={clsx(
                        className,
                        "w-full rounded-md p-1 text-black",
                    )}
                    {...inputProps}
                />
                {icon && (
                    <span className="absolute right-2 top-1/2 size-7 -translate-y-1/2 cursor-pointer text-black">
                        {icon}
                    </span>
                )}
            </div>
        </>
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
                    onClick={() => setIsShowingPassword(!isShowingPassword)}
                />
            }
            {...inputProps}
        />
    );
};
