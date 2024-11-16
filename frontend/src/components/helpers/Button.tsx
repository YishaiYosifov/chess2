"use client";

const Button = ({
    children,
    className,
    ...buttonProps
}: React.ButtonHTMLAttributes<HTMLButtonElement>) => {
    return (
        <button
            className={`${className ?? ""} cursor-pointer rounded-md bg-cta p-2 text-3xl`}
            {...buttonProps}
        >
            {children}
        </button>
    );
};
export default Button;
