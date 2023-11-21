import Image from "next/image";

const ProfilePicture = ({
    userId,
    width = 120,
    height = 120,
    lastChanged,
    className,
}: {
    userId: number;
    width?: number;
    height?: number;
    lastChanged?: string;
    className?: string;
}) => {
    return (
        <Image
            className={className}
            alt="profile picture"
            src={`${process.env.NEXT_PUBLIC_API_URL}/api/profile/${userId}/profile-picture?${lastChanged}`}
            width={width}
            height={height}
        />
    );
};
export default ProfilePicture;
