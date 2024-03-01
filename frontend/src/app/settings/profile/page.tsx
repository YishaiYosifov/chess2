import { PrivateAuthedProfileOut } from "@/client";

import ProfilePictureSettings from "@/components/settings/profile/ProfilePictureSettings";
import ProfileSettings from "@/components/settings/profile/ProfileSettings";
import StoreInitializer from "@/components/StoreInitializer";
import withAuth from "@/components/hocs/withAuth";

export const metadata = { title: "Profile - Settings - Chess 2" };

const ProfilePage = withAuth(
    async ({ profile }: { profile: PrivateAuthedProfileOut }) => {
        return (
            <div style={{ maxWidth: "1426px" }}>
                <StoreInitializer
                    values={{ localProfile: profile }}
                    action="SET_LOCAL_PROFILE"
                />

                <section className="mt-4">
                    <div className="card">
                        <ProfilePictureSettings />
                    </div>
                </section>

                <section className="mt-4">
                    <div className="card">
                        <ProfileSettings />
                    </div>
                </section>
            </div>
        );
    }
);
export default ProfilePage;
