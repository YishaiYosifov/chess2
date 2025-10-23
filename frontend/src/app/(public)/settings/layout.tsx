import SettingsPageSwitcher from "@/features/settings/components/SettingsPageSwitcher";
import SettingsSelector from "@/features/settings/components/SettingsSelector";
import { ReactNode } from "react";

export default function SettingsLayout({ children }: { children: ReactNode }) {
    return (
        <main className="flex flex-1 justify-center gap-5 p-5">
            <SettingsSelector />
            <SettingsPageSwitcher>{children}</SettingsPageSwitcher>
        </main>
    );
}
