"use client";

import {
    PlayIcon,
    PlusIcon,
    Cog6ToothIcon,
    UserCircleIcon,
    BoltSlashIcon,
    CalendarIcon,
    HomeIcon,
} from "@heroicons/react/24/outline";

import Link from "next/link";

import NavItem from "./NavItem";
import constants from "@/lib/constants";

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
                href="/register"
                icon={<PlusIcon />}
                isCollapsed={isCollapsed}
                className="text-secondary rounded-md"
            >
                Register
            </NavItem>
        </>
    );

    return (
        <>
            <NavItem
                as={Link}
                href={constants.PATHS.PLAY}
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
            <NavItem
                as={Link}
                href={constants.PATHS.QUESTS}
                icon={<CalendarIcon />}
                isCollapsed={isCollapsed}
            >
                Quests
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
                href={constants.PATHS.SETTINGS_BASE}
                icon={<Cog6ToothIcon />}
                isCollapsed={isCollapsed}
            >
                Settings
            </NavItem>

            <NavItem
                as={Link}
                href={constants.PATHS.LOGOUT}
                icon={<BoltSlashIcon />}
                isCollapsed={isCollapsed}
            >
                Logout
            </NavItem>
        </>
    );
    return hasAccessCookie && authedLinks;
};
