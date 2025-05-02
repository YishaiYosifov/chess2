"use client";

import { useContext, useEffect, useRef, useState } from "react";

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
import Image from "next/image";

import { AuthContext } from "@/contexts/authContext";
import LogoText from "@public/assets/logo-text.svg";
import Logo from "@public/assets/logo-no-bg.svg";
import NavItem from "./NavItem";
import Link from "next/link";
import Button from "../helpers/Button";

const Navbar = () => {
    const toggleMobileButton = useRef<HTMLButtonElement>(null);
    const mobileNav = useRef<HTMLDivElement>(null);
    const [isCollapsed, setIsCollapsed] = useState(false);

    function toggleMenu(): void {
        toggleMobileButton.current?.classList.toggle("toggle-btn");
        mobileNav.current?.classList.toggle("hidden");
        mobileNav.current?.classList.toggle("flex");
    }

    function closeMenu(): void {
        toggleMobileButton.current?.classList.remove("toggle-btn");
        mobileNav.current?.classList.add("hidden");
        mobileNav.current?.classList.remove("flex");
    }

    function toggleCollapse(event): void {
        event.preventDefault();
        setIsCollapsed((prev) => !prev);
    }

    return (
        <aside
            className={`border-secondary/50 bg-navbar flex h-screen
                ${isCollapsed ? "w-25 items-center" : "w-64"} flex-col justify-between gap-10
                overflow-auto border-r px-5 py-10 text-3xl transition-[width]`}
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

export default Navbar;

const UpperNavItems = ({ isCollapsed }: { isCollapsed: boolean }) => {
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

const LowerNavItems = ({ isCollapsed }: { isCollapsed: boolean }) => {
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
