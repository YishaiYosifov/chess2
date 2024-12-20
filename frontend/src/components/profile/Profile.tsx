"use client";

import clsx from "clsx";

import { User } from "@/lib/apiClient/models";
import ProfilePicture from "./ProfilePicture";
import Card from "../helpers/Card";
import Flag from "./Flag";

/** Show basic information about a user */
const Profile = ({
    profile,
    className,
}: {
    profile: User;
    className?: string;
}) => {
    return (
        <Card className={clsx("w-full flex-col gap-3 sm:flex-row", className)}>
            <ProfilePicture
                className="self-center"
                username={profile.username}
                lastChanged={profile.pfpLastChanged}
            />
            <section className="flex min-w-0 flex-col gap-3">
                <div className="flex gap-3">
                    <span
                        className="overflow-hidden text-ellipsis whitespace-nowrap text-3xl"
                        data-testid="username"
                    >
                        {profile.username}
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
