import {
    BsFillHouseDoorFill,
    BsPlayFill,
    BsPersonFill,
    BsGearFill,
    BsBoxArrowInLeft,
    BsPlusSquare,
    BsBoxArrowInRight,
} from "react-icons/bs";

export default [
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
