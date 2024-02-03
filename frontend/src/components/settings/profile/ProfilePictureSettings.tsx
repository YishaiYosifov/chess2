"use client";

import { Button } from "react-bootstrap";

import { useRef, useState, ChangeEvent } from "react";

import { useAuthedProfile } from "../../contexts/AuthContext";
import styles from "./ProfilePictureSettings.module.scss";
import { revalidateUser } from "@/app/actions";
import { settingsApi } from "@/lib/apis";
import constants from "@/lib/constants";

import ProfilePicture from "@/components/ProfilePicture";

const ProfilePictureSettings = () => {
    const { username, pfpLastChanged } = useAuthedProfile();

    const uploadPfpInput = useRef<HTMLInputElement>(null);
    const [lastChanged, setLastChanged] = useState(pfpLastChanged);
    const [status, setStatus] = useState("");

    const openFileSelector = () => uploadPfpInput.current?.click();

    async function uploadPfp(event: ChangeEvent<HTMLInputElement>) {
        const files = (event.target as HTMLInputElement).files;
        if (!files) return;

        const a = settingsApi;

        try {
            await settingsApi.uploadProfilePicture({
                pfp: files[0],
            });
        } catch (err: any) {
            switch (err?.response?.status) {
                case 400:
                case 413:
                    setStatus((await err.response.json()).detail);
                    break;
                default:
                    setStatus(constants.GENERIC_ERROR);
                    console.error(err);
            }
            return;
        }

        setStatus("");
        setLastChanged(new Date());
        revalidateUser(username);
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
                    data-testid="pfpSettingsFileSelector"
                />

                <div className={styles["upload-button-container"]}>
                    <Button
                        variant="dark"
                        onClick={openFileSelector}
                        data-testid="pfpSettingsSubmit"
                    >
                        Update Profile Picture
                    </Button>
                    <span
                        className="text-invalid"
                        data-testid="pfpSettingsStatus"
                    >
                        {status}
                    </span>
                </div>
                <p>Must be JPEG, PNG, WEBP or GIF and cannot exceed 1MB</p>
            </div>
        </div>
    );
};
export default ProfilePictureSettings;
