import Image from "next/image";

import styles from "./index.module.scss";
import { apiConfig, profileApi } from "@/lib/apis";

export const metadata = {
    title: "Chess 2 - Home",
};

const IndexPage = async () => {
    return (
        <header className={styles["main-container"]}>
            <div className={styles.header}>
                <h1>chess 2</h1>
                <p>the number one and only website to play anarchy chess</p>
            </div>

            <div className={styles.logo}>
                <div className={styles["logo-bg-square"]} />
                <div className={styles["logo-bg-square"]} />
                <div className={styles["logo-bg-square"]} />
                <div className={styles["logo-bg-square"]} />
                <Image
                    src="/assets/logo.svg"
                    alt="logo"
                    width={250}
                    height={250}
                    draggable={false}
                />
            </div>
        </header>
    );
};
export default IndexPage;
