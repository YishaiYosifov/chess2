"use client";

import { usePathname } from "next/navigation";
import styles from "./SettingsNavbar.module.scss";
import { BsGearFill } from "react-icons/bs";

const SettingsNavbar = ({
    settings,
}: {
    settings: { name: string; url: string }[];
}) => {
    const selected = usePathname().split("/").pop() || "profile";

    return (
        <nav className={styles.nav}>
            <h1>
                <BsGearFill /> Settings
            </h1>
            <div className={styles["nav-body"]}>
                {settings.map((setting) => (
                    <a
                        className={`${styles.item} ${
                            setting.url === selected && styles.selected
                        }`}
                        href={`/settings/${setting.url}`}
                        key={setting.name}
                    >
                        {setting.name}
                    </a>
                ))}
            </div>
        </nav>
    );
};
export default SettingsNavbar;
