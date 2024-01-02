"use client";

import { Card } from "react-bootstrap";
import Image from "next/image";

import styles from "./Profile.module.scss";

import ProfilePicture from "../ProfilePicture";
import type { PublicUserOut } from "@/client";

/** Show basic information about a user */
const Profile = ({ profile }: { profile: PublicUserOut }) => {
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
                    <Image
                        data-testid="countryImage"
                        src="/assets/logo.svg"
                        width={40}
                        height={40}
                        className="rounded-1 img-fluid"
                        alt="Country"
                    />{" "}
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
