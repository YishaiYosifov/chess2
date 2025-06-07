import Chessboard from "@/components/game/Chessboard";
import withAuthedSession from "@/hocs/withAuthedSession";
import { getLiveGame } from "@/lib/apiClient";
import { parseFen } from "@/lib/utils/chessUtils";
import { notFound, redirect } from "next/navigation";

export const metadata = { title: "Live Game - Chess 2" };

const GamePage = withAuthedSession(
    async ({
        params,
        userId,
    }: {
        params: Promise<{ gameToken: string }>;
        userId: string;
    }) => {
        const { gameToken } = await params;

        const { error, data: game } = await getLiveGame({
            path: { gameToken },
        });
        if (error || !game) {
            console.error(error);
            notFound();
        }

        // if the user is not participating in the game, they shouldn't be here
        // TODO: allow spectating
        console.log(userId, game);
        if (
            userId != game.playerWhite.userId &&
            userId != game.playerBlack.userId
        )
            redirect("/");

        const playingAs =
            userId == game.playerWhite.userId
                ? game.playerWhite
                : game.playerBlack;
        console.log(playingAs);
        return (
            <Chessboard
                startingPieces={parseFen(game.fen)}
                legalMoves={game.legalMoves}
                playingAs={playingAs.color}
                playingSide={game.playerToMove}
            />
        );
    },
);
export default GamePage;
