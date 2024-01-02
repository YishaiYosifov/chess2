import { apiConfig } from "@/lib/apis";

import ChangeProfilePicture from "@/components/settings/ChangeProfilePicture";
import SettingsForm from "@/components/settings/profile/ProfileForm";
import StoreInitializer from "@/components/StoreInitializer";
import withAuth from "@/components/hocs/withAuth";
import { PrivateUserOut } from "@/client";

const ProfilePage = withAuth(
    async ({ profile }: { profile: PrivateUserOut }) => {
        const countries = await (
            await fetch(`${apiConfig.basePath}/static/countries.json`)
        ).json();

        return (
            <div style={{ maxWidth: "1426px" }}>
                <StoreInitializer
                    values={{ localProfile: profile }}
                    action="SET_LOCAL_PROFILE"
                />
                <section className="mt-4">
                    <div className="card">
                        <ChangeProfilePicture />
                    </div>
                </section>

                <section className="mt-4">
                    <div className="card">
                        <SettingsForm />
                    </div>
                </section>
            </div>
        );
    }
);
export default ProfilePage;
