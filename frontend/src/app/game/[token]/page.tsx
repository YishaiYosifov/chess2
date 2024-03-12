import Chessboard from "@/components/game/Chessboard";
import { liveGameApi } from "@/lib/apis";

export const metadata = { title: "Live Game - Chess 2" };

const GamePage = async ({
    params: { token },
}: {
    params: { token: string };
}) => {
    console.log(await liveGameApi.getLiveGame({ token }));
    return <Chessboard />;
};
export default GamePage;
