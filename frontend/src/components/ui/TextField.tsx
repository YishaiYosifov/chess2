"use client";

import React, { useId } from "react";

import { twMerge } from "tailwind-merge";
import { PolymorphicProps } from "@/types/polymorphicProps";

interface TextFieldOwnProps {
    label?: string;
    icon?: React.ReactNode;
}

type InputProps<C extends React.ElementType> = PolymorphicProps<
    C,
    TextFieldOwnProps
>;

const TextField = <C extends React.ElementType = "input">({
    as,
    className,
    label,
    icon,
    "aria-label": ariaLabel,
    ...props
}: InputProps<C>) => {
    const id = useId();
    const Component = as || "input";

    return (
        <div className="w-full">
            {label && (
                <label className="text-text/90 font-medium" htmlFor={id}>
                    {label}
                </label>
            )}
            <div className="relative">
                <Component
                    id={id}
                    aria-label={ariaLabel ?? label}
                    className={twMerge(
                        `bg-background/50 autofill:bg-background/50 text-text w-full rounded-md p-1
                        disabled:cursor-not-allowed`,
                        className,
                    )}
                    {...props}
                />
                {icon && (
                    <span className="text-text absolute top-1/2 right-2 size-7 -translate-y-1/2 cursor-pointer">
                        {icon}
                    </span>
                )}
            </div>
        </div>
    );
};
export default TextField;
