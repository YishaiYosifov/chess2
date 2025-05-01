"use client";

import { useContext, useRef } from "react";

import {
    PlayIcon,
    HomeIcon,
    ArrowRightEndOnRectangleIcon,
    PencilSquareIcon,
    Cog6ToothIcon,
    ArrowLeftIcon,
    UserCircleIcon,
    BoltSlashIcon,
} from "@heroicons/react/24/outline";
import { HeartIcon } from "@heroicons/react/24/solid";
import Image from "next/image";

import { AuthContext } from "@/contexts/authContext";
import LogoText from "@public/assets/logo-text.svg";
import NavItem from "./NavItem";

const Navbar = () => {
    const toggleMobileButton = useRef<HTMLButtonElement>(null);
    const mobileNav = useRef<HTMLDivElement>(null);

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

    return (
        <aside
            className="border-secondary/50 bg-background flex h-full w-64 flex-col justify-between
                gap-10 border-r px-5 py-10 text-3xl"
            aria-label="sidebar"
        >
            <Image
                src={LogoText}
                alt="logo"
                height={60}
                width={167}
                className="h-min w-full"
            />
            <ul className="flex flex-col gap-6">
                <UpperNavItems />
            </ul>

            {/* Spacer */}

            <div className="flex-grow" />

            <ul className="flex flex-col gap-5 justify-self-end opacity-70">
                <LowerNavItems />
            </ul>
        </aside>
    );
};
export default Navbar;

const UpperNavItems = () => {
    const { hasAuthCookies } = useContext(AuthContext);

    const authedLinks = (
        <NavItem href="/profile" icon={<UserCircleIcon />}>
            Profile
        </NavItem>
    );
    const unauthedLinks = (
        <>
            <NavItem href="/login" icon={<ArrowRightEndOnRectangleIcon />}>
                Login
            </NavItem>
            <NavItem href="/signup" icon={<PencilSquareIcon />}>
                Signup
            </NavItem>
        </>
    );

    return (
        <>
            <NavItem
                href="/play"
                className="text-secondary"
                icon={<PlayIcon />}
            >
                Play
            </NavItem>
            <NavItem href="/" icon={<HomeIcon />}>
                Home
            </NavItem>
            {hasAuthCookies ? authedLinks : unauthedLinks}
            <NavItem href="/donate" icon={<HeartIcon color="red" />}>
                Donate
            </NavItem>
        </>
    );
};

const LowerNavItems = () => {
    const { hasAuthCookies } = useContext(AuthContext);

    const authedLinks = (
        <>
            <NavItem href="/settings" icon={<Cog6ToothIcon />}>
                Settings
            </NavItem>
            <NavItem href="/logout" icon={<BoltSlashIcon />}>
                Logout
            </NavItem>
        </>
    );
    return hasAuthCookies && authedLinks;
};
