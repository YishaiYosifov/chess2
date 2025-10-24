import { useEffect, useState } from "react";
import Cookies from "js-cookie";

import constants from "@/lib/constants";

function useCollapseState(initial: boolean) {
    const [isCollapsed, setIsCollapsed] = useState(initial);

    useEffect(() => {
        const handleResize = () => {
            const isSmallScreen = window.innerWidth < 1024;
            if (isSmallScreen) {
                setIsCollapsed(true);
                return;
            }

            const cookie = Cookies.get(constants.COOKIES.SIDEBAR_COLLAPSED);
            setIsCollapsed(cookie !== undefined);
        };

        handleResize();
        window.addEventListener("resize", handleResize);
        return () => window.removeEventListener("resize", handleResize);
    }, []);

    const toggleCollapse = () => {
        setIsCollapsed((prev) => {
            const next = !prev;
            if (!next) {
                Cookies.remove(constants.COOKIES.SIDEBAR_COLLAPSED);
            } else {
                const expires = new Date(
                    Date.now() + 400 * 24 * 60 * 60 * 1000,
                );
                Cookies.set(constants.COOKIES.SIDEBAR_COLLAPSED, "true", {
                    expires,
                });
            }
            return next;
        });
    };

    return { isCollapsed, toggleCollapse };
}
export default useCollapseState;
