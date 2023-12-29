import { apiConfig } from "@/lib/apis";
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
    lastChanged?: Date;
    className?: string;
}) => {
    const lastChangedTimestamp = (lastChanged ?? new Date()).valueOf() / 1000;

    return (
        <Image
            className={className}
            alt="profile picture"
            src={
                `${apiConfig.basePath}/profile/${username}` +
                `/profile-picture?${lastChangedTimestamp}`
            }
            width={width}
            height={height}
        />
    );
};
export default ProfilePicture;
