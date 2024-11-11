"use client";

import { useContext } from "react";
import Image from "next/image";
import Link from "next/link";

import { AuthContext } from "../../contexts/authContext";

const NavbarProvider = () => {
    const { hasAuthCookies } = useContext(AuthContext);

    const Logo = () => (
        <Image
            src="/assets/logo-text.svg"
            alt="logo"
            height={40}
            width={147}
            className="inline-block rounded align-top"
        />
    );

    return (
        <header className="border-white/20 fixed z-10 w-full border-b">
            <nav
                className="mx-auto flex max-w-4xl items-center justify-between p-4 text-2xl"
                data-testid="navbar"
            >
                <section className="flex h-full items-center justify-between space-x-8">
                    <Logo />
                    <Link href="#">Home</Link>
                    <Link href="/play">Play</Link>
                    <Link href="/login">Login</Link>
                </section>

                <Signup />

                {/*<Navbar.Toggle
                    aria-controls={`offcanvasNavbar-expand-${expand}`}
                />

                <Navbar.Offcanvas
                    id={`offcanvasNavbar-expand-${expand}`}
                    aria-labelledby={`offcanvasNavbarLabel-expand-${expand}`}
                    placement="start"
                >
                    <Offcanvas.Header closeButton>
                        <Offcanvas.Title
                            id={`offcanvasNavbarLabel-expand-${expand}`}
                        >
                            <Logo />
                        </Offcanvas.Title>
                    </Offcanvas.Header>

                    <Offcanvas.Body>
                        <Nav className="justify-content-start flex-grow-1">
                            <Nav.Link as={Link} href="/">
                                Home
                            </Nav.Link>

                            <Nav.Link as={Link} href="/play">
                                Play
                            </Nav.Link>

                            {hasAuthCookies ? (
                                <Nav.Link as={Link} href="/user">
                                    Profile
                                </Nav.Link>
                            ) : (
                                <Nav.Link as={Link} href="/login">
                                    Login
                                </Nav.Link>
                            )}
                        </Nav>

                        <Nav className="justify-content-end flex-grow-1">
                            {hasAuthCookies ? (
                                <>
                                    <Nav.Link
                                        as={Link}
                                        href="/settings/profile"
                                    >
                                        <BsGearFill />
                                    </Nav.Link>

                                    <Nav.Link as={Link} href="/logout">
                                        <BsBoxArrowRight />
                                    </Nav.Link>
                                </>
                            ) : (
                                <Signup />
                            )}
                        </Nav>
                    </Offcanvas.Body>
                </Navbar.Offcanvas>*/}
            </nav>
        </header>
    );
};
export default NavbarProvider;

const Signup = () => {
    return (
        <Link href="/signup">
            <span className="from-teal-200 to-teal-600 text-transparent bg-gradient-to-r bg-clip-text">
                Signup
            </span>
        </Link>
    );
};
