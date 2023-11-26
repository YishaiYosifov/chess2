import { redirect } from "next/navigation";

const SettingsPage = async () => {
    redirect("/settings/profile");
};

export default SettingsPage;
