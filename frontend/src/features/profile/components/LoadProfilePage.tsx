import RatingCard from "@/features/profile/components/RatingsCard";
import Profile from "@/features/profile/components/Profile";
import {
    getGameResults,
    getHasStarred,
    getRatingArchives,
    getStarsReceivedCount,
    getUser,
    PublicUser,
    SessionUser,
} from "@/lib/apiClient";
import constants from "@/lib/constants";
import EmptyRatingCard from "@/features/profile/components/EmptyRatingCard";
import GameHistory from "@/features/profile/components/GameHistory";
import { isAuthed } from "@/features/auth/lib/userGuard";
import dataOrThrow from "@/lib/apiClient/dataOrThrow";

const LoadProfilePage = async ({
    loggedInUser,
    accessToken,
    profileUsername,
}: {
    loggedInUser: SessionUser | null;
    accessToken: string | null;
    profileUsername: string;
}) => {
    async function getProfile(): Promise<PublicUser> {
        if (
            loggedInUser &&
            isAuthed(loggedInUser) &&
            loggedInUser.userName === profileUsername
        )
            return loggedInUser;

        const profile = await dataOrThrow(
            getUser({
                path: { username: profileUsername },
            }),
        );

        return profile;
    }
    const profile = await getProfile();

    const lastMonth = new Date();
    lastMonth.setMonth(lastMonth.getMonth() - 1);
    lastMonth.setHours(0, 0, 0, 0);

    const [ratings, games, starsCount, hasStarred] = await Promise.all([
        dataOrThrow(
            getRatingArchives({
                path: { userId: profile.userId },
                query: { since: lastMonth.toLocaleString() },
            }),
        ),
        dataOrThrow(
            getGameResults({
                path: { userId: profile.userId },
                query: {
                    Page: 0,
                    PageSize: constants.PAGINATION_PAGE_SIZE.GAME_SUMMARY,
                },
            }),
        ),
        dataOrThrow(
            getStarsReceivedCount({
                path: { starredUserId: profile.userId },
            }),
        ),
        (async (): Promise<boolean> => {
            if (!accessToken || profile.userId === loggedInUser?.userId)
                return false;

            return dataOrThrow(
                getHasStarred({
                    path: { starredUserId: profile.userId },
                    auth: () => accessToken,
                }),
            );
        })(),
    ]);

    const ratingCards = constants.DISPLAY_TIME_CONTROLS.map((timeControl) => {
        const overview = ratings.find((x) => x.timeControl === timeControl);
        return overview ? (
            <RatingCard key={timeControl} overview={overview} />
        ) : (
            <EmptyRatingCard key={timeControl} timeControl={timeControl} />
        );
    });

    return (
        <div className="mx-auto flex w-full max-w-6xl flex-col gap-5 p-6">
            <Profile
                profile={profile}
                initialStarCount={starsCount}
                initialHasStarred={hasStarred}
            />

            <section className="flex flex-shrink-0 gap-5 overflow-x-auto">
                {ratingCards}
            </section>

            <GameHistory
                initialGameResults={games}
                profileViewpoint={profile}
            />
        </div>
    );
};
export default LoadProfilePage;
