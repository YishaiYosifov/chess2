import { twMerge } from "tailwind-merge";
import Image from "next/image";

export interface ProfilePictureProps {
    userId: string;
    size?: number;
    className?: string;
    refreshKey?: number;
}

const ProfilePicture = ({
    userId,
    size = 120,
    className,
    refreshKey,
}: ProfilePictureProps) => {
    let url = `${process.env.NEXT_PUBLIC_API_URL}/api/Profile/profile-picture/${userId}`;
    if (refreshKey !== undefined) url += `?${refreshKey}`;

    return (
        <Image
            data-testid="profilePicture"
            className={twMerge("aspect-square rounded-md", className)}
            style={{ width: size, height: size }}
            alt="profile picture"
            src={url}
            width={size}
            height={size}
            unoptimized // we already cache with etag in the backend
        />
    );
};
export default ProfilePicture;
