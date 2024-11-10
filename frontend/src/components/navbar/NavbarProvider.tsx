"use client";

import { useContext } from "react";
import Image from "next/image";
import Link from "next/link";

import { AuthContext } from "../../contexts/authContext";

const NavbarProvider = () => {
    const { hasAuthCookies } = useContext(AuthContext);

    const expand = "md";
    const Logo = () => (
        <Image
            src="/assets/logo-text.svg"
            alt="logo"
            height={40}
            width={147}
            className="inline-block align-top rounded"
        />
    );

    return (
        <header className="fixed top-0 z-10 bg-background" data-testid="navbar">
            <Container fluid="md">
                <Navbar.Brand href="/">
                    <Logo />
                </Navbar.Brand>

                <Navbar.Toggle
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
                </Navbar.Offcanvas>
            </Container>
        </header>
    );
};
export default NavbarProvider;

const Signup = () => {
    return (
        <Nav.Link as={Link} href="/signup" id={styles["signup-container"]}>
            <span id={styles.signup}>Signup</span>
            <span id={styles["signup-helper"]}>Signup</span>
            <BsArrowRight />
        </Nav.Link>
    );
};
