"use client";

import RankDisplay from "@/components/RankDisplay";
import Card from "@/components/ui/Card";
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

    return (
        <Card className="w-full items-center justify-between sm:flex-row">
            <MinimalProfileView profile={user}>
                <p className="ml-auto" data-testid="dailyQuestRankPoints">
                    {questPoints} points
                </p>
            </MinimalProfileView>

            <RankDisplay rank={currentRank} totalPlayers={totalPlayers} />
        </Card>
    );
};
export default DailyQuestRankCard;
