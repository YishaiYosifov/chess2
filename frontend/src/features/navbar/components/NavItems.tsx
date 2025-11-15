"use client";

import {
    PlayIcon,
    Cog6ToothIcon,
    UserCircleIcon,
    BoltSlashIcon,
    CalendarIcon,
    HomeIcon,
    ArrowLeftEndOnRectangleIcon,
    BookOpenIcon,
} from "@heroicons/react/24/outline";
import { HeartIcon } from "@heroicons/react/16/solid";

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

            {hasAccessCookie && (
                <NavItem
                    as={Link}
                    href="/profile"
                    icon={<UserCircleIcon />}
                    isCollapsed={isCollapsed}
                >
                    Profile
                </NavItem>
            )}
            <NavItem
                as={Link}
                href={constants.PATHS.DONATE}
                icon={<HeartIcon color="red" />}
                isCollapsed={isCollapsed}
            >
                Donate
            </NavItem>

            {!hasAccessCookie && (
                <NavItem
                    as={Link}
                    href="/register"
                    icon={<ArrowLeftEndOnRectangleIcon />}
                    isCollapsed={isCollapsed}
                    className="text-secondary rounded-md"
                >
                    <div className="flex flex-col gap-1">
                        <span>Sign In</span>
                    </div>
                </NavItem>
            )}
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
    return (
        <>
            <NavItem
                as={Link}
                href={constants.PATHS.GUIDE}
                icon={<BookOpenIcon />}
                isCollapsed={isCollapsed}
            >
                Guide
            </NavItem>
            {hasAccessCookie && authedLinks}
        </>
    );
};
