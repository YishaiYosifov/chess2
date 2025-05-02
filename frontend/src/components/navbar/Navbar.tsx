"use client";

import { useContext, useRef, useState } from "react";
import clsx from "clsx";

import {
    PlayIcon,
    HomeIcon,
    ArrowRightEndOnRectangleIcon,
    PencilSquareIcon,
    Cog6ToothIcon,
    ArrowLeftIcon,
    ArrowRightIcon,
    UserCircleIcon,
    BoltSlashIcon,
} from "@heroicons/react/24/outline";
import { HeartIcon } from "@heroicons/react/24/solid";
import Cookies from "js-cookie";
import Image from "next/image";
import Link from "next/link";

import constants from "@/lib/constants";

import { AuthContext } from "@/contexts/authContext";
import LogoText from "@public/assets/logo-text.svg";
import Logo from "@public/assets/logo-no-bg.svg";
import NavItem from "./NavItem";

const Navbar = ({
    isCollapsedInitialState = false,
}: {
    isCollapsedInitialState?: boolean;
}) => {
    return (
        <>
            <MobileNavbar />
            <DesktopNavbar isCollapsedInitialState={isCollapsedInitialState} />
        </>
    );
};

export default Navbar;

const MobileNavbar = () => {
    const toggleMobileButton = useRef<HTMLButtonElement>(null);
    const mobileNav = useRef<HTMLDivElement>(null);

    function toggleMenu(): void {
        toggleMobileButton.current?.classList.toggle("toggle-btn");
        mobileNav.current?.classList.toggle("hidden");
        mobileNav.current?.classList.toggle("flex");
    }

    return (
        <header className="sticky top-0 z-50 h-[75px] w-full md:hidden">
            <section
                className="bg-navbar border-secondary/50 flex h-full w-full max-w-4xl items-center
                    justify-between border-b p-5"
                data-testid="navbarMobile"
            >
                <Image
                    src={LogoText}
                    alt="logo"
                    height={40}
                    width={147}
                    className="inline-block rounded align-top"
                />

                <button
                    className="relative h-8 w-8 text-3xl md:hidden"
                    ref={toggleMobileButton}
                    onClick={toggleMenu}
                >
                    <span
                        className="beforeduration-500 bg-text before:bg-text after:bg-text absolute top-4 left-0
                            -mt-0.5 h-1 w-8 rounded transition-all duration-500 before:absolute before:h-1
                            before:w-8 before:-translate-x-4 before:translate-y-3 before:rounded
                            before:transition-all before:content-[''] after:absolute after:h-1 after:w-8
                            after:-translate-x-4 after:-translate-y-3 after:rounded after:transition-all
                            after:duration-500 after:content-['']"
                    />
                </button>
            </section>

            <nav
                className="bg-navbar absolute left-0 hidden h-[calc(100vh-75px)] w-full flex-col
                    items-center gap-5 overflow-auto pt-10 text-6xl md:hidden"
                ref={mobileNav}
                onClick={toggleMenu}
                data-testid="navbarMobileOpened"
            >
                <UpperNavItems />
                <LowerNavItems />
            </nav>
        </header>
    );
};

const DesktopNavbar = ({
    isCollapsedInitialState,
}: {
    isCollapsedInitialState: boolean;
}) => {
    const [isCollapsed, setIsCollapsed] = useState(isCollapsedInitialState);

    function toggleCollapse(): void {
        setIsCollapsed((prev) => {
            const newIsCollapsed = !prev;
            if (!newIsCollapsed) {
                Cookies.remove(constants.SIDEBAR_COLLAPSED_COOKIE);
                return newIsCollapsed;
            }

            const date = new Date();
            date.setTime(date.getTime() + 400 * 24 * 60 * 60 * 1000);
            Cookies.set(constants.SIDEBAR_COLLAPSED_COOKIE, "1", {
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
                    <UpperNavItems isCollapsed={isCollapsed} />
                </ul>

                {/* Spacer */}
                <div className="flex-grow" />

                <ul className="flex flex-col gap-5 justify-self-end opacity-70">
                    <LowerNavItems isCollapsed={isCollapsed} />
                    <NavItem
                        as="button"
                        className="cursor-pointer"
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

const UpperNavItems = ({ isCollapsed = false }: { isCollapsed?: boolean }) => {
    const { hasAuthCookies } = useContext(AuthContext);

    const authedLinks = (
        <NavItem
            as={Link}
            href="/profile"
            icon={<UserCircleIcon />}
            isCollapsed={isCollapsed}
        >
            Profile
        </NavItem>
    );
    const unauthedLinks = (
        <>
            <NavItem
                as={Link}
                href="/login"
                icon={<ArrowRightEndOnRectangleIcon />}
                isCollapsed={isCollapsed}
            >
                Login
            </NavItem>
            <NavItem
                as={Link}
                href="/signup"
                icon={<PencilSquareIcon />}
                isCollapsed={isCollapsed}
            >
                Signup
            </NavItem>
        </>
    );

    return (
        <>
            <NavItem
                as={Link}
                href="/play"
                className="text-secondary"
                icon={<PlayIcon />}
                isCollapsed={isCollapsed}
            >
                Play
            </NavItem>
            <NavItem
                as={Link}
                href="/"
                icon={<HomeIcon />}
                isCollapsed={isCollapsed}
            >
                Home
            </NavItem>
            {hasAuthCookies ? authedLinks : unauthedLinks}
            <NavItem
                as={Link}
                href="/donate"
                icon={<HeartIcon color="red" />}
                isCollapsed={isCollapsed}
            >
                Donate
            </NavItem>
        </>
    );
};

const LowerNavItems = ({ isCollapsed = false }: { isCollapsed?: boolean }) => {
    const { hasAuthCookies } = useContext(AuthContext);

    const authedLinks = (
        <>
            <NavItem
                as={Link}
                href="/settings"
                icon={<Cog6ToothIcon />}
                isCollapsed={isCollapsed}
            >
                Settings
            </NavItem>

            <NavItem
                as={Link}
                href="/logout"
                icon={<BoltSlashIcon />}
                isCollapsed={isCollapsed}
            >
                Logout
            </NavItem>
        </>
    );
    return hasAuthCookies && authedLinks;
};
