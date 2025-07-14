import GamesTable from "@/features/profile/components/GamesTable";
import RatingCard from "@/features/profile/components/RatingsCard";
import Profile from "@/features/profile/components/Profile";
import { getGameResults, getRatingArchives, getUser } from "@/lib/apiClient";
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

    const lastMonth = new Date();
    lastMonth.setMonth(lastMonth.getMonth() - 1);
    lastMonth.setHours(0, 0, 0, 0);

    const { error: ratingsError, data: ratings } = await getRatingArchives({
        path: { userId: profile.userId },
        query: { since: lastMonth.toLocaleString() },
    });
    if (ratingsError || !ratings) {
        console.error(ratingsError);
        notFound();
    }
    const { error: gamesError, data: games } = await getGameResults({
        path: { userId: profile.userId },
        query: { Page: 0, PageSize: 10 },
    });
    if (gamesError || !games) {
        console.error(gamesError);
        notFound();
    }

    return (
        <div className="mx-auto flex w-full max-w-6xl flex-col gap-10 p-6">
            <Profile profile={profile} />

            <section className="flex flex-shrink-0 gap-10 overflow-x-auto">
                {ratings.map((rating, i) => (
                    <RatingCard ratingData={rating} key={i} />
                ))}
            </section>

            <section className="flex-shrink-0 overflow-x-auto">
                <GamesTable games={games.items} profileViewpoint={profile} />
            </section>
        </div>
    );
};
export default UserPage;
