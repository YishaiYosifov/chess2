import Link from "next/link";
import React from "react";

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
            className={`${className} hover:opacity-70 transition-opacity`}
        >
            {children}
        </Link>
    );
};
export default NavItem;
