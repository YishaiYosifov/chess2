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
                        "w-full rounded-md bg-white p-1 text-black disabled:cursor-not-allowed",
                        className,
                    )}
                    {...props}
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
export default TextField;
