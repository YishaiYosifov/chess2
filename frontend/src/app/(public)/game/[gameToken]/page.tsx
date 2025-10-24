import LiveChessboard from "@/features/liveGame/components/LiveChessboard";
import WithSession from "@/features/auth/hocs/WithSession";
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
            {async ({ accessToken }) => {
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
