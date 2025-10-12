import WithSession from "@/features/auth/hocs/WithSession";
import ChallengeSidebar from "@/features/challenges/components/challengePageSidebar/ChallengeSidebar";
import StaticChessboardWithSidebar from "@/features/chessboard/components/StaticChessboardWithSidebar";
import { getChallenge } from "@/lib/apiClient";
import dataOrThrow from "@/lib/apiClient/dataOrThrow";

export const metadata = { title: "Challenge - Chess 2" };

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
