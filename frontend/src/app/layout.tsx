import { Secular_One } from "next/font/google";
import { cookies } from "next/headers";
import type { Metadata } from "next";
import { ReactNode } from "react";

import constants from "@/lib/constants";
import "./globals.css";

import AuthContextProvider from "@/contexts/authContext";
import Navbar from "@/components/navbar/Navbar";
import WSPushAction from "@/components/WSPushAction";

const secularOne = Secular_One({
    weight: ["400"],
    subsets: ["latin"],
});

export const metadata: Metadata = {
    icons: {
        icon: "./public/favicon.ico",
    },
};

/**
 * The root layout.
 *
 * This will:
 * * Create the navbar element
 * * Initializes the store with whether the user is authorized or not.
 *   Do not use the store to determine whether the user is authorized or not without using the With/WithoutAuth HOCs.
 */
const RootLayout = async ({ children }: { children: ReactNode }) => {
    const nextCookies = await cookies();
    const hasAuthCookies = nextCookies.has(constants.REFRESH_TOKEN);

    return (
        <html lang="en" data-bs-theme="dark" className="min-w-[320px]">
            <body className={`${secularOne.className} bg-background text-text`}>
                <AuthContextProvider hasAuthCookies={hasAuthCookies}>
                    <WSPushAction />

                    <Navbar />
                    <main className="flex h-screen flex-row justify-center pt-[72px]">
                        {children}
                    </main>
                </AuthContextProvider>
            </body>
        </html>
    );
};
export default RootLayout;
