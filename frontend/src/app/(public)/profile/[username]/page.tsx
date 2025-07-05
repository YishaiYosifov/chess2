import GamesTable from "@/features/profile/components/GamesTable";
import RatingCard from "@/features/profile/components/RatingsCard";
import Profile from "@/features/profile/components/Profile";
import { FinishedGame, GameResult, RatingOverview } from "@/types/tempModels";
import { notFound } from "next/navigation";
import { createUser } from "@/lib/testUtils/fakers/userFaker";
import { getUser } from "@/lib/apiClient";

type Params = Promise<{ username: string }>;

export async function generateMetadata({ params }: { params: Params }) {
    const { username } = await params;
    return {
        title: `${username} - Chess 2 Profile - Chess 2`,
    };
}

const UserPage = async ({ params }: { params: Params }) => {
    const { username } = await params;

    const { error, data: profile } = await getUser({ path: { username } });
    if (error || !profile) {
        console.error(error);
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

    const testGamesData: FinishedGame[] = [
        {
            token: "123",

            userWhite: profile,
            userBlack: createUser(),

            timeControl: 900,
            increment: 1,

            results: GameResult.WHITE_WIN,
            createdAt: Date.now().valueOf(),
        },
        {
            token: "456",

            userWhite: createUser(),
            userBlack: profile,

            timeControl: 900,
            increment: 1,

            results: GameResult.WHITE_WIN,
            createdAt: Date.now().valueOf(),
        },
        {
            token: "789",

            userWhite: createUser(),
            userBlack: profile,

            timeControl: 900,
            increment: 1,

            results: GameResult.DRAW,
            createdAt: Date.now().valueOf(),
        },
        {
            token: "101112",

            userWhite: profile,
            userBlack: createUser(),

            timeControl: 900,
            increment: 1,

            results: GameResult.DRAW,
            createdAt: Date.now().valueOf(),
        },
    ];

    return (
        <div className="flex w-screen max-w-4xl flex-col gap-10 p-10">
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
                <GamesTable games={testGamesData} profileViewpoint={profile} />
            </section>
        </div>
    );
};
export default UserPage;
