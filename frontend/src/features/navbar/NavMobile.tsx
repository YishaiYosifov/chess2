"use client";

import { useRef } from "react";

import Image from "next/image";

import { LowerNavItems, UpperNavItems } from "./NavItems";
import LogoText from "@public/assets/logo-text.svg";
import Link from "next/link";

const NavMobile = ({
    isLoggedIn: hasAccessCookie,
}: {
    isLoggedIn: boolean;
}) => {
    const toggleMobileButton = useRef<HTMLButtonElement>(null);
    const mobileNav = useRef<HTMLDivElement>(null);

    function toggleMenu(): void {
        toggleMobileButton.current?.classList.toggle("toggle-btn");
        mobileNav.current?.classList.toggle("hidden");
        mobileNav.current?.classList.toggle("flex");
    }

    return (
        <header className="sticky top-0 z-50 h-[75px] w-full md:hidden">
            <section
                className="bg-navbar border-secondary/50 flex h-full w-full max-w-4xl items-center
                    justify-between border-b p-5"
                data-testid="navMobile"
            >
                <Link href="/">
                    <Image
                        src={LogoText}
                        alt="logo"
                        height={40}
                        width={147}
                        className="inline-block rounded align-top"
                    />
                </Link>

                <button
                    className="relative h-8 w-8 text-3xl md:hidden"
                    ref={toggleMobileButton}
                    onClick={toggleMenu}
                >
                    <span
                        className="beforeduration-500 bg-text before:bg-text after:bg-text absolute top-4 left-0
                            -mt-0.5 h-1 w-8 rounded transition-all duration-500 before:absolute before:h-1
                            before:w-8 before:-translate-x-4 before:translate-y-3 before:rounded
                            before:transition-all before:content-[''] after:absolute after:h-1 after:w-8
                            after:-translate-x-4 after:-translate-y-3 after:rounded after:transition-all
                            after:duration-500 after:content-['']"
                    />
                </button>
            </section>

            <nav
                className="bg-navbar absolute left-0 hidden h-[calc(100vh-75px)] w-full flex-col
                    items-center gap-5 overflow-auto pt-10 text-6xl md:hidden"
                ref={mobileNav}
                onClick={toggleMenu}
                data-testid="navMobileOpened"
            >
                <UpperNavItems hasAccessCookie={hasAccessCookie} />
                <LowerNavItems hasAccessCookie={hasAccessCookie} />
            </nav>
        </header>
    );
};

export default NavMobile;
