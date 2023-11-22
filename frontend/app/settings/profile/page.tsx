import { LocalProfile } from "@/zustand/slices/authSlice";

import ChangeProfilePicture from "@/components/settings/ChangeProfilePicture";
import SettingsForm from "@/components/settings/profile/ProfileForm";
import StoreInitializer from "@/components/StoreInitializer";
import withAuth from "@/components/hocs/withAuth";

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
