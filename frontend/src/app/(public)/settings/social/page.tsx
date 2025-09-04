import BlockedForm from "@/features/settings/components/social/BlockedForm";
import PrivacyForm from "@/features/settings/components/social/PrivacyForm";
import StarsForm from "@/features/settings/components/social/StarsForm";
import { getPreferences } from "@/lib/apiClient";
import constants from "@/lib/constants";
import { cookies } from "next/headers";

export const metadata = { title: "Social Settings - Chess 2" };

export default async function SocialPage() {
    const cookieStore = await cookies();
    const accessToken = cookieStore.get(constants.COOKIES.ACCESS_TOKEN);
    const { error: preferencesError, data: initialPreferences } =
        await getPreferences({ auth: () => accessToken?.value });

    if (preferencesError || !initialPreferences) {
        console.error(preferencesError);
        throw preferencesError;
    }

    return (
        <>
            <PrivacyForm initialPreferences={initialPreferences} />
            <StarsForm />
            <BlockedForm />
        </>
    );
}
