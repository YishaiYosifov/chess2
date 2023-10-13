import Image from "next/image";

import styles from "./Profile.module.scss";
import { PublicProfile } from "@/lib/types";

import logo from "../public/assets/logo.svg";

const Profile = ({ profile }: { profile: PublicProfile }) => {
    return (
        <div className="card">
            <div className="card-body row">
                <div className="col-lg-4 d-flex align-items-center justify-content-center">
                    <Image
                        src={`${process.env.NEXT_PUBLIC_API_URL}/api/profile/${profile.username}/profile-picture?${profile.pfpLastChanged}`}
                        height={220}
                        width={220}
                        className={`rounded border border-white border-3 img-fluid ${styles["profile-picture"]}`}
                        alt="User Profile Picture"
                    />
                </div>
                <div className="col">
                    <h4 className="mt-3">
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
        </div>
    );
};
export default Profile;
