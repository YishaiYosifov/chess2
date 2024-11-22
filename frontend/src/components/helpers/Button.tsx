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
                className,
                "disabled:bg-primary/50 bg-primary rounded-md p-2 text-3xl disabled:text-text/50",
            )}
            {...buttonProps}
        >
            {children}
        </button>
    );
};
export default Button;
