import WithAuthedUser from "@/features/auth/components/WithAuthedUser";
import SettingsPageSwitcher from "@/features/settings/components/SettingsPageSwitcher";
import SettingsSelector from "@/features/settings/components/SettingsSelector";
import { JSX } from "react";

export default function SettingsLayout({
    children,
}: {
    children: JSX.Element;
}) {
    return (
        <WithAuthedUser>
            <div className="flex w-full justify-center gap-5 p-5">
                <SettingsSelector />
                <SettingsPageSwitcher>{children}</SettingsPageSwitcher>
            </div>
        </WithAuthedUser>
    );
}
