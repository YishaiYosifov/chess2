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

    function toggleMenu(): void {
        toggleMobileButton.current?.classList.toggle("toggle-btn");
        mobileNav.current?.classList.toggle("hidden");
        mobileNav.current?.classList.toggle("flex");
    }

    function closeMenu(): void {
        toggleMobileButton.current?.classList.remove("toggle-btn");
        mobileNav.current?.classList.add("hidden");
        mobileNav.current?.classList.remove("flex");
    }

    return (
        // <aside
        //     className="fixed left-0 top-0 h-screen w-64"
        //     aria-label="sidebar"
        // >
        //     <div className="h-full border-r border-secondary/50 bg-background px-5 py-10">
        //         <ul className="space-y-2 text-3xl">
        //             <li>
        //                 <Image
        //                     src={LogoText}
        //                     alt="logo"
        //                     height={60}
        //                     width={167}
        //                     className="inline-block rounded align-top"
        //                 />
        //             </li>
        //             <li>
        //                 <a
        //                     href="#"
        //                     className="flex items-center rounded-lg p-2 text-gray-900 hover:bg-gray-100 dark:text-white
        //                         dark:hover:bg-gray-700"
        //                 >
        //                     <svg
        //                         xmlns="http://www.w3.org/2000/svg"
        //                         viewBox="0 0 24 24"
        //                         fill="currentColor"
        //                         className="size-6"
        //                     >
        //                         <path
        //                             fillRule="evenodd"
        //                             d="M7.5 6a4.5 4.5 0 1 1 9 0 4.5 4.5 0 0 1-9 0ZM3.751 20.105a8.25 8.25 0 0 1 16.498 0 .75.75 0 0 1-.437.695A18.683 18.683 0 0 1 12 22.5c-2.786 0-5.433-.608-7.812-1.7a.75.75 0 0 1-.437-.695Z"
        //                             clipRule="evenodd"
        //                         />
        //                     </svg>

        //                     <span className="ms-3 flex-1 whitespace-nowrap">
        //                         Profile
        //                     </span>
        //                 </a>
        //             </li>
        //             <li>
        //                 <a
        //                     href="#"
        //                     className="flex items-center rounded-lg p-2 text-gray-900 hover:bg-gray-100 dark:text-white
        //                         dark:hover:bg-gray-700"
        //                 >
        //                     <svg
        //                         xmlns="http://www.w3.org/2000/svg"
        //                         viewBox="0 0 24 24"
        //                         fill="currentColor"
        //                         className="size-6"
        //                     >
        //                         <path
        //                             fillRule="evenodd"
        //                             d="M11.078 2.25c-.917 0-1.699.663-1.85 1.567L9.05 4.889c-.02.12-.115.26-.297.348a7.493 7.493 0 0 0-.986.57c-.166.115-.334.126-.45.083L6.3 5.508a1.875 1.875 0 0 0-2.282.819l-.922 1.597a1.875 1.875 0 0 0 .432 2.385l.84.692c.095.078.17.229.154.43a7.598 7.598 0 0 0 0 1.139c.015.2-.059.352-.153.43l-.841.692a1.875 1.875 0 0 0-.432 2.385l.922 1.597a1.875 1.875 0 0 0 2.282.818l1.019-.382c.115-.043.283-.031.45.082.312.214.641.405.985.57.182.088.277.228.297.35l.178 1.071c.151.904.933 1.567 1.85 1.567h1.844c.916 0 1.699-.663 1.85-1.567l.178-1.072c.02-.12.114-.26.297-.349.344-.165.673-.356.985-.57.167-.114.335-.125.45-.082l1.02.382a1.875 1.875 0 0 0 2.28-.819l.923-1.597a1.875 1.875 0 0 0-.432-2.385l-.84-.692c-.095-.078-.17-.229-.154-.43a7.614 7.614 0 0 0 0-1.139c-.016-.2.059-.352.153-.43l.84-.692c.708-.582.891-1.59.433-2.385l-.922-1.597a1.875 1.875 0 0 0-2.282-.818l-1.02.382c-.114.043-.282.031-.449-.083a7.49 7.49 0 0 0-.985-.57c-.183-.087-.277-.227-.297-.348l-.179-1.072a1.875 1.875 0 0 0-1.85-1.567h-1.843ZM12 15.75a3.75 3.75 0 1 0 0-7.5 3.75 3.75 0 0 0 0 7.5Z"
        //                             clipRule="evenodd"
        //                         />
        //                     </svg>

        //                     <span className="ms-3 flex-1 whitespace-nowrap">
        //                         Settings
        //                     </span>
        //                 </a>
        //             </li>
        //             <li>
        //                 <a
        //                     href="#"
        //                     className="flex items-center rounded-lg p-2 text-gray-900 hover:bg-gray-100 dark:text-white
        //                         dark:hover:bg-gray-700"
        //                 >
        //                     <svg
        //                         xmlns="http://www.w3.org/2000/svg"
        //                         viewBox="0 0 24 24"
        //                         fill="currentColor"
        //                         className="size-6"
        //                     >
        //                         <path
        //                             fillRule="evenodd"
        //                             d="M16.5 3.75a1.5 1.5 0 0 1 1.5 1.5v13.5a1.5 1.5 0 0 1-1.5 1.5h-6a1.5 1.5 0 0 1-1.5-1.5V15a.75.75 0 0 0-1.5 0v3.75a3 3 0 0 0 3 3h6a3 3 0 0 0 3-3V5.25a3 3 0 0 0-3-3h-6a3 3 0 0 0-3 3V9A.75.75 0 1 0 9 9V5.25a1.5 1.5 0 0 1 1.5-1.5h6Zm-5.03 4.72a.75.75 0 0 0 0 1.06l1.72 1.72H2.25a.75.75 0 0 0 0 1.5h10.94l-1.72 1.72a.75.75 0 1 0 1.06 1.06l3-3a.75.75 0 0 0 0-1.06l-3-3a.75.75 0 0 0-1.06 0Z"
        //                             clipRule="evenodd"
        //                         />
        //                     </svg>

        //                     <span className="ms-3 flex-1 whitespace-nowrap">
        //                         Sign In
        //                     </span>
        //                 </a>
        //             </li>
        //             <li>
        //                 <a
        //                     href="#"
        //                     className="flex items-center rounded-lg p-2 text-gray-900 hover:bg-gray-100 dark:text-white
        //                         dark:hover:bg-gray-700"
        //                 >
        //                     <svg
        //                         xmlns="http://www.w3.org/2000/svg"
        //                         viewBox="0 0 24 24"
        //                         fill="currentColor"
        //                         className="size-6"
        //                     >
        //                         <path d="M21.731 2.269a2.625 2.625 0 0 0-3.712 0l-1.157 1.157 3.712 3.712 1.157-1.157a2.625 2.625 0 0 0 0-3.712ZM19.513 8.199l-3.712-3.712-8.4 8.4a5.25 5.25 0 0 0-1.32 2.214l-.8 2.685a.75.75 0 0 0 .933.933l2.685-.8a5.25 5.25 0 0 0 2.214-1.32l8.4-8.4Z" />
        //                         <path d="M5.25 5.25a3 3 0 0 0-3 3v10.5a3 3 0 0 0 3 3h10.5a3 3 0 0 0 3-3V13.5a.75.75 0 0 0-1.5 0v5.25a1.5 1.5 0 0 1-1.5 1.5H5.25a1.5 1.5 0 0 1-1.5-1.5V8.25a1.5 1.5 0 0 1 1.5-1.5h5.25a.75.75 0 0 0 0-1.5H5.25Z" />
        //                     </svg>

        //                     <span className="ms-3 flex-1 whitespace-nowrap">
        //                         Sign Up
        //                     </span>
        //                 </a>
        //             </li>
        //         </ul>
        //     </div>
        // </aside>
        <header className="fixed z-10 w-full border-b border-secondary/50 bg-background p-4 text-2xl">
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
                    pt-10 text-6xl md:hidden"
                ref={mobileNav}
                onClick={closeMenu}
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
            <NavItem href="/play" className="text-secondary">
                Play
            </NavItem>
            <NavItem href="/">Home</NavItem>
        </>
    );

    const authedLinks = (
        <>
            <NavItem href="/profile">Profile</NavItem>
            <NavItem href="/settings">Settings</NavItem>
        </>
    );
    const unauthedLinks = (
        <>
            <NavItem href="/login">Login</NavItem>
            <NavItem href="/signup">Signup</NavItem>
        </>
    );

    return (
        <>
            {baseLinks} {isAuthed ? authedLinks : unauthedLinks}
        </>
    );
};
