import React from "react";
import clsx from "clsx";
import { twMerge } from "tailwind-merge";

const Card = ({
    className,
    ...props
}: React.HTMLAttributes<HTMLDivElement>) => {
    return (
        <div
            className={twMerge("bg-card flex rounded-md p-4", className)}
            {...props}
        />
    );
};
export default Card;
