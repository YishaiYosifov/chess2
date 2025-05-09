import { cookies } from "next/headers";

import constants from "@/lib/constants";

import NavDesktop from "./NavDesktop";
import NavMobile from "./NavMobile";

const Navbar = async () => {
    const cookieStore = await cookies();
    const isNavCollapsed = cookieStore.has(constants.COOKIES.SIDEBAR_COLLAPSED);
    const hasAccessToken = cookieStore.has(constants.COOKIES.ACCESS_TOKEN);

    return (
        <>
            <NavMobile hasAccessToken={hasAccessToken} />
            <NavDesktop
                hasAccessToken={hasAccessToken}
                isCollapsedInitialState={isNavCollapsed}
            />
        </>
    );
};

export default Navbar;
