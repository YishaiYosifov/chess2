"use client";

import { Card } from "react-bootstrap";

import styles from "./Profile.module.scss";

import ProfilePicture from "../ProfilePicture";
import type { AuthedProfileOut } from "@/client";
import countries from "@/data/countries.json";
import { TypedCountries } from "@/lib/types";

/** Show basic information about a user */
const Profile = ({ profile }: { profile: AuthedProfileOut }) => {
    return (
        <Card className={styles.profile}>
            <ProfilePicture
                username={profile.username}
                width={250}
                height={250}
                className={styles["profile-picture"]}
                lastChanged={profile.pfpLastChanged}
            />

            <div className={styles.info}>
                <span>
                    {(countries as TypedCountries)[profile.countryAlpha3].flag}{" "}
                    {profile.username}
                </span>

                <textarea
                    data-testid="aboutArea"
                    className={`form-control ${styles.about}`}
                    readOnly
                    value={profile.about || ""}
                />
            </div>
        </Card>
    );
};
export default Profile;
