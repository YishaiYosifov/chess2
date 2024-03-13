import { ReactNode } from "react";

import styles from "./SettingsLayout.module.scss";
import constants from "@/lib/constants";

import SettingsNavbar from "@/components/settings/SettingsNavbar";

const layout = async ({ children }: { children: ReactNode }) => {
    return (
        <>
            <SettingsNavbar settings={constants.SETTING_PAGES} />
            <section className={styles["setting-page"]}>{children}</section>
        </>
    );
};
export default layout;
