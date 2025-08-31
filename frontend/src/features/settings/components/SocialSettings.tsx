import PrivacyForm from "./social/PrivacyForm";
import BlockedForm from "./social/BlockedForm";
import FriendsForm from "./social/FriendsForm";

const SocialSettings = () => {
    return (
        <>
            <PrivacyForm />
            <FriendsForm />
            <BlockedForm />
        </>
    );
};
export default SocialSettings;
