"use client";

import { User } from "@/lib/apiClient/models";
import Card from "../helpers/Card";
import ProfilePicture from "./ProfilePicture";
import Flag from "./Flag";

/** Show basic information about a user */
const Profile = ({ profile }: { profile: User }) => {
    return (
        <Card className="min-w-0 gap-3">
            <ProfilePicture />
            <section className="flex min-w-0 flex-col">
                <div className="flex gap-3">
                    <span className="overflow-hidden text-ellipsis whitespace-nowrap text-3xl">
                        {profile.username}
                    </span>
                    <Flag size={40} countryCode={profile.countryCode} />
                </div>
                <p className="text-text/70">{profile.about}</p>
            </section>
        </Card>
    );
};
export default Profile;
