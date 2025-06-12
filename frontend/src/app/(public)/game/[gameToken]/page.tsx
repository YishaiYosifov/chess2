import LiveChessboard from "@/components/liveGame/LiveChessboard";
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
            <div className="grid">
                <LiveChessboard
                    gameToken={gameToken}
                    startingPieces={decodedFen}
                    legalMoves={decodedLegalMoves}
                    playingAs={playingAs.color}
                    sideToMove={game.currentPlayerColor}
                    breakpoints={[
                        {
                            maxScreenSize: 768,
                            paddingOffset: { width: 0, height: 76 },
                        },
                    ]}
                    defaultOffset={{ width: 256, height: 0 }}
                />
            </div>
        );
    },
);
export default GamePage;
