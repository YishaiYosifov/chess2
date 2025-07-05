import { cookies } from "next/headers";

import constants from "@/lib/constants";

import NavDesktop from "./NavDesktop";
import NavMobile from "./NavMobile";

const Navbar = async () => {
    const cookieStore = await cookies();
    const isNavCollapsed = cookieStore.has(constants.COOKIES.SIDEBAR_COLLAPSED);
    const hasAccessCookie = cookieStore.has(constants.COOKIES.IS_AUTHED);

    return (
        <>
            <NavMobile hasAccessCookie={hasAccessCookie} />
            <NavDesktop
                hasAccessCookie={hasAccessCookie}
                isCollapsedInitialState={isNavCollapsed}
            />
        </>
    );
};

export default Navbar;
