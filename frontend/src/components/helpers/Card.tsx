import React from "react";
import clsx from "clsx";

const Card = ({
    className,
    ...props
}: React.HTMLAttributes<HTMLDivElement>) => {
    return (
        <div
            className={clsx("flex rounded-md bg-card p-4", className)}
            {...props}
        />
    );
};
export default Card;
