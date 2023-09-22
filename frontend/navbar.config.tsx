import {
    BsFillHouseDoorFill,
    BsBoxArrowInRight,
    BsBoxArrowInLeft,
    BsPlusSquare,
    BsPersonFill,
    BsPlayFill,
    BsGearFill,
} from "react-icons/bs";

import { IconType } from "react-icons";
import { ReactElement } from "react";

export interface NavItem {
    label: string;
    icon: ReactElement<IconType>;
    position: string;
    href: string;
    authReq?: string;
}

export const navItems: NavItem[] = [
    {
        label: "Home",
        icon: <BsFillHouseDoorFill />,

        position: "pre-seperator",
        href: "/",
    },
    {
        label: "Play",
        icon: <BsPlayFill />,

        position: "pre-seperator",
        href: "/play",
    },
    {
        label: "Profile",
        icon: <BsPersonFill />,

        authReq: "authenticated",

        position: "pre-seperator",
        href: "/user",
    },
    {
        label: "Settings",
        icon: <BsGearFill />,

        authReq: "authenticated",

        position: "pre-seperator",
        href: "/settings",
    },

    {
        label: "Log In",
        icon: <BsBoxArrowInLeft />,

        authReq: "unauthenticated",

        position: "post-seperator",
        href: "/login",
    },
    {
        label: "Sign Up",
        icon: <BsPlusSquare />,

        authReq: "unauthenticated",

        position: "post-seperator",
        href: "/signup",
    },
    {
        label: "Log Out",
        icon: <BsBoxArrowInRight />,

        authReq: "authenticated",

        position: "post-seperator",
        href: "/logout",
    },
];
