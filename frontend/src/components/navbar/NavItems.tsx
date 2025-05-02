"use client";

import { useContext } from "react";

import {
    PlayIcon,
    HomeIcon,
    ArrowRightEndOnRectangleIcon,
    PencilSquareIcon,
    Cog6ToothIcon,
    UserCircleIcon,
    BoltSlashIcon,
} from "@heroicons/react/24/outline";
import { HeartIcon } from "@heroicons/react/24/solid";
import Link from "next/link";

import { AuthContext } from "@/contexts/authContext";
import NavItem from "./NavItem";

export const UpperNavItems = ({
    isCollapsed = false,
}: {
    isCollapsed?: boolean;
}) => {
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

export const LowerNavItems = ({
    isCollapsed = false,
}: {
    isCollapsed?: boolean;
}) => {
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
