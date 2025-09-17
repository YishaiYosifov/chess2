"use client";

import Card from "@/components/ui/Card";
import ProgressBar from "@/components/ui/ProgressBar";
import { useAuthedUser } from "@/features/auth/hooks/useSessionUser";
import MinimalProfileView from "@/features/profile/components/MinimalProfileView";

const DailyQuestRankCard = ({
    questPoints,
    currentRank,
    totalPlayers,
}: {
    questPoints?: number;
    currentRank?: number;
    totalPlayers: number;
}) => {
    const user = useAuthedUser();
    if (user === null || !currentRank) return null;

    const percentile = ((totalPlayers - currentRank) / totalPlayers) * 100;
    return (
        <Card className="flex w-full items-center justify-between p-4 sm:flex-row">
            <MinimalProfileView profile={user}>
                <p className="ml-auto" data-testid="dailyQuestRankPoints">
                    {questPoints} points
                </p>
            </MinimalProfileView>

            <div className="w-full sm:w-auto">
                <h2 className="text-xl font-bold">Your Rank</h2>
                <div className="flex items-center gap-3">
                    <p
                        className="text-2xl font-extrabold text-amber-400"
                        data-testid="dailyQuestRankNumber"
                    >
                        #{currentRank}
                    </p>
                    <ProgressBar percent={percentile} />
                </div>

                <p
                    className="text-text/70 text-sm"
                    data-testid="dailyQuestRankPercentile"
                >
                    You&apos;re in the top {percentile.toFixed(1)}%! Keep going!
                </p>
            </div>
        </Card>
    );
};
export default DailyQuestRankCard;
