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
                "bg-primary disabled:bg-primary/70 disabled:text-text/50 rounded-md p-2",
                className,
            )}
            {...buttonProps}
        >
            {children}
        </button>
    );
};
export default Button;
