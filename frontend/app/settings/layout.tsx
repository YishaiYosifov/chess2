"use client";

import { ReactNode } from "react";

import { LocalProfile } from "@/zustand/slices/authSlice";
import styles from "./SettingsLayout.module.scss";

import SettingsNavbar from "@/components/pages/settings/SettingsNavbar";
import StoreInitializer from "@/components/StoreInitializer";
import withAuth from "@/components/hocs/withAuth";

const layout = async ({
    children,
    profile,
}: {
    children: ReactNode;
    profile: LocalProfile;
}) => {
    const settings = [
        { name: "Profile", url: "profile" },
        { name: "Live Game", url: "live-game" },
        { name: "Blocked", url: "blocked" },
        { name: "Password", url: "blocked" },
    ];

    return (
        <>
            <StoreInitializer
                values={{ localProfile: {} }}
                action="SET_LOCAL_PROFILE"
            />

            <SettingsNavbar settings={settings} />

            <section className={styles["setting-page"]}>{children}</section>
        </>
    );
};
export default layout;
