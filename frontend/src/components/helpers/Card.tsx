import React from "react";
import clsx from "clsx";

const Card = ({
    children,
    className,
    ...props
}: React.HTMLAttributes<HTMLDivElement>) => {
    return (
        <div className={clsx("flex bg-card", className)} {...props}>
            {children}
        </div>
    );
};
export default Card;
