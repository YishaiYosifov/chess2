"use client";

import ProfilePicture from "./ProfilePicture";
import Card from "@/components/ui/Card";
import { PublicUser } from "@/lib/apiClient";
import Flag from "./Flag";
import Button from "@/components/ui/Button";

const Profile = ({ profile }: { profile: PublicUser }) => {
    return (
        <Card className="flex flex-col gap-4 p-4 sm:flex-row sm:items-start">
            <ProfilePicture
                className="mx-auto sm:mx-0 sm:self-start"
                userId={profile.userId}
            />

            <section className="flex flex-1 flex-col gap-3">
                <div className="flex flex-col gap-2 sm:flex-row sm:items-center sm:justify-between">
                    <span
                        className="flex items-center gap-3 overflow-hidden"
                        data-testid="username"
                    >
                        <span className="truncate text-2xl sm:text-3xl">
                            {profile.userName}
                        </span>
                        <Flag size={32} countryCode={profile.countryCode} />
                    </span>

                    <div className="flex flex-wrap gap-2 sm:gap-3">
                        <Button className="bg-secondary min-w-[100px] flex-1 truncate text-black">
                            Challenge
                        </Button>
                        <Button className="min-w-[100px] flex-1 truncate">
                            Add Friend
                        </Button>
                    </div>
                </div>

                <p
                    className="text-text/70 text-sm break-words sm:text-base"
                    data-testid="aboutMe"
                >
                    {profile.about}
                </p>
            </section>
        </Card>
    );
};
export default Profile;
