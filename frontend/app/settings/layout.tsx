"use client";

import { ReactNode } from "react";

import styles from "./SettingsLayout.module.scss";

import SettingsNavbar from "@/components/settings/SettingsNavbar";

const layout = async ({ children }: { children: ReactNode }) => {
    const settings = [
        { name: "Profile", url: "profile" },
        { name: "Live Game", url: "live-game" },
        { name: "Blocked", url: "blocked" },
        { name: "Password", url: "password" },
    ];

    return (
        <>
            <SettingsNavbar settings={settings} />
            <section className={styles["setting-page"]}>{children}</section>
        </>
    );
};
export default layout;
