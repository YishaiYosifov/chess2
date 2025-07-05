import { twMerge } from "tailwind-merge";
import Image from "next/image";

export interface ProfilePictureProps {
    userId?: string;
    width?: number;
    height?: number;
    className?: string;
}

const ProfilePicture = ({
    width = 120,
    height = 120,
    className,
}: ProfilePictureProps) => {
    return (
        <Image
            data-testid="profilePicture"
            className={twMerge("aspect-square rounded-md", className)}
            alt="profile picture"
            src={`/assets/logo-image-temp.webp`}
            width={width}
            height={height}
        />
    );
};
export default ProfilePicture;
