"use client";

import { Button } from "react-bootstrap";

import { useRef, useState, ChangeEvent } from "react";

import styles from "./ChangeProfilePicture.module.scss";
import { apiRequest } from "@/lib/utils/common";
import { revalidateUser } from "@/app/actions";
import { useStore } from "@/zustand/store";
import ProfilePicture from "@/components/ProfilePicture";

const ChangeProfilePicture = () => {
    const { username, pfpLastChanged, userId } = useStore.use.localProfile();

    const uploadPfpInput = useRef<HTMLInputElement>(null);
    const [lastChanged, setLastChanged] = useState(pfpLastChanged);
    const [status, setStatus] = useState("");

    const openFileSelector = () => uploadPfpInput.current?.click();

    async function uploadPfp(event: ChangeEvent<HTMLInputElement>) {
        const files = (event.target as HTMLInputElement).files;
        if (!files) return;

        const data = new FormData();
        data.append("pfp", files[0]);

        const uploadResponse = await apiRequest(
            "/profile/upload-profile-picture",
            {
                body: data,
                method: "PUT",
            }
        );
        const uploadData = await uploadResponse.json();

        switch (uploadResponse.status) {
            case 201:
                setStatus("");
                setLastChanged(new Date().toISOString());
                revalidateUser(username);
                break;
            case 400:
                setStatus(uploadData.data.pfp[0]);
                break;
            default:
                setStatus("Something went wrong.");
                console.error(uploadData);
                break;
        }
    }

    return (
        <div className={styles.container}>
            <ProfilePicture
                username={username}
                lastChanged={lastChanged}
                className={styles["profile-picture"]}
            />
            <div className={styles["upload-container"]}>
                <input
                    type="file"
                    accept=".jpg,.jpeg,.png,.webp,.gif"
                    ref={uploadPfpInput}
                    onChange={uploadPfp}
                    hidden
                />

                <div className={styles["upload-button-container"]}>
                    <Button variant="dark" onClick={openFileSelector}>
                        Update Profile Picture
                    </Button>
                    <span className="text-invalid">{status}</span>
                </div>
                <p>Must be JPEG, PNG, WEBP or GIF and cannot exceed 1MB</p>
            </div>
        </div>
    );
};
export default ChangeProfilePicture;
