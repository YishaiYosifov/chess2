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

            userWhite: createUser(),
            userBlack: profile,

            timeControl: 900,
            increment: 1,

            results: GameResult.White,
            createdAt: Date.now().valueOf(),
        },
        {
            token: "789",

            userWhite: createUser(),
            userBlack: profile,

            timeControl: 900,
            increment: 1,

            results: GameResult.Draw,
            createdAt: Date.now().valueOf(),
        },
        {
            token: "101112",

            userWhite: profile,
            userBlack: createUser(),

            timeControl: 900,
            increment: 1,

            results: GameResult.Draw,
            createdAt: Date.now().valueOf(),
        },
    ];

    return (
        <>
            <div className="m-5 flex flex-col items-center gap-10 md:hidden">
                <Profile profile={profile} />
                <RatingScroll>
                    <RatingCard
                        ratingData={testRatingData}
                        className="w-min flex-none"
                    />
                    <RatingCard
                        ratingData={testRatingData}
                        className="w-min flex-none"
                    />
                    <RatingCard
                        ratingData={testRatingData}
                        className="w-min flex-none"
                    />
                </RatingScroll>

                <section className="w-full overflow-x-auto">
                    <GamesTable
                        games={testGamesData}
                        profileViewpoint={profile}
                    />
                </section>
            </div>

            <div className="hidden h-full max-w-4xl flex-col gap-10 p-10 md:flex">
                <Profile profile={profile} />
                <RatingCard ratingData={testRatingData} className="flex-none" />
                {/* <RatingScroll>
                    <RatingCard
                        ratingData={testRatingData}
                        className="flex-none"
                    />
                    <RatingCard
                        ratingData={testRatingData}
                        className="flex-none"
                    />
                    <RatingCard
                        ratingData={testRatingData}
                        className="flex-none"
                    />
                </RatingScroll> */}
                <GamesTable
                    className="max-w-4xl"
                    games={testGamesData}
                    profileViewpoint={profile}
                />
            </div>
        </>
    );
};
export default UserPage;

const RatingScroll = ({ children }: { children: React.ReactNode }) => {
    return (
        <section className="scrollbar flex flex-shrink-0 flex-nowrap gap-10 overflow-x-auto">
            {children}
        </section>
    );
};
