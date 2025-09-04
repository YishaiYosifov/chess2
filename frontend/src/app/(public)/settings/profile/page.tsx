import UsernameSettingsForm from "@/features/settings/components/profile/UsernameSettingsForm";
import ProfileSettingsForm from "@/features/settings/components/profile/ProfileSettingsForm";
import ProfilePictureForm from "@/features/settings/components/profile/ProfilePictureForm";

export default function ProfileSettingsPage() {
    return (
        <>
            <ProfilePictureForm />
            <UsernameSettingsForm />
            <ProfileSettingsForm />
        </>
    );
}
