import RatingCard from "@/features/profile/components/RatingsCard";
import Profile from "@/features/profile/components/Profile";
import { getGameResults, getRatingArchives, getUser } from "@/lib/apiClient";
import { notFound } from "next/navigation";
import constants from "@/lib/constants";
import EmptyRatingCard from "@/features/profile/components/EmptyRatingCard";
import GameHistory from "@/features/profile/components/GameHistory";

type Params = Promise<{ username: string }>;

export async function generateMetadata({ params }: { params: Params }) {
    const { username } = await params;
    return {
        title: `${username} - Chess 2 Profile - Chess 2`,
    };
}

const UserPage = async ({ params }: { params: Params }) => {
    const { username } = await params;

    const { error: profileError, data: profile } = await getUser({
        path: { username },
    });
    if (profileError || !profile) {
        console.error(profileError);
        notFound();
    }

    const lastMonth = new Date();
    lastMonth.setMonth(lastMonth.getMonth() - 1);
    lastMonth.setHours(0, 0, 0, 0);

    const [ratingsResult, gamesResult] = await Promise.all([
        getRatingArchives({
            path: { userId: profile.userId },
            query: { since: lastMonth.toLocaleString() },
        }),
        getGameResults({
            path: { userId: profile.userId },
            query: {
                Page: 0,
                PageSize: constants.PAGINATION_PAGE_SIZE.GAME_SUMMARY,
            },
        }),
    ]);

    const { error: ratingsError, data: ratings } = ratingsResult;
    if (ratingsError || !ratings) {
        console.error(ratingsError);
        notFound();
    }

    const { error: gamesError, data: games } = gamesResult;
    if (gamesError || !games) {
        console.error(gamesError);
        notFound();
    }

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
            <Profile profile={profile} />

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
export default UserPage;
