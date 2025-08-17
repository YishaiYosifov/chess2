import { cookies } from "next/headers";

import constants from "@/lib/constants";

import NavDesktop from "./NavDesktop";
import NavMobile from "./NavMobile";

const Navbar = async () => {
    const cookieStore = await cookies();
    const isNavCollapsed = cookieStore.has(constants.COOKIES.SIDEBAR_COLLAPSED);
    const isLoggedIn = cookieStore.has(constants.COOKIES.IS_LOGGED_IN);

    return (
        <>
            <NavMobile isLoggedIn={isLoggedIn} />
            <NavDesktop
                isLoggedIn={isLoggedIn}
                isCollapsedInitialState={isNavCollapsed}
            />
        </>
    );
};

export default Navbar;
