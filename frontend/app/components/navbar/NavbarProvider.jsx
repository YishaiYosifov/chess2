"use client";

import Container from "react-bootstrap/Container";
import Offcanvas from "react-bootstrap/Offcanvas";
import Navbar from "react-bootstrap/Navbar";
import Nav from "react-bootstrap/Nav";
import Image from "next/image";
import Link from "next/link";

import classes from "./NavbarProvider.module.scss";
import NavSeperator from "./NavSeperator";
import NavItem from "./NavItem";
import { useStore } from "@/app/store";

/**
 * Create a bootstrap navbar
 *
 * @param {Object} param.navItemData - the data of the items that should be in the navbar
 */
const NavbarProvider = ({ navItemData }) => {
    const isAuthed = useStore.use.isAuthed();

    const preSeparator = [];
    const postSeparator = [];
    for (const itemData of navItemData) {
        if (
            (itemData.authReq === "authenticated" && !isAuthed) ||
            (itemData.authReq === "unauthenticated" && isAuthed)
        )
            continue;

        const navItem = <NavItem key={itemData.label} itemData={itemData} />;
        if (itemData.position === "pre-seperator") preSeparator.push(navItem);
        else if (itemData.position === "post-seperator")
            postSeparator.push(navItem);
        else throw Error(`Invalid Nav Item Position: ${itemData.position}`);
    }

    return (
        <Navbar collapseOnSelect expand="lg" className="mb-3">
            <Container fluid>
                <Navbar.Toggle aria-controls="offcanvas-navbar" />
                <Link href="/" className="d-block d-lg-none navbar-brand">
                    chess 2
                </Link>
                <Navbar.Offcanvas
                    id="offcanvas-navbar"
                    aria-labelledby="offcanvas-navbar-label"
                    placement="start"
                    responsive="lg"
                >
                    <Offcanvas.Header closeButton>
                        <Offcanvas.Title id="offcanvas-navbar-label">
                            <Image
                                className={classes.logo}
                                src="/assets/logo.webp"
                                width={40}
                                height={40}
                                alt="logo"
                            />
                            chess 2
                        </Offcanvas.Title>
                    </Offcanvas.Header>

                    <Offcanvas.Body>
                        <Nav className={classes["nav-links"]}>
                            <Link
                                href="/"
                                className="d-none d-lg-block navbar-brand"
                            >
                                chess 2
                            </Link>
                            {preSeparator}
                            <NavSeperator />
                            {postSeparator}
                        </Nav>
                    </Offcanvas.Body>
                </Navbar.Offcanvas>
            </Container>
        </Navbar>
    );
};
export default NavbarProvider;
