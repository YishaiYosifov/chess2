"use client";

import { usePathname } from "next/navigation";
import Link from "next/link";

/**
 * Create a nav item
 *
 * @param {Object} param.itemData - the object that describes the nav item
 */
const NavItem = ({ itemData }) => {
    const pathname = usePathname();
    return (
        <Link
            href={itemData.href}
            className={`${
                pathname.split("/")[1] === itemData.href ? "active" : ""
            } nav-link`}
        >
            {itemData.icon}
            {itemData.label}
        </Link>
    );
};
export default NavItem;
