"use client";

import RankDisplay from "@/components/RankDisplay";
import Card from "@/components/ui/Card";
import { useAuthedUser } from "@/features/auth/hooks/useSessionUser";
import MinimalProfileView from "@/features/profile/components/MinimalProfileView";
import { MyWinStreakStats } from "@/lib/apiClient";

const WinStreakProfileStats = ({
    stats,
    totalPlayers,
}: {
    stats: MyWinStreakStats;
    totalPlayers: number;
}) => {
    const user = useAuthedUser();
    if (!user) return null;

    return (
        <Card className="w-full flex-row">
            <MinimalProfileView profile={user}>
                <div
                    className="ml-auto flex flex-col justify-between text-right"
                    data-testid="winStreakProfileStatsStreaks"
                >
                    <p>Current Streak: {stats.currentStreak}</p>
                    <p>Highest Streak: {stats.highestStreak}</p>
                </div>
            </MinimalProfileView>

            <RankDisplay rank={stats.rank} totalPlayers={totalPlayers} />
        </Card>
    );
};
export default WinStreakProfileStats;
