"use client";

import { TrashIcon } from "@heroicons/react/24/outline";
import { useRef, useState, ChangeEvent } from "react";

import { useAuthedProfile } from "@/features/auth/hooks/useAuthed";
import { revalidateUser } from "@/app/actions";
import constants from "@/lib/constants";

import ProfilePicture from "@/components/profile/ProfilePicture";
import { settingsApi } from "@/lib/apiClient";
import Button from "@/components/helpers/Button";
import Card from "@/components/helpers/Card";

const ProfilePictureSettings = () => {
    const { userName: username, pfpLastChanged } = useAuthedProfile();

    const uploadPfpInput = useRef<HTMLInputElement>(null);
    const [lastChanged, setLastChanged] = useState(pfpLastChanged);
    const [status, setStatus] = useState("");

    const openFileSelector = () => uploadPfpInput.current?.click();

    async function uploadPfp(event: ChangeEvent<HTMLInputElement>) {
        const files = (event.target as HTMLInputElement).files;
        if (!files) return;

        try {
            await settingsApi.uploadProfilePicture(files[0]);
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
        <Card className="flex-col items-center gap-3 sm:flex-row">
            <ProfilePicture username={username} lastChanged={lastChanged} />

            <section>
                <input
                    type="file"
                    accept=".jpg,.jpeg,.png,.webp"
                    ref={uploadPfpInput}
                    onChange={uploadPfp}
                    hidden
                    data-testid="pfpSettingsFileSelector"
                />

                <section className="flex items-center gap-2">
                    <Button
                        onClick={openFileSelector}
                        className="text-sm"
                        data-testid="pfpSettingsSubmit"
                    >
                        Update Profile Picture
                    </Button>
                    <TrashIcon className="text-secondary size-9" />

                    <span
                        className="text-error"
                        data-testid="pfpSettingsStatus"
                    >
                        {status}
                    </span>
                </section>

                <p>Must be JPEG, PNG or WEBP and cannot exceed 2MB</p>
            </section>
        </Card>
    );
};
export default ProfilePictureSettings;
