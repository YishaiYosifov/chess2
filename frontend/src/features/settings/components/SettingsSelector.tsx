"use client";

import { usePathname, useRouter } from "next/navigation";
import { UserIcon, GlobeAltIcon } from "@heroicons/react/24/outline";
import Card from "@/components/ui/Card";
import constants from "@/lib/constants";
import clsx from "clsx";

const SettingsSelector = () => {
    const pathname = usePathname();
    const isBaseSettings = pathname === constants.PATHS.SETTINGS_BASE;

    return (
        <Card
            className={clsx(
                "sticky top-5 max-h-[calc(100vh-2.5rem)] w-full gap-0 p-0 md:max-w-80",
                isBaseSettings ? "flex" : "hidden md:flex",
            )}
        >
            <SettingButton
                path={constants.PATHS.SETTINGS_PROFILE}
                className="rounded-t-md"
            >
                <UserIcon className="h-8 w-8" />
                Profile
            </SettingButton>
            <SettingButton path={constants.PATHS.SETTINGS_SOCIAL}>
                <GlobeAltIcon className="h-8 w-8" />
                Social
            </SettingButton>
        </Card>
    );
};
export default SettingsSelector;

const SettingButton = ({
    path,
    className,
    children,
}: {
    path: string;
    className?: string;
    children: React.ReactNode;
}) => {
    const router = useRouter();

    return (
        <button
            className={clsx(
                "hover:bg-primary flex cursor-pointer items-center gap-2 p-5 text-lg transition",
                className,
            )}
            onClick={() => router.push(path)}
        >
            {children}
        </button>
    );
};
