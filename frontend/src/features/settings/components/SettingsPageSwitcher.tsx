"use client";

import { ArrowLeftIcon } from "@heroicons/react/24/outline";

import constants from "@/lib/constants";
import React from "react";
import ProfileSettings from "./ProfileSettings";
import SocialSettings from "./SocialSettings";
import { usePathname, useRouter, useSearchParams } from "next/navigation";
import clsx from "clsx";

const SettingsPageSwitcher = () => {
    const searchParams = useSearchParams();
    const router = useRouter();
    const pathname = usePathname();

    const page = searchParams.get(constants.SEARCH_PARAMS.SETTINGS_PAGE);

    const settingsMap: Record<string, React.ReactNode> = {
        [constants.SETTING_QUERY_PATHS.PROFILE]: <ProfileSettings />,
        [constants.SETTING_QUERY_PATHS.SOCIAL]: <SocialSettings />,
    };

    const clearSearch = () => router.push(pathname);

    return (
        <section
            className={clsx(
                "w-full max-w-3xl flex-col gap-5",
                page ? "flex" : "hidden md:flex",
            )}
        >
            <button
                className="flex cursor-pointer items-center gap-3 md:hidden"
                onClick={clearSearch}
            >
                <ArrowLeftIcon className="h-6 w-6" /> Go Back
            </button>
            {settingsMap[page ?? constants.SETTING_QUERY_PATHS.PROFILE]}
        </section>
    );
};
export default SettingsPageSwitcher;
