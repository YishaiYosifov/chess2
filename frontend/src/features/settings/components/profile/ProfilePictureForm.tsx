"use client";

import { TrashIcon } from "@heroicons/react/24/outline";

import ProfilePicture from "@/features/profile/components/ProfilePicture";
import Button from "@/components/ui/Button";
import Card from "@/components/ui/Card";
import { useAuthedUser } from "@/features/auth/hooks/useSessionUser";

const ProfilePictureForm = () => {
    const user = useAuthedUser();
    if (!user) return null;

    return (
        <Card className="w-full max-w-3xl">
            <section className="flex h-fit w-full gap-3">
                <ProfilePicture userId={user.userId} />
                <div className="flex flex-col justify-center gap-3">
                    <div className="flex items-center gap-3">
                        <Button>Update Profile Picture</Button>
                        <TrashIcon className="h-7 w-7" />
                    </div>
                    <p>Must be a valid image and cannot exceed 2MB</p>
                </div>
            </section>
        </Card>
    );
};
export default ProfilePictureForm;
