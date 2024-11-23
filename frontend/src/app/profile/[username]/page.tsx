import GamesTable from "@/components/profile/GamesTable";
import RatingCard from "@/components/profile/RatingsCard";
import { profileApi } from "@/lib/apiClient/client";
import Profile from "@/components/profile/Profile";
import {
    FinishedGame,
    GameResult,
    RatingOverview,
    User,
} from "@/lib/apiClient/models";
import { notFound } from "next/navigation";
import { createUser } from "@/lib/testUtils/fakers/userFaker";

type Params = Promise<{ username: string }>;

export async function generateMetadata({ params }: { params: Params }) {
    const { username } = await params;
    return {
        title: `${username} - Chess 2 Profile - Chess 2`,
    };
}

const UserPage = async ({ params }: { params: Params }) => {
    const { username } = await params;

    let profile: User;
    try {
        profile = await profileApi.getUser(username);
    } catch {
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

            results: GameResult.White,
            createdAt: Date.now().valueOf(),
        },
        {
            token: "456",

            userBlack: profile,
            userWhite: createUser(),

            timeControl: 900,
            increment: 1,

            results: GameResult.White,
            createdAt: Date.now().valueOf(),
        },
        {
            token: "789",

            userBlack: profile,
            userWhite: createUser(),

            timeControl: 900,
            increment: 1,

            results: GameResult.Draw,
            createdAt: Date.now().valueOf(),
        },
    ];

    return (
        <div className="mx-5 mt-5 flex flex-col gap-10">
            <Profile profile={profile} />
            <RatingCard ratingData={testRatingData} />
            <GamesTable games={testGamesData} profileViewpoint={profile} />
        </div>
    );
};
export default UserPage;
