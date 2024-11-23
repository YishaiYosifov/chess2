import Profile from "@/components/profile/Profile";
import RatingCard from "@/components/profile/RatingsCard";
import { profileApi } from "@/lib/apiClient/client";
import { User } from "@/lib/apiClient/models";
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

    let profile: User;
    try {
        profile = await profileApi.getUser(username);
    } catch {
        notFound();
    }

    return (
        <div className="mx-5 flex flex-col gap-10">
            <Profile profile={profile} />
            <RatingCard
                ratingData={{
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
                }}
            />
        </div>
    );
};
export default UserPage;
