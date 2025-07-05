"use client";

import {
    PlayIcon,
    HomeIcon,
    ArrowRightEndOnRectangleIcon,
    PencilSquareIcon,
    Cog6ToothIcon,
    UserCircleIcon,
    BoltSlashIcon,
} from "@heroicons/react/24/outline";

import Link from "next/link";

import NavItem from "./NavItem";

export const UpperNavItems = ({
    hasAccessCookie,
    isCollapsed = false,
}: {
    hasAccessCookie: boolean;
    isCollapsed?: boolean;
}) => {
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
            {hasAccessCookie ? authedLinks : unauthedLinks}
            {/* <NavItem
                as={Link}
                href="/donate"
                icon={<HeartIcon color="red" />}
                isCollapsed={isCollapsed}
            >
                Donate
            </NavItem> */}
        </>
    );
};

export const LowerNavItems = ({
    hasAccessCookie,
    isCollapsed = false,
}: {
    hasAccessCookie: boolean;
    isCollapsed?: boolean;
}) => {
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
    return hasAccessCookie && authedLinks;
};
