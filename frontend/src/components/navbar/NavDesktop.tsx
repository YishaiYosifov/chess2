"use client";

import { useState } from "react";
import clsx from "clsx";

import { ArrowLeftIcon, ArrowRightIcon } from "@heroicons/react/24/outline";
import Cookies from "js-cookie";
import Image from "next/image";

import constants from "@/lib/constants";

import { LowerNavItems, UpperNavItems } from "./NavItems";
import LogoText from "@public/assets/logo-text.svg";
import Logo from "@public/assets/logo-no-bg.svg";
import NavItem from "./NavItem";

const NavDesktop = ({
    hasAccessToken,
    isCollapsedInitialState,
}: {
    hasAccessToken: boolean;
    isCollapsedInitialState: boolean;
}) => {
    const [isCollapsed, setIsCollapsed] = useState(isCollapsedInitialState);

    function toggleCollapse(): void {
        setIsCollapsed((prev) => {
            const newIsCollapsed = !prev;
            if (!newIsCollapsed) {
                Cookies.remove(constants.COOKIES.SIDEBAR_COLLAPSED);
                return newIsCollapsed;
            }

            const date = new Date();
            date.setTime(date.getTime() + 400 * 24 * 60 * 60 * 1000);
            Cookies.set(constants.COOKIES.SIDEBAR_COLLAPSED, "1", {
                expires: date,
            });
            return newIsCollapsed;
        });
    }

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
                data-testid="navbarDesktop"
                data-is-collapsed={isCollapsed}
                aria-label="sidebar"
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
                <ul className="flex flex-col gap-6">
                    <UpperNavItems
                        hasAccessToken={hasAccessToken}
                        isCollapsed={isCollapsed}
                    />
                </ul>

                {/* Spacer */}
                <div className="flex-grow" />

                <ul className="flex flex-col gap-5 justify-self-end opacity-70">
                    <LowerNavItems
                        hasAccessToken={hasAccessToken}
                        isCollapsed={isCollapsed}
                    />

                    {/* Collapse button */}
                    <NavItem
                        as="button"
                        className="cursor-pointer"
                        data-testid="collapseButton"
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
