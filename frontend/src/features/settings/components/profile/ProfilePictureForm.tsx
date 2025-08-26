"use client";

import { TrashIcon } from "@heroicons/react/24/outline";
import { ChangeEvent, useRef, useState } from "react";

import ProfilePicture from "@/features/profile/components/ProfilePicture";
import { useAuthedUser } from "@/features/auth/hooks/useSessionUser";
import { uploadProfilePicture } from "@/lib/apiClient";
import Button from "@/components/ui/Button";
import Card from "@/components/ui/Card";
import constants from "@/lib/constants";

const ProfilePictureForm = () => {
    const user = useAuthedUser();
    const inputRef = useRef<HTMLInputElement>(null);
    const [error, setError] = useState<string | null>(null);
    const [refreshKey, setRefreshKey] = useState(0);

    if (!user) return null;

    async function onUpload(
        event: ChangeEvent<HTMLInputElement>,
    ): Promise<void> {
        const files = event.target.files;
        if (!files || files.length === 0) return;

        const file = files[0];
        if (file.size > constants.PROFILE_PICTURE_MAX_SIZE) {
            setError("Profile picture is cannot exceed 2MB");
            return;
        }

        const { error } = await uploadProfilePicture({
            body: { File: file },
        });
        if (error) {
            console.error(error);
            setError(error.errors[0].description);
            return;
        }

        setRefreshKey((prev) => prev + 1);
        setError(null);
    }

    return (
        <Card className="w-full max-w-3xl gap-0">
            <section className="flex h-fit w-full gap-3">
                <ProfilePicture userId={user.userId} refreshKey={refreshKey} />
                <div className="flex flex-col justify-center gap-3">
                    <div className="flex items-center gap-3">
                        <input
                            type="file"
                            ref={inputRef}
                            onChange={onUpload}
                            hidden
                        />

                        <Button onClick={() => inputRef.current?.click()}>
                            Update Profile Picture
                        </Button>
                        <TrashIcon className="h-7 w-7" />
                    </div>
                    <p>Must be a valid image and cannot exceed 2MB</p>
                </div>
            </section>
            {error && <span className="text-error">{error}</span>}
        </Card>
    );
};
export default ProfilePictureForm;
