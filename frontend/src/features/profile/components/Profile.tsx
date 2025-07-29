"use client";

import ProfilePicture from "./ProfilePicture";
import Card from "@/components/ui/Card";
import { PublicUser } from "@/lib/apiClient";
import Flag from "./Flag";

const Profile = ({ profile }: { profile: PublicUser }) => {
    return (
        <Card className={"w-full flex-col gap-3 sm:flex-row"}>
            <ProfilePicture className="self-center" userId={profile.userId} />
            <section className="flex min-w-0 flex-col gap-3">
                <div className="flex gap-3">
                    <span
                        className="overflow-hidden text-3xl text-ellipsis whitespace-nowrap"
                        data-testid="username"
                    >
                        {profile.userName}
                    </span>
                    <Flag size={40} countryCode={profile.countryCode} />
                </div>
                <p className="text-text/70" data-testid="aboutMe">
                    {profile.about}
                </p>
            </section>
        </Card>
    );
};
export default Profile;
