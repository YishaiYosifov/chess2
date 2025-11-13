import WinStreakObjective from "@/features/winStreak/components/WinStreakObjective";
import WinStreakProfileStats from "@/features/winStreak/components/WinStreakProfile";
import WinStreakHeader from "@/features/winStreak/components/WinStreakHeader";
import WinStreakRules from "@/features/winStreak/components/WinStreakRules";
import WithOptionalAuthedUser from "@/features/auth/hocs/WithOptionalAuthedUser";
import dataOrThrow from "@/lib/apiClient/dataOrThrow";
import { getMyWinStreakStats, getWinStreakLeaderboard } from "@/lib/apiClient";
import constants from "@/lib/constants";
import WinStreakLeaderboard from "@/features/winStreak/components/WinStreakLeaderboard";

export const metadata = { title: "Win Streak Challenge - Chess 2" };

export default async function WinStreakPage() {
    return (
        <WithOptionalAuthedUser>
            {async ({ accessToken }) => {
                const [leaderboard, myStats] = await Promise.all([
                    dataOrThrow(
                        getWinStreakLeaderboard({
                            query: {
                                Page: 0,
                                PageSize:
                                    constants.PAGINATION_PAGE_SIZE
                                        .WIN_STREAK_LEADERBOARD,
                            },
                        }),
                    ),
                    (async () => {
                        if (!accessToken) return;

                        return await dataOrThrow(
                            getMyWinStreakStats({
                                auth: () => accessToken,
                            }),
                        );
                    })(),
                ]);

                return (
                    <main className="mx-auto flex max-w-6xl flex-1 flex-col items-center gap-3 p-5">
                        <WinStreakHeader />

                        <div className="grid w-full grid-rows-2 gap-3 lg:grid-cols-2 lg:grid-rows-1">
                            <WinStreakObjective />
                            <WinStreakRules />
                        </div>

                        {myStats && (
                            <WinStreakProfileStats
                                stats={myStats}
                                totalPlayers={leaderboard.totalCount}
                            />
                        )}

                        <WinStreakLeaderboard
                            initialLeaderboard={leaderboard}
                        />
                    </main>
                );
            }}
        </WithOptionalAuthedUser>
    );
}
