import { Secular_One } from "next/font/google";
import type { Metadata } from "next";
import { ReactNode } from "react";

import "../globals.css";

import ChallengeNotificationRenderer from "@/features/challenges/components/ChallengeNotification/ChallengeNotificationRenderer";
import SessionProvider from "@/features/auth/contexts/sessionContext";
import LobbyHandler from "@/features/lobby/Components/LobbyHandler";
import Navbar from "@/features/navbar/Navbar";
import clsx from "clsx";

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
            <body
                className={clsx(
                    "bg-background text-text",
                    secularOne.className,
                )}
            >
                <SessionProvider user={null}>
                    <div className="flex min-h-screen max-w-screen min-w-[300px] flex-col md:flex-row">
                        <Navbar />
                        {children}
                    </div>

                    <LobbyHandler />
                    <ChallengeNotificationRenderer />
                </SessionProvider>
            </body>
        </html>
    );
};
export default RootLayout;
