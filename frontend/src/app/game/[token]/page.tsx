import { ResponseError } from "@/client";
import Chessboard from "@/components/game/Chessboard";
import { liveGameApi } from "@/lib/apis";
import { parseFen } from "@/lib/utils/chessUtils";
import { notFound } from "next/navigation";

export const metadata = { title: "Live Game - Chess 2" };

const GamePage = async ({
    params: { token },
}: {
    params: { token: string };
}) => {
    let game;
    try {
        game = await liveGameApi.getLiveGame({ token });
    } catch (err) {
        if (!(err instanceof ResponseError)) throw err;

        if (err.response.status == 404) notFound();
        throw err;
    }

    return <Chessboard startingPieces={parseFen(game.fen)} />;
};
export default GamePage;
