import React, { forwardRef } from "react";
import { twMerge } from "tailwind-merge";

const Card: React.ForwardRefRenderFunction<
    HTMLDivElement,
    React.InputHTMLAttributes<HTMLDivElement>
> = ({ className, ...props }, ref) => {
    return (
        <div
            className={twMerge(
                "bg-card border-secondary/30 flex flex-col gap-3 rounded-md border p-4",
                className,
            )}
            ref={ref}
            {...props}
        />
    );
};
export default forwardRef(Card);
