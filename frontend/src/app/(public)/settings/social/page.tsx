import WithAuthedUser from "@/features/auth/components/WithAuthedUser";
import BlockedForm from "@/features/settings/components/social/BlockedForm";
import PrivacyForm from "@/features/settings/components/social/PrivacyForm";
import StarsForm from "@/features/settings/components/social/StarsForm";
import { getPreferences, getStarredUsers } from "@/lib/apiClient";
import dataOrThrow from "@/lib/apiClient/dataOrThrow";
import constants from "@/lib/constants";

export const metadata = { title: "Social Settings - Chess 2" };

export default async function SocialPage() {
    return (
        <WithAuthedUser>
            {async ({ user, accessToken }) => {
                const [preferences, stars] = await Promise.all([
                    dataOrThrow(getPreferences({ auth: () => accessToken })),
                    dataOrThrow(
                        getStarredUsers({
                            query: {
                                Page: 0,
                                PageSize: constants.PAGINATION_PAGE_SIZE.STARS,
                            },
                            path: { userId: user.userId },
                        }),
                    ),
                ]);

                return (
                    <>
                        <PrivacyForm initialPreferences={preferences} />
                        <StarsForm initialStars={stars} />
                        <BlockedForm />
                    </>
                );
            }}
        </WithAuthedUser>
    );
}
