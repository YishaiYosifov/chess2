"use client";

import { twMerge } from "tailwind-merge";

const Button = ({
    children,
    className,
    ...buttonProps
}: React.ButtonHTMLAttributes<HTMLButtonElement>) => {
    return (
        <button
            className={twMerge(
                `bg-primary disabled:text-text/50 cursor-pointer rounded-md p-2
                hover:brightness-90 disabled:brightness-70`,
                className,
            )}
            {...buttonProps}
        >
            {children}
        </button>
    );
};
export default Button;
