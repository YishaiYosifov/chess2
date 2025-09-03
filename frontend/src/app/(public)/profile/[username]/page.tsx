import RatingCard from "@/features/profile/components/RatingsCard";
import Profile from "@/features/profile/components/Profile";
import {
    getGameResults,
    getHasStarred,
    getRatingArchives,
    getStarredUsers,
    getStarsReceivedCount,
    getUser,
    PublicUser,
    SessionUser,
} from "@/lib/apiClient";
import { notFound } from "next/navigation";
import constants from "@/lib/constants";
import EmptyRatingCard from "@/features/profile/components/EmptyRatingCard";
import GameHistory from "@/features/profile/components/GameHistory";
import WithOptionalAuthedUser from "@/features/auth/components/WithOptionalAuthedUser";
import { isAuthed } from "@/features/auth/lib/userGuard";

type Params = Promise<{ username: string }>;

export async function generateMetadata({ params }: { params: Params }) {
    const { username } = await params;
    return {
        title: `${username} - Chess 2 Profile - Chess 2`,
    };
}

export default async function ProfilePage({ params }: { params: Params }) {
    const { username } = await params;

    return (
        <WithOptionalAuthedUser>
            {async ({ user, accessToken }) => (
                <LoadProfilePage
                    loggedInUser={user}
                    accessToken={accessToken}
                    profileUsername={username}
                />
            )}
        </WithOptionalAuthedUser>
    );
}

async function LoadProfilePage({
    loggedInUser,
    accessToken,
    profileUsername,
}: {
    loggedInUser: SessionUser | null;
    accessToken: string | null;
    profileUsername: string;
}) {
    async function getProfile(): Promise<PublicUser> {
        if (
            loggedInUser &&
            isAuthed(loggedInUser) &&
            loggedInUser.userName === profileUsername
        )
            return loggedInUser;

        const { error: profileError, data: profile } = await getUser({
            path: { username: profileUsername },
        });
        if (profileError || profile === undefined) {
            console.error(profileError);
            notFound();
        }
        return profile;
    }
    const profile = await getProfile();

    const lastMonth = new Date();
    lastMonth.setMonth(lastMonth.getMonth() - 1);
    lastMonth.setHours(0, 0, 0, 0);

    const [ratings, games, starred, starsCount, hasStarred] = await Promise.all(
        [
            requireData(
                getRatingArchives({
                    path: { userId: profile.userId },
                    query: { since: lastMonth.toLocaleString() },
                }),
            ),
            requireData(
                getGameResults({
                    path: { userId: profile.userId },
                    query: {
                        Page: 0,
                        PageSize: constants.PAGINATION_PAGE_SIZE.GAME_SUMMARY,
                    },
                }),
            ),
            requireData(
                getStarredUsers({
                    path: { userId: profile.userId },
                    query: {
                        Page: 0,
                        PageSize: constants.PAGINATION_PAGE_SIZE.STARS,
                    },
                }),
            ),
            requireData(
                getStarsReceivedCount({
                    path: { starredUserId: profile.userId },
                }),
            ),
            (async (): Promise<boolean> => {
                if (!accessToken || profile.userId === loggedInUser?.userId)
                    return false;

                return requireData(
                    getHasStarred({
                        path: { starredUserId: profile.userId },
                        auth: () => accessToken,
                    }),
                );
            })(),
        ],
    );

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
}

async function requireData<T>(
    promise: Promise<{ error?: unknown; data?: T }>,
): Promise<T> {
    const { error, data } = await promise;
    if (error || data === undefined) {
        console.error(error);
        notFound();
    }
    return data;
}
