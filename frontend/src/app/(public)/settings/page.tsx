import withAuthedUser from "@/features/auth/hocs/withAuthedUser";
import SettingsSelector from "@/features/settings/components/SettingsSelector";
import SettingsPageSwitcher from "@/features/settings/components/SettingsPageSwitcher";

function Page() {
    return (
        <div className="flex w-full justify-center gap-5 p-5">
            <SettingsSelector />
            <SettingsPageSwitcher />
        </div>
    );
}
export default withAuthedUser(Page);
