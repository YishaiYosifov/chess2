import Chessboard from "@/components/game/Chessboard";
import withAuthedSession from "@/hocs/withAuthedSession";
import { getLiveGame } from "@/lib/apiClient";
import { decodeLegalMoves, parseFen } from "@/lib/utils/chessUtils";
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
        console.log(game);

        // if the user is not participating in the game, they shouldn't be here
        // TODO: allow spectating
        if (
            userId != game.playerWhite.userId &&
            userId != game.playerBlack.userId
        )
            redirect("/");

        const decodedLegalMoves = decodeLegalMoves(game.legalMoves);
        const playingAs =
            userId == game.playerWhite.userId
                ? game.playerWhite
                : game.playerBlack;
        return (
            <Chessboard
                startingPieces={parseFen(game.fen)}
                legalMoves={decodedLegalMoves}
                playingAs={playingAs.color}
                sideToMove={game.playerToMove}
            />
        );
    },
);
export default GamePage;
