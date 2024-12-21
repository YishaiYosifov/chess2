import ProfilePictureSettings from "@/components/settings/profile/ProfilePictureSettings";
import CredentialSettings from "@/components/settings/profile/CredentialSettings";
import withAuth from "@/hocs/withAuth";
import ProfileSettings from "@/components/settings/profile/ProfileSettings";

const SettingsPage = withAuth(async () => {
    return (
        <div className="flex w-screen max-w-4xl flex-col gap-10 p-10">
            <ProfilePictureSettings />
            <CredentialSettings />
            <ProfileSettings />
        </div>
    );
});
export default SettingsPage;
