import React, { forwardRef } from "react";
import { twMerge } from "tailwind-merge";

const Card: React.ForwardRefRenderFunction<
    HTMLDivElement,
    React.InputHTMLAttributes<HTMLDivElement>
> = ({ className, ...props }, ref) => {
    return (
        <article
            className={twMerge(
                "bg-card flex flex-col gap-3 rounded-md p-4",
                className,
            )}
            ref={ref}
            {...props}
        />
    );
};
export default forwardRef(Card);
