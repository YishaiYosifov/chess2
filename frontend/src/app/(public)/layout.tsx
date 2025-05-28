import { Secular_One } from "next/font/google";
import type { Metadata } from "next";
import { ReactNode } from "react";

import "../globals.css";

import WSPushAction from "@/components/WSPushAction";
import Navbar from "@/components/navbar/Navbar";

const secularOne = Secular_One({
    weight: ["400"],
    subsets: ["latin"],
});

export const metadata: Metadata = {
    icons: {
        icon: "./public/favicon.ico",
    },
};

const RootLayout = async ({ children }: { children: ReactNode }) => {
    return (
        <html lang="en" data-bs-theme="dark">
            <body className={`${secularOne.className} bg-background text-text`}>
                <WSPushAction />

                <div className="flex min-h-screen min-w-screen flex-col md:flex-row">
                    <Navbar />
                    <main className="flex flex-1">{children}</main>
                </div>
            </body>
        </html>
    );
};
export default RootLayout;
