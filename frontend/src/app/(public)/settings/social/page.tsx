import BlockedForm from "@/features/settings/components/social/BlockedForm";
import PrivacyForm from "@/features/settings/components/social/PrivacyForm";
import StarsForm from "@/features/settings/components/social/StarsForm";

export default function SocialPage() {
    return (
        <>
            <PrivacyForm />
            <StarsForm />
            <BlockedForm />
        </>
    );
}
