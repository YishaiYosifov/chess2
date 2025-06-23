import ProfilePicture from "../profile/ProfilePicture";
import Flag from "../profile/Flag";
import { GamePlayer } from "@/lib/apiClient";

const LiveChessboardProfile = ({ player }: { player: GamePlayer }) => {
    return (
        <div className="flex items-center justify-between">
            <div className="flex items-center gap-3">
                <ProfilePicture height={50} width={50} />
                <Flag size={30} />
                <span>{player.userName}</span>
            </div>
            <span className="text-2xl">1:23:45</span>
        </div>
    );
};
export default LiveChessboardProfile;
