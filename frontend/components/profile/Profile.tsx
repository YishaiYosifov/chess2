"use client";

import { Card } from "react-bootstrap";
import Image from "next/image";

import type { PublicProfile } from "@/lib/types";
import logo from "@/public/assets/logo.svg";
import styles from "./Profile.module.scss";

/** Show basic information about a user */
const Profile = ({ profile }: { profile: PublicProfile }) => {
    return (
        <Card className={styles.profile}>
            <Image
                src={`${process.env.NEXT_PUBLIC_API_URL}/profile/${profile.username}/profile-picture?${profile.pfpLastChanged}`}
                width={250}
                height={250}
                className={styles["profile-picture"]}
                alt="User Profile Picture"
            />
            <div className={styles.info}>
                <span>
                    <Image
                        src={logo}
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
