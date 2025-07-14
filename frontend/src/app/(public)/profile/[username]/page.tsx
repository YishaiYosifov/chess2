import GamesTable from "@/features/profile/components/GamesTable";
import RatingCard from "@/features/profile/components/RatingsCard";
import Profile from "@/features/profile/components/Profile";
import { getGameResults, getUser } from "@/lib/apiClient";
import { RatingOverview } from "@/types/tempModels";
import { notFound } from "next/navigation";

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

    const { error: gamesError, data: games } = await getGameResults({
        path: { userId: profile?.userId },
        query: { Page: 0, PageSize: 10 },
    });
    if (gamesError || !games) {
        console.error(gamesError);
        notFound();
    }

    const testRatingData: RatingOverview = {
        max: 6969,
        current: 420,
        history: [
            {
                elo: 69,
                achievedAt: new Date(2024, 10, 21).getTime(),
            },
            {
                elo: 420,
                achievedAt: new Date(2024, 10, 22).getTime(),
            },
        ],
    };

    return (
        <div className="mx-auto flex w-full max-w-6xl flex-col gap-10 p-6">
            <Profile profile={profile} />

            <section className="flex flex-shrink-0 gap-10 overflow-x-auto">
                <RatingCard ratingData={testRatingData} className="min-w-96" />
                <RatingCard ratingData={testRatingData} className="min-w-96" />
                <RatingCard ratingData={testRatingData} className="min-w-96" />
                <RatingCard ratingData={testRatingData} className="min-w-96" />
                <RatingCard ratingData={testRatingData} className="min-w-96" />
                <RatingCard ratingData={testRatingData} className="min-w-96" />
                <RatingCard ratingData={testRatingData} className="min-w-96" />
                <RatingCard ratingData={testRatingData} className="min-w-96" />
            </section>

            <section className="flex-shrink-0 overflow-x-auto">
                <GamesTable games={games.items} profileViewpoint={profile} />
            </section>
        </div>
    );
};
export default UserPage;
