"use client";

import clsx from "clsx";

import { ArrowLeftIcon, ArrowRightIcon } from "@heroicons/react/24/outline";
import Image from "next/image";

import { LowerNavItems, UpperNavItems } from "./NavItems";
import LogoText from "@public/assets/logo-text.svg";
import Logo from "@public/assets/logo-no-bg.svg";
import NavItem from "./NavItem";
import useCollapseState from "./useCollapseState";
import Link from "next/link";

const NavDesktop = ({
    isLoggedIn,
    isCollapsedInitialState,
}: {
    isLoggedIn: boolean;
    isCollapsedInitialState: boolean;
}) => {
    const { isCollapsed, toggleCollapse } = useCollapseState(
        isCollapsedInitialState,
    );

    const width = isCollapsed ? "w-25" : "w-64";
    return (
        <section className={clsx(width, "shrink-0 transition-[width]")}>
            <aside
                className={clsx(
                    `border-secondary/50 bg-navbar fixed z-50 hidden h-screen flex-col
                    justify-between gap-10 overflow-auto border-r px-5 py-10 text-3xl
                    transition-[width] md:flex`,
                    isCollapsed && "items-center",
                    width,
                )}
                data-testid="navDesktop"
                data-is-collapsed={isCollapsed}
                aria-label="sidebar"
            >
                <Link
                    href="/"
                    className="flex justify-center"
                    data-testid="navDesktopLogo"
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
                <ul className="flex flex-col gap-6">
                    <UpperNavItems
                        hasAccessCookie={isLoggedIn}
                        isCollapsed={isCollapsed}
                    />
                </ul>

                {/* Spacer */}
                <div className="flex-grow" />

                <ul className="flex flex-col gap-5 justify-self-end opacity-70">
                    <LowerNavItems
                        hasAccessCookie={isLoggedIn}
                        isCollapsed={isCollapsed}
                    />

                    {/* Collapse button */}
                    <NavItem
                        as="button"
                        className="hidden cursor-pointer lg:flex"
                        data-testid="navDesktopCollapseButton"
                        icon={
                            isCollapsed ? <ArrowRightIcon /> : <ArrowLeftIcon />
                        }
                        onClick={toggleCollapse}
                        isCollapsed={isCollapsed}
                    >
                        Collapse
                    </NavItem>
                </ul>
            </aside>
        </section>
    );
};

export default NavDesktop;
