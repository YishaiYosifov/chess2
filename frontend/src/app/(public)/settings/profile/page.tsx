import UsernameSettingsForm from "@/features/settings/components/profile/UsernameSettingsForm";
import ProfileSettingsForm from "@/features/settings/components/profile/ProfileSettingsForm";
import ProfilePictureForm from "@/features/settings/components/profile/ProfilePictureForm";
import WithAuthedUser from "@/features/auth/hocs/WithAuthedUser";

export const metadata = { title: "Profile Settings - Anarchy Chess" };

export default function ProfileSettingsPage() {
    return (
        <WithAuthedUser>
            <>
                <ProfilePictureForm />
                <UsernameSettingsForm />
                <ProfileSettingsForm />
            </>
        </WithAuthedUser>
    );
}
