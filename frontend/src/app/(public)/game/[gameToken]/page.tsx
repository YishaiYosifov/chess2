import { notFound, redirect } from "next/navigation";

import LiveChessboard from "@/features/liveGame/components/LiveChessboard";
import WithSession from "@/features/auth/components/WithSession";
import { getGame } from "@/lib/apiClient";

export const metadata = { title: "Live Game - Chess 2" };

export default async function GamePage({
    params,
}: {
    params: Promise<{ gameToken: string }>;
}) {
    return (
        <WithSession>
            {async ({ user, accessToken }) => {
                const { gameToken } = await params;

                const { error, data: game } = await getGame({
                    path: { gameToken },
                    auth: () => accessToken,
                });

                if (error || game === undefined) {
                    console.warn(error);
                    notFound();
                }

                // if the user is not participating in the game, they shouldn't be here
                // TODO: allow spectating
                if (
                    user.userId != game.whitePlayer.userId &&
                    user.userId != game.blackPlayer.userId
                )
                    redirect("/");

                return (
                    <LiveChessboard gameToken={gameToken} gameState={game} />
                );
            }}
        </WithSession>
    );
}
