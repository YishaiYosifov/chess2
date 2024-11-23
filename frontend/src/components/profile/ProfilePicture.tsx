import clsx from "clsx";
import Image from "next/image";

export interface ProfilePictureProps {
    username?: string;
    width?: number;
    height?: number;
    lastChanged?: number;
    className?: string;
}

const ProfilePicture = ({
    username,
    width = 120,
    height = 120,
    lastChanged,
    className,
}: ProfilePictureProps) => {
    return (
        <Image
            data-testid="profilePicture"
            className={clsx("rounded-md", className)}
            alt="profile picture"
            src={`/assets/logo.svg?${lastChanged}`}
            width={width}
            height={height}
        />
    );
};
export default ProfilePicture;
