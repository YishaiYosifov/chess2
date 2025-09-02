"use client";

import { StarIcon as StarIconOutline } from "@heroicons/react/24/outline";
import { StarIcon as StarIconSolid } from "@heroicons/react/24/solid";

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

            <section className="flex min-w-0 flex-1 flex-col items-center gap-3 sm:items-start sm:gap-1">
                <div className="flex w-full flex-col gap-2 sm:flex-row sm:items-center sm:justify-between">
                    <span
                        className="flex flex-1 items-center justify-center gap-3 overflow-hidden sm:justify-start"
                        data-testid="username"
                    >
                        <span className="min-w-0 truncate text-2xl sm:text-3xl">
                            {profile.userName}
                        </span>
                        <Flag size={32} countryCode={profile.countryCode} />
                    </span>

                    <div className="flex flex-wrap gap-3 sm:gap-3">
                        {/* <StarIconOutline className="h-8 w-8 text-amber-300" /> */}
                        <StarIconSolid className="h-8 w-8 text-amber-300" />
                        <Button className="bg-secondary min-w-[100px] flex-1 truncate text-black">
                            Challenge
                        </Button>
                    </div>
                </div>

                <p
                    className="text-text/70 max-w-full text-sm break-words"
                    data-testid="aboutMe"
                >
                    {profile.about}
                </p>
            </section>
        </Card>
    );
};
export default Profile;
