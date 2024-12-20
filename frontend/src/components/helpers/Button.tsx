"use client";

import clsx from "clsx";

const Button = ({
    children,
    className,
    ...buttonProps
}: React.ButtonHTMLAttributes<HTMLButtonElement>) => {
    return (
        <button
            className={clsx(
                "rounded-md bg-primary p-2 text-3xl disabled:bg-primary/50 disabled:text-text/50",
                className,
            )}
            {...buttonProps}
        >
            {children}
        </button>
    );
};
export default Button;
