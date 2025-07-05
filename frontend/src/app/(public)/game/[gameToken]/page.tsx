import LiveChessboard from "@/components/liveGame/LiveChessboard";
import withAuthedSession from "@/features/auth/hocs/withAuthedSession";
import { notFound, redirect } from "next/navigation";
import { getLiveGame } from "@/lib/apiClient";

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
            userId != game.whitePlayer.userId &&
            userId != game.blackPlayer.userId
        )
            redirect("/");

        return (
            <LiveChessboard
                gameToken={gameToken}
                gameState={game}
                userId={userId}
            />
        );
    },
);
export default GamePage;
