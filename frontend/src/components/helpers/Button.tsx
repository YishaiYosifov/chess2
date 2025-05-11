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
                "bg-primary disabled:bg-primary/50 disabled:text-text/50 rounded-md p-2 text-3xl",
                className,
            )}
            {...buttonProps}
        >
            {children}
        </button>
    );
};
export default Button;
