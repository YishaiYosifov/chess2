import ProfilePicture from "../profile/ProfilePicture";
import Flag from "../profile/Flag";

const LiveChessboardProfile = () => {
    return (
        <div className="flex items-center justify-between">
            <div className="flex items-center gap-3">
                <ProfilePicture height={50} width={50} />
                <Flag size={30} />
                <span>user 1</span>
            </div>
            <span className="text-2xl">1:23:45</span>
        </div>
    );
};
export default LiveChessboardProfile;
