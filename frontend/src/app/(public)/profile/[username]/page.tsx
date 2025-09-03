import RatingCard from "@/features/profile/components/RatingsCard";
import Profile from "@/features/profile/components/Profile";
import {
    getGameResults,
    getRatingArchives,
    getStars,
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
            {async ({ user }) => (
                <LoadProfilePage
                    loggedInUser={user}
                    profileUsername={username}
                />
            )}
        </WithOptionalAuthedUser>
    );
}

async function LoadProfilePage({
    loggedInUser,
    profileUsername,
}: {
    loggedInUser: SessionUser | null;
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
        if (profileError || !profile) {
            console.error(profileError);
            notFound();
        }
        return profile;
    }
    const profile = await getProfile();

    const lastMonth = new Date();
    lastMonth.setMonth(lastMonth.getMonth() - 1);
    lastMonth.setHours(0, 0, 0, 0);

    const [ratings, games, stars] = await Promise.all([
        safeFetch(() =>
            getRatingArchives({
                path: { userId: profile.userId },
                query: { since: lastMonth.toLocaleString() },
            }),
        ),
        safeFetch(() =>
            getGameResults({
                path: { userId: profile.userId },
                query: {
                    Page: 0,
                    PageSize: constants.PAGINATION_PAGE_SIZE.GAME_SUMMARY,
                },
            }),
        ),
        safeFetch(() =>
            getStars({
                path: { userId: profile.userId },
                query: {
                    Page: 0,
                    PageSize: constants.PAGINATION_PAGE_SIZE.STARS,
                },
            }),
        ),
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
            <Profile profile={profile} initialStarCount={stars.totalCount} />

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

async function safeFetch<T>(
    fn: () => Promise<{ error?: unknown; data?: T }>,
): Promise<T> {
    const { error, data } = await fn();
    if (error || !data) {
        console.error(error);
        notFound();
    }
    return data;
}
