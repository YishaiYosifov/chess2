"use client";

import { useEffect, useRef, useState } from "react";
import Image from "next/image";
import Link from "next/link";
import clsx from "clsx";

import LogoText from "@public/assets/logo-text.svg";
import Sidebar from "./Sidebar";
import getSidebarCollapseWidthCls from "../lib/sidebarWidth";

const NavMobile = ({ hasAccessCookie }: { hasAccessCookie: boolean }) => {
    const [isOpen, setIsOpen] = useState(false);
    const headerRef = useRef<HTMLElement>(null);

    const toggle = () => setIsOpen((prev) => !prev);

    useEffect(() => {
        const handleClickOutside = (event: MouseEvent) => {
            if (!headerRef.current?.contains(event.target as Node))
                setIsOpen(false);
        };

        document.addEventListener("mousedown", handleClickOutside);

        return () => {
            document.removeEventListener("mousedown", handleClickOutside);
        };
    }, []);

    return (
        <header
            className="sticky top-0 z-50 h-[75px] w-full md:hidden"
            ref={headerRef}
        >
            <section
                className="bg-navbar border-secondary/50 flex h-full w-full max-w-4xl items-center
                    justify-between border-b p-5"
                data-testid="navMobile"
            >
                <button
                    className={clsx(
                        "relative h-8 w-8 cursor-pointer text-3xl",
                        isOpen && "toggle-btn",
                    )}
                    onClick={toggle}
                    data-testid="sidebarToggle"
                >
                    <span
                        className="bg-text before:bg-text after:bg-text absolute top-4 left-0 -mt-0.5 h-1 w-8
                            rounded transition-all duration-500 before:absolute before:h-1 before:w-8
                            before:-translate-x-4 before:translate-y-3 before:rounded before:transition-all
                            before:duration-500 before:content-[''] after:absolute after:h-1 after:w-8
                            after:-translate-x-4 after:-translate-y-3 after:rounded after:transition-all
                            after:duration-500 after:content-['']"
                    />
                </button>

                <Link href="/">
                    <Image
                        src={LogoText}
                        alt="logo"
                        height={40}
                        width={147}
                        className="inline-block rounded align-top"
                    />
                </Link>
            </section>

            <section
                className={clsx(
                    "fixed top-[75px] z-50 flex h-[calc(100vh-75px)] transition-transform",
                    isOpen ? "translate-x-0" : "-translate-x-full",
                    getSidebarCollapseWidthCls(false),
                )}
                data-testid="sidebarSlider"
            >
                <Sidebar
                    isCollapsed={false}
                    hasAccessCookie={hasAccessCookie}
                />
            </section>
        </header>
    );
};

export default NavMobile;
