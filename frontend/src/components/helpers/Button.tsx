"use client";

const Button = ({
    children,
    className,
    ...buttonProps
}: React.ButtonHTMLAttributes<HTMLButtonElement>) => {
    return (
        <button
            className={`${className ?? ""} rounded-md bg-cta p-2 text-3xl disabled:bg-cta/50
                disabled:text-text/50`}
            {...buttonProps}
        >
            {children}
        </button>
    );
};
export default Button;
