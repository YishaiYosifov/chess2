import Profile from "@/components/profile/Profile";
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
        <div className="mx-5 grid grid-rows-3 gap-10">
            <Profile profile={profile} />
        </div>
    );
};
export default UserPage;
