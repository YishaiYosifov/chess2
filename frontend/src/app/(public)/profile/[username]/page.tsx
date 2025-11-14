import WithOptionalAuthedUser from "@/features/auth/hocs/WithOptionalAuthedUser";
import LoadProfilePage from "@/features/profile/components/LoadProfilePage";

type Params = Promise<{ username: string }>;

export async function generateMetadata({ params }: { params: Params }) {
    const { username } = await params;
    return {
        title: `${username} - Anarchy Chess Profile - Anarchy Chess`,
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
