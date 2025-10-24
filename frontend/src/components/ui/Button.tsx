"use client";

import { twMerge } from "tailwind-merge";

const Button = ({
    className,
    ...buttonProps
}: React.ButtonHTMLAttributes<HTMLButtonElement>) => {
    return (
        <button
            className={twMerge(
                `bg-primary disabled:text-text/50 cursor-pointer rounded-md p-2
                hover:brightness-90 disabled:cursor-not-allowed disabled:brightness-70`,
                className,
            )}
            {...buttonProps}
        />
    );
};
export default Button;
