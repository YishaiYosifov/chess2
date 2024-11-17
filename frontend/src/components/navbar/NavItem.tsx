import React from "react";
import clsx from "clsx";

import Link from "next/link";

const NavItem = ({
    href,
    className,
    children,
}: {
    href: string;
    className?: string;
    children?: React.ReactNode;
}) => {
    return (
        <Link
            href={href}
            className={clsx(className, "transition-opacity hover:opacity-70")}
        >
            {children}
        </Link>
    );
};
export default NavItem;
