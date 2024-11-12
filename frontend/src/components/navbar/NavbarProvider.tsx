"use client";

import { useContext, useRef } from "react";
import Image from "next/image";

import { AuthContext } from "../../contexts/authContext";
import NavItem from "./NavItem";

const NavbarProvider = () => {
    const { hasAuthCookies } = useContext(AuthContext);

    const toggleMobileButton = useRef<HTMLButtonElement>(null);
    const mobileNav = useRef<HTMLDivElement>(null);

    const toggleMenu = () => {
        toggleMobileButton.current?.classList.toggle("toggle-btn");
        mobileNav.current?.classList.toggle("hidden");
        mobileNav.current?.classList.toggle("flex");
    };

    return (
        <header className="fixed bg-background z-10 w-full border-b border-primary/50 p-4 text-2xl">
            <section
                className="mx-auto flex max-w-4xl items-center justify-between"
                data-testid="navbar"
            >
                <Image
                    src="/assets/logo-text.svg"
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
                        className="absolute left-0 top-4 -mt-0.5 h-1 w-8 rounded before:absolute bg-text
                            before:-translate-x-4 before:h-1 before:w-8 before:translate-y-3 before:rounded
                            before:bg-text before:content-[''] after:absolute after:-translate-x-4 after:h-1
                            after:w-8 after:-translate-y-3 after:rounded after:bg-text after:content-['']
                            transition-all duration-500 before:transition-all beforeduration-500
                            after:transition-all after:duration-500"
                    />
                </button>
            </section>

            <nav
                className="absolute left-0 h-screen w-full flex-col items-center gap-5 bg-background
                    text-6xl md:hidden hidden"
                ref={mobileNav}
                data-testid="navbarMobile"
            >
                <NavItems isAuthed={hasAuthCookies} />
            </nav>
        </header>
    );
};
export default NavbarProvider;

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
