import React from "react";

import Link from "next/link";
import clsx from "clsx";

const NavItem = ({
    href,
    className,
    icon,
    children,
}: {
    href: string;
    className?: string;
    icon?: React.ReactNode;
    children?: React.ReactNode;
}) => {
    return (
        <div
            className={clsx(
                className,
                "relative flex items-center gap-4 transition-opacity hover:opacity-70",
            )}
        >
            {icon && (
                <span className="size-9 cursor-pointer opacity-70">{icon}</span>
            )}
            <Link href={href}>{children}</Link>
        </div>
    );
};
export default NavItem;
