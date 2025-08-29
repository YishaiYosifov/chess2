import UsernameSettingsForm from "./profile/UsernameSettingsForm";
import ProfileSettingsForm from "./profile/ProfileSettingsForm";
import ProfilePictureForm from "./profile/ProfilePictureForm";

const ProfileSettings = () => {
    return (
        <>
            <ProfilePictureForm />
            <UsernameSettingsForm />
            <ProfileSettingsForm />
        </>
    );
};
export default ProfileSettings;
