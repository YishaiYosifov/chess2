import { redirect } from "next/navigation";

import LiveChessboard from "@/features/liveGame/components/LiveChessboard";
import WithSession from "@/features/auth/components/WithSession";
import { getGame, getPreferences } from "@/lib/apiClient";
import dataOrThrow from "@/lib/apiClient/dataOrThrow";

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

                const [game, preferences] = await Promise.all([
                    dataOrThrow(
                        getGame({
                            path: { gameToken },
                            auth: () => accessToken,
                        }),
                    ),
                    dataOrThrow(getPreferences({ auth: () => accessToken })),
                ]);

                // if the user is not participating in the game, they shouldn't be here
                // TODO: allow spectating
                if (
                    user.userId != game.whitePlayer.userId &&
                    user.userId != game.blackPlayer.userId
                )
                    redirect("/");

                return (
                    <LiveChessboard
                        gameToken={gameToken}
                        gameState={game}
                        preferences={preferences}
                    />
                );
            }}
        </WithSession>
    );
}
