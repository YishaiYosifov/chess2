import { ButtonHTMLAttributes } from "react";
import { twMerge } from "tailwind-merge";

const PaginationButton = ({
    className,
    children,
    ...props
}: ButtonHTMLAttributes<HTMLButtonElement>) => {
    return (
        <button
            className={twMerge(
                `min-w-8 cursor-pointer rounded bg-neutral-800/50 p-1 disabled:cursor-not-allowed
                disabled:brightness-75`,
                className,
            )}
            {...props}
        >
            {children}
        </button>
    );
};
export default PaginationButton;
