"use client";

import { usePathname, useRouter } from "next/navigation";
import { ArrowLeftIcon } from "@heroicons/react/24/outline";
import React, { JSX } from "react";
import clsx from "clsx";
import constants from "@/lib/constants";

const SettingsPageSwitcher = ({ children }: { children: JSX.Element }) => {
    const router = useRouter();
    const pathname = usePathname();
    const isBaseSettings = pathname === constants.PATHS.SETTINGS_BASE;

    const goBack = () => router.push(constants.PATHS.SETTINGS_BASE);

    return (
        <section
            className={clsx(
                "w-full max-w-3xl flex-col gap-5",
                isBaseSettings ? "hidden md:flex" : "flex",
            )}
        >
            <button
                className="flex cursor-pointer items-center gap-3 md:hidden"
                onClick={goBack}
            >
                <ArrowLeftIcon className="h-6 w-6" /> Go Back
            </button>
            {children}
        </section>
    );
};
export default SettingsPageSwitcher;
