import ProfileSettingsForm from "./profile/ProfileSettingsForm";
import ProfilePictureForm from "./profile/ProfilePictureForm";
import UsernameSettingsForm from "./profile/UsernameSettingsForm";

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
