import WithSession from "@/features/auth/components/WithSession";
import ChallengeSidebar from "@/features/challenges/components/ChallengeSidebar";
import StaticChessboardWithSidebar from "@/features/chessboard/components/StaticChessboardWithSidebar";
import { getChallenge } from "@/lib/apiClient";
import dataOrThrow from "@/lib/apiClient/dataOrThrow";

export default async function ChallengePage({
    params,
}: {
    params: Promise<{ challengeId: string }>;
}) {
    const { challengeId } = await params;

    return (
        <WithSession>
            {async ({ accessToken }) => {
                const challenge = await dataOrThrow(
                    getChallenge({
                        path: { challengeId },
                        auth: () => accessToken,
                    }),
                );

                return (
                    <StaticChessboardWithSidebar
                        aside={<ChallengeSidebar challenge={challenge} />}
                    />
                );
            }}
        </WithSession>
    );
}
