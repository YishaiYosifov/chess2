import { twMerge } from "tailwind-merge";
import Image from "next/image";

export interface ProfilePictureProps {
    userId: string;
    width?: number;
    height?: number;
    className?: string;
    refreshKey?: number;
}

const ProfilePicture = ({
    userId,
    width = 120,
    height = 120,
    className,
    refreshKey,
}: ProfilePictureProps) => {
    let url = `${process.env.NEXT_PUBLIC_API_URL}/api/Profile/profile-picture/${userId}`;
    if (refreshKey !== undefined) url += `?${refreshKey}`;

    return (
        <Image
            data-testid="profilePicture"
            className={twMerge("aspect-square rounded-md", className)}
            alt="profile picture"
            src={url}
            width={width}
            height={height}
            unoptimized // we already cache with etag in the backend
        />
    );
};
export default ProfilePicture;
