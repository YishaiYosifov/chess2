"use client";

import NavDesktop from "./NavDesktop";
import NavMobile from "./NavMobile";

const Navbar = ({
    isCollapsedInitialState = false,
}: {
    isCollapsedInitialState?: boolean;
}) => {
    return (
        <>
            <NavMobile />
            <NavDesktop isCollapsedInitialState={isCollapsedInitialState} />
        </>
    );
};

export default Navbar;
