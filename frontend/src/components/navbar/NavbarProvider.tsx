"use client";

import { BsArrowRight, BsBoxArrowRight, BsGearFill } from "react-icons/bs";
import Container from "react-bootstrap/Container";
import Offcanvas from "react-bootstrap/Offcanvas";
import Navbar from "react-bootstrap/Navbar";
import Nav from "react-bootstrap/Nav";
import { useContext } from "react";
import Image from "next/image";
import Link from "next/link";

import { AuthContext } from "../contexts/AuthContext";
import styles from "./navbar.module.scss";
import "./navbar.scss";

const NavbarProvider = () => {
    const { hasAuthCookies } = useContext(AuthContext);

    const expand = "md";
    const Logo = () => (
        <Image
            src="/assets/logo-text.svg"
            alt="logo"
            height={40}
            width={147}
            className="d-inline-block align-top rounded m-0"
        />
    );

    return (
        <Navbar fixed="top" collapseOnSelect expand={expand}>
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
        </Navbar>
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
