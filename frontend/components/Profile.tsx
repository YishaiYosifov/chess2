"use client";

import { Card } from "react-bootstrap";
import Image from "next/image";

import type { PublicProfile } from "@/lib/types";
import logo from "../public/assets/logo.svg";
import styles from "./Profile.module.scss";

/** Show basic information about a user */
const Profile = ({ profile }: { profile: PublicProfile }) => {
    return (
        <Card className={styles.profile}>
            <div className={styles["info"]}>
                <Image
                    src={`${process.env.NEXT_PUBLIC_API_URL}/api/profile/${profile.username}/profile-picture?${profile.pfpLastChanged}`}
                    width={200}
                    height={200}
                    className={styles["profile-picture"]}
                    alt="User Profile Picture"
                />
                <div style={{ width: "100%" }}>
                    <h4>
                        <Image
                            src={logo}
                            width={40}
                            height={40}
                            className="rounded-1 img-fluid"
                            alt="Country"
                        />{" "}
                        {profile.username}
                    </h4>

                    <textarea
                        className={`form-control ${styles.about}`}
                        readOnly
                        value={profile.about || ""}
                    />
                </div>
            </div>
        </Card>
    );
};
export default Profile;
