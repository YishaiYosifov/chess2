import WithAuthedUser from "@/features/auth/components/WithAuthedUser";
import SettingsSelector from "@/features/settings/components/SettingsSelector";
import SettingsPageSwitcher from "@/features/settings/components/SettingsPageSwitcher";

export default function Page() {
    return (
        <WithAuthedUser>
            <div className="flex w-full justify-center gap-5 p-5">
                <SettingsSelector />
                <SettingsPageSwitcher />
            </div>
        </WithAuthedUser>
    );
}
