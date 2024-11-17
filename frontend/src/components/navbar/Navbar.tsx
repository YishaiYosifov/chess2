"use client";

import { useContext, useRef } from "react";
import Image from "next/image";

import { AuthContext } from "@/contexts/authContext";
import LogoText from "@public/assets/logo-text.svg";
import NavItem from "./NavItem";

const Navbar = () => {
    const { hasAuthCookies } = useContext(AuthContext);

    const toggleMobileButton = useRef<HTMLButtonElement>(null);
    const mobileNav = useRef<HTMLDivElement>(null);

    const toggleMenu = () => {
        toggleMobileButton.current?.classList.toggle("toggle-btn");
        mobileNav.current?.classList.toggle("hidden");
        mobileNav.current?.classList.toggle("flex");
    };

    return (
        <header className="fixed z-10 w-full border-b border-primary/50 bg-background p-4 text-2xl">
            <section
                className="mx-auto flex max-w-4xl items-center justify-between"
                data-testid="navbar"
            >
                <Image
                    src={LogoText}
                    alt="logo"
                    height={40}
                    width={147}
                    className="inline-block rounded align-top"
                />

                <nav className="hidden justify-between space-x-8 md:flex">
                    <NavItems isAuthed={hasAuthCookies} />
                </nav>

                <button
                    className="relative h-8 w-8 text-3xl md:hidden"
                    ref={toggleMobileButton}
                    onClick={toggleMenu}
                >
                    <span
                        className="beforeduration-500 absolute left-0 top-4 -mt-0.5 h-1 w-8 rounded bg-text
                            transition-all duration-500 before:absolute before:h-1 before:w-8
                            before:-translate-x-4 before:translate-y-3 before:rounded before:bg-text
                            before:transition-all before:content-[''] after:absolute after:h-1 after:w-8
                            after:-translate-x-4 after:-translate-y-3 after:rounded after:bg-text
                            after:transition-all after:duration-500 after:content-['']"
                    />
                </button>
            </section>

            <nav
                className="absolute left-0 hidden h-screen w-full flex-col items-center gap-5 bg-background
                    text-6xl md:hidden"
                ref={mobileNav}
                data-testid="navbarMobile"
            >
                <NavItems isAuthed={hasAuthCookies} />
            </nav>
        </header>
    );
};
export default Navbar;

const NavItems = ({ isAuthed }: { isAuthed: boolean }) => {
    const baseLinks = (
        <>
            <NavItem href="/">Home</NavItem>
            <NavItem href="/play">Play</NavItem>
        </>
    );

    const authedLinks = <></>;
    const unauthedLinks = (
        <>
            <NavItem href="/login">Login</NavItem>
            <NavItem href="/signup" className="text-primary">
                Signup
            </NavItem>
        </>
    );

    return (
        <>
            {baseLinks} {isAuthed ? authedLinks : unauthedLinks}
        </>
    );
};
