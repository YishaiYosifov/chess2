import ProfilePicture from "../profile/ProfilePicture";
import Flag from "../profile/Flag";
import { GamePlayer } from "@/lib/apiClient";

const LiveChessboardProfile = ({ player }: { player: GamePlayer }) => {
    return (
        <div className="flex items-center justify-between">
            <div className="flex items-center gap-3">
                <ProfilePicture height={50} width={50} />
                <Flag countryCode={player.countryCode} size={30} />

                <span className="font-medium text-white">
                    {player.userName}
                </span>

                {player.rating && (
                    <span className="w-fit rounded bg-white/10 px-2 py-0.5 text-xs text-white/80">
                        {player.rating}
                    </span>
                )}
            </div>
            <span className="text-2xl">1:23:45</span>
        </div>
    );
};
export default LiveChessboardProfile;
