import Chessboard from "@/components/game/Chessboard";
import withAuthedSession from "@/hocs/withAuthedSession";
import { getLiveGame } from "@/lib/apiClient";
import { decodeFen } from "@/lib/chessDecoders/fenDecoder";
import { decodeLegalMoves } from "@/lib/chessDecoders/moveDecoder";
import { notFound, redirect } from "next/navigation";

export const metadata = { title: "Live Game - Chess 2" };

const GamePage = withAuthedSession(
    async ({
        params,
        userId,
        accessToken,
    }: {
        params: Promise<{ gameToken: string }>;
        userId: string;
        accessToken?: string;
    }) => {
        const { gameToken } = await params;

        const { error, data: game } = await getLiveGame({
            path: { gameToken },
            auth: () => accessToken,
        });

        if (error || !game) {
            console.warn(error);
            notFound();
        }

        // if the user is not participating in the game, they shouldn't be here
        // TODO: allow spectating
        if (
            userId != game.playerWhite.userId &&
            userId != game.playerBlack.userId
        )
            redirect("/");

        const decodedLegalMoves = decodeLegalMoves(game.legalMoves);
        const decodedFen = decodeFen(game.fen);

        const playingAs =
            userId == game.playerWhite.userId
                ? game.playerWhite
                : game.playerBlack;
        return (
            <Chessboard
                startingPieces={decodedFen}
                legalMoves={decodedLegalMoves}
                playingAs={playingAs.color}
                sideToMove={game.playerToMove}
            />
        );
    },
);
export default GamePage;
