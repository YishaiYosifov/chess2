"use client";

import { Button } from "react-bootstrap";

import { useRef, useState, ChangeEvent } from "react";

import styles from "./ChangeProfilePicture.module.scss";
import { revalidateUser } from "@/app/actions";
import { useStore } from "@/zustand/store";

import ProfilePicture from "@/components/ProfilePicture";
import { settingsApi } from "@/lib/apis";
import { ResponseError } from "@/client";

const ChangeProfilePicture = () => {
    const { username, pfpLastChanged } = useStore.use.localProfile();

    const uploadPfpInput = useRef<HTMLInputElement>(null);
    const [lastChanged, setLastChanged] = useState(pfpLastChanged);
    const [status, setStatus] = useState("");

    const openFileSelector = () => uploadPfpInput.current?.click();

    async function uploadPfp(event: ChangeEvent<HTMLInputElement>) {
        const files = (event.target as HTMLInputElement).files;
        if (!files) return;

        try {
            await settingsApi.uploadProfilePicture({
                pfp: files[0],
            });
        } catch (err) {
            if (!(err instanceof ResponseError)) {
                setStatus("Something went wrong.");
                console.error(err);
                return;
            }

            switch (err.response.status) {
                case 400:
                case 413:
                    setStatus((await err.response.json()).detail);
                    break;
                default:
                    setStatus("Something went wrong.");
                    console.error(await err.response.text());
                    break;
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
