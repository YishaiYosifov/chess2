"use client";

import { usePathname } from "next/navigation";
import { NavItem } from "@/navbar.config";
import Link from "next/link";

const NavOption = ({ itemData }: { itemData: NavItem }) => {
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
export default NavOption;
