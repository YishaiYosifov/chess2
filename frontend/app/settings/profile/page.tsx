import { LocalProfile } from "@/zustand/slices/authSlice";

import ChangeProfilePicture from "@/components/pages/settings/ChangeProfilePicture";
import SettingsForm from "@/components/pages/settings/profile/ProfileForm";
import withAuth from "@/components/hocs/withAuth";
import StoreInitializer from "@/components/StoreInitializer";

const ProfilePage = withAuth(async ({ profile }: { profile: LocalProfile }) => {
    const countries = await (
        await fetch(`${process.env.NEXT_PUBLIC_API_URL}/static/countries.json`)
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
});
export default ProfilePage;
