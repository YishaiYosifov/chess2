"use client";

import { Card } from "react-bootstrap";
import Image from "next/image";

import type { PublicProfile } from "@/lib/types";
import styles from "./Profile.module.scss";

import ProfilePicture from "../ProfilePicture";

/** Show basic information about a user */
const Profile = ({ profile }: { profile: PublicProfile }) => {
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
                        src="/assets/logo.svg"
                        width={40}
                        height={40}
                        className="rounded-1 img-fluid"
                        alt="Country"
                    />{" "}
                    {profile.username}
                </span>

                <textarea
                    className={`form-control ${styles.about}`}
                    readOnly
                    value={profile.about || ""}
                />
            </div>
        </Card>
    );
};
export default Profile;
