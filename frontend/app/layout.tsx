import { Secular_One } from "next/font/google";
import "bootstrap/dist/css/bootstrap.css";
import { ReactNode } from "react";

import "./globals.scss";

import NavbarProvider from "@/components/navbar/NavbarProvider";
import InitializeStore from "@/components/InitializeStore";

import { cookies } from "next/headers";

const secularOne = Secular_One({
    weight: ["400"],
    subsets: ["latin"],
});

export const metadata = {
    icons: {
        icon: "./public/favicon.ico",
    },
};

const RootLayout = async ({ children }: { children?: ReactNode }) => {
    const nextCookies = cookies();
    const isAuthed =
        nextCookies.has("refresh_token_cookie") &&
        nextCookies.has("access_token_cookie");
    return (
        <html lang="en" data-bs-theme="dark">
            <body className={secularOne.className}>
                <InitializeStore values={{ isAuthed: isAuthed }} />

                <NavbarProvider />
                <main>{children}</main>
            </body>
        </html>
    );
};
export default RootLayout;
