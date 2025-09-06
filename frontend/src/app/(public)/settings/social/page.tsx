import {
    getBlockedUsers,
    getPreferences,
    getStarredUsers,
} from "@/lib/apiClient";

import BlockedForm from "@/features/settings/components/social/BlockedForm";
import PrivacyForm from "@/features/settings/components/social/PrivacyForm";
import StarsForm from "@/features/settings/components/social/StarsForm";
import WithAuthedUser from "@/features/auth/components/WithAuthedUser";
import dataOrThrow from "@/lib/apiClient/dataOrThrow";
import constants from "@/lib/constants";

export const metadata = { title: "Social Settings - Chess 2" };

export default async function SocialPage() {
    return (
        <WithAuthedUser>
            {async ({ user, accessToken }) => {
                const [preferences, stars, blocked] = await Promise.all([
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
                    dataOrThrow(
                        getBlockedUsers({
                            query: {
                                Page: 0,
                                PageSize:
                                    constants.PAGINATION_PAGE_SIZE.BLOCKED,
                            },
                            auth: () => accessToken,
                        }),
                    ),
                ]);

                return (
                    <>
                        <PrivacyForm initialPreferences={preferences} />
                        <StarsForm initialStars={stars} />
                        <BlockedForm initialBlocked={blocked} />
                    </>
                );
            }}
        </WithAuthedUser>
    );
}
