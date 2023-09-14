import { Secular_One } from "next/font/google";

import "bootstrap/dist/css/bootstrap.css";
import navItems from "@/navbar.config.js";
import "./globals.scss";

import { getServerSession, initializeSessionUserState } from "./utils/server";
import { useStore } from "./store";

import NavbarProvider from "./components/navbar/NavbarProvider";
import InitializeStore from "./components/InitializeStore";

const secularOne = Secular_One({
    weight: ["400"],
    subsets: ["latin"],
});

export const metadata = {
    icons: {
        icon: "./public/favicon.ico",
    },
};

const RootLayout = async ({ children }) => {
    useStore.setState(getServerSession());
    await initializeSessionUserState();

    return (
        <html lang="en" data-bs-theme="dark">
            <body className={secularOne.className}>
                <InitializeStore values={useStore.getState()} />

                <NavbarProvider navItemData={navItems} />
                {<main>{children}</main>}
            </body>
        </html>
    );
};
export default RootLayout;
