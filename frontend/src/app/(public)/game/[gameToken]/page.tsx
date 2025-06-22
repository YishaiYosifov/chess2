import ChessboardSide from "@/components/liveGame/ChessboardSide";
import LiveChessboard from "@/components/liveGame/LiveChessboard";
import withAuthedSession from "@/hocs/withAuthedSession";
import { getLiveGame } from "@/lib/apiClient";
import { decodeFen } from "@/lib/chessDecoders/fenDecoder";
import {
    decodeMoves,
    decodeMovesIntoMap,
} from "@/lib/chessDecoders/moveDecoder";
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

        const decodedLegalMoves = decodeMovesIntoMap(game.legalMoves);
        const decodedMoveHistory = decodeMoves(game.moveHistory);
        const decodedFen = decodeFen(game.fen);

        const playingAs =
            userId == game.playerWhite.userId
                ? game.playerWhite
                : game.playerBlack;
        return (
            <div
                className="jutsify-center flex w-full flex-col items-center justify-center gap-5 p-5
                    lg:h-screen lg:flex-row"
            >
                <div className="flex md:max-h-screen">
                    <LiveChessboard
                        gameToken={gameToken}
                        initialMoveHistory={decodedMoveHistory}
                        startingPieces={decodedFen}
                        legalMoves={decodedLegalMoves}
                        playingAs={playingAs.color}
                        sideToMove={game.sideToMove}
                        breakpoints={[
                            {
                                maxScreenSize: 768,
                                paddingOffset: { width: 40, height: 110 },
                            },
                            {
                                maxScreenSize: 1024,
                                paddingOffset: { width: 200, height: 50 },
                            },
                        ]}
                        defaultOffset={{ width: 626, height: 100 }}
                        className="m-auto"
                    />
                </div>

                <ChessboardSide />
            </div>
        );
    },
);
export default GamePage;
