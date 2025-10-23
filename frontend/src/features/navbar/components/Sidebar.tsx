import { ArrowLeftIcon, ArrowRightIcon } from "@heroicons/react/24/outline";
import Image from "next/image";
import Link from "next/link";
import clsx from "clsx";

import { LowerNavItems, UpperNavItems } from "./NavItems";
import LogoText from "@public/assets/logo-text.svg";
import Logo from "@public/assets/logo-no-bg.svg";
import NavItem from "./NavItem";
import getSidebarCollapseWidthCls from "../lib/sidebarWidth";

const Sidebar = ({
    isCollapsed,
    hasAccessCookie,
    toggleCollapse,
}: {
    isCollapsed: boolean;
    hasAccessCookie: boolean;
    toggleCollapse?: () => void;
}) => {
    return (
        <aside
            className={clsx(
                `border-secondary/50 bg-navbar fixed z-50 flex h-full flex-col justify-between
                gap-10 overflow-auto border-r p-5 text-3xl transition-[width]`,
                isCollapsed && "items-center",
                getSidebarCollapseWidthCls(isCollapsed),
            )}
            data-testid="sidebar"
            data-is-collapsed={isCollapsed}
            aria-label="sidebar"
        >
            <ul
                className={clsx(
                    "flex flex-col gap-5",
                    isCollapsed && "items-center",
                )}
            >
                <Link
                    href="/"
                    className="hidden md:flex"
                    data-testid="sidebarLogo"
                >
                    {isCollapsed ? (
                        <Image src={Logo} alt="Logo" width={60} height={60} />
                    ) : (
                        <Image
                            src={LogoText}
                            alt="Logo with text"
                            height={60}
                            width={200}
                            className="self-center"
                        />
                    )}
                </Link>
                <UpperNavItems
                    hasAccessCookie={hasAccessCookie}
                    isCollapsed={isCollapsed}
                />
            </ul>

            <ul className="flex flex-col gap-5 opacity-70">
                <LowerNavItems
                    hasAccessCookie={hasAccessCookie}
                    isCollapsed={isCollapsed}
                />

                {/* Collapse button */}
                <NavItem
                    as="button"
                    className="hidden lg:flex"
                    data-testid="sidebarCollapseButton"
                    icon={isCollapsed ? <ArrowRightIcon /> : <ArrowLeftIcon />}
                    onClick={toggleCollapse}
                    isCollapsed={isCollapsed}
                >
                    Collapse
                </NavItem>
            </ul>
        </aside>
    );
};
export default Sidebar;
