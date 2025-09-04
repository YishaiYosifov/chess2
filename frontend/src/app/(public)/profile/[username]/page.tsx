import WithOptionalAuthedUser from "@/features/auth/components/WithOptionalAuthedUser";
import LoadProfilePage from "@/features/profile/components/LoadProfilePage";

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
