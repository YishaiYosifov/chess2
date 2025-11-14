import WithSession from "@/features/auth/hocs/WithSession";
import ChallengeSidebar from "@/features/challenges/components/challengePageSidebar/ChallengeSidebar";
import StaticChessboardWithSidebar from "@/features/chessboard/components/StaticChessboardWithSidebar";
import { getChallenge } from "@/lib/apiClient";
import dataOrThrow from "@/lib/apiClient/dataOrThrow";

export const metadata = { title: "Challenge - Anarchy Chess" };

export default async function ChallengePage({
    params,
}: {
    params: Promise<{ challengeToken: string }>;
}) {
    const { challengeToken } = await params;

    return (
        <WithSession>
            {async ({ accessToken }) => {
                const challenge = await dataOrThrow(
                    getChallenge({
                        path: { challengeToken },
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
