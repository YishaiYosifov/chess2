"use client";

import { usePathname, useRouter, useSearchParams } from "next/navigation";
import { UserIcon, GlobeAltIcon } from "@heroicons/react/24/outline";
import Card from "@/components/ui/Card";
import constants from "@/lib/constants";
import clsx from "clsx";

const SettingsSelector = () => {
    const searchParams = useSearchParams();
    const page = searchParams.get(constants.SEARCH_PARAMS.SETTINGS_PAGE);

    return (
        <Card
            className={clsx(
                "h-full w-full gap-0 p-0 md:max-w-80",
                page ? "hidden md:flex" : "flex",
            )}
        >
            <SettingButton queryPath={constants.SETTING_QUERY_PATHS.PROFILE}>
                <UserIcon className="h-8 w-8" />
                Profile
            </SettingButton>
            <SettingButton queryPath={constants.SETTING_QUERY_PATHS.SOCIAL}>
                <GlobeAltIcon className="h-8 w-8" />
                Social
            </SettingButton>
        </Card>
    );
};
export default SettingsSelector;

const SettingButton = ({
    queryPath,
    children,
}: {
    queryPath: string;
    children: React.ReactNode;
}) => {
    const router = useRouter();
    const pathname = usePathname();

    function addToSearch(): void {
        const params = new URLSearchParams();
        params.set(constants.SEARCH_PARAMS.SETTINGS_PAGE, queryPath);
        router.push(pathname + "?" + params.toString());
    }

    return (
        <button
            className="hover:bg-primary flex cursor-pointer items-center gap-2 p-5 text-lg transition"
            onClick={addToSearch}
        >
            {children}
        </button>
    );
};
