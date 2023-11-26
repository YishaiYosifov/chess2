import Image from "next/image";

const ProfilePicture = ({
    username,
    width = 120,
    height = 120,
    lastChanged,
    className,
}: {
    username?: string;
    width?: number;
    height?: number;
    lastChanged?: string;
    className?: string;
}) => {
    return (
        <Image
            className={className}
            alt="profile picture"
            src={`${process.env.NEXT_PUBLIC_API_URL}/profile/${username}/profile-picture?${lastChanged}`}
            width={width}
            height={height}
        />
    );
};
export default ProfilePicture;
