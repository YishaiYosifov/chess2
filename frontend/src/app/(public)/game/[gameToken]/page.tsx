import withAuthedSession from "@/features/auth/hocs/withAuthedSession";
import { notFound, redirect } from "next/navigation";
import { getGame } from "@/lib/apiClient";
import GameStatePreprocessor from "@/features/liveGame/components/GameStatePreprocessor";

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

        const { error, data: game } = await getGame({
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
            <GameStatePreprocessor
                gameToken={gameToken}
                gameState={game}
                userId={userId}
            />
        );
    },
);
export default GamePage;
