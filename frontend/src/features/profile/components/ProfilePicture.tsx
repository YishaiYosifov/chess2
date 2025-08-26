import { twMerge } from "tailwind-merge";
import Image from "next/image";

export interface ProfilePictureProps {
    userId: string;
    width?: number;
    height?: number;
    className?: string;
}

const ProfilePicture = ({
    userId,
    width = 120,
    height = 120,
    className,
}: ProfilePictureProps) => {
    return (
        <Image
            data-testid="profilePicture"
            className={twMerge("aspect-square rounded-md", className)}
            alt="profile picture"
            src={`${process.env.NEXT_PUBLIC_API_URL}/api/Profile/profile-picture/${userId}`}
            width={width}
            height={height}
            unoptimized // we already cache with etag in the backend
        />
    );
};
export default ProfilePicture;
