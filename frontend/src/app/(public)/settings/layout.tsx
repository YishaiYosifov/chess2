import SettingsPageSwitcher from "@/features/settings/components/SettingsPageSwitcher";
import SettingsSelector from "@/features/settings/components/SettingsSelector";
import { ReactNode } from "react";

export default function SettingsLayout({ children }: { children: ReactNode }) {
    return (
        <div className="flex w-full justify-center gap-5 p-5">
            <SettingsSelector />
            <SettingsPageSwitcher>{children}</SettingsPageSwitcher>
        </div>
    );
}
