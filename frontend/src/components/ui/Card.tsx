import React, { forwardRef } from "react";
import { twMerge } from "tailwind-merge";

const Card: React.ForwardRefRenderFunction<
    HTMLDivElement,
    React.InputHTMLAttributes<HTMLDivElement>
> = ({ className, ...props }, ref) => {
    return (
        <div
            className={twMerge("bg-card flex rounded-md p-4", className)}
            ref={ref}
            {...props}
        />
    );
};
export default forwardRef(Card);
