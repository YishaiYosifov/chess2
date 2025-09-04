import UsernameSettingsForm from "@/features/settings/components/profile/UsernameSettingsForm";
import ProfileSettingsForm from "@/features/settings/components/profile/ProfileSettingsForm";
import ProfilePictureForm from "@/features/settings/components/profile/ProfilePictureForm";

export const metadata = { title: "Profile Settings - Chess 2" };

export default function ProfileSettingsPage() {
    return (
        <>
            <ProfilePictureForm />
            <UsernameSettingsForm />
            <ProfileSettingsForm />
        </>
    );
}
