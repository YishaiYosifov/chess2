import {
    getDailyQuest,
    getMyQuestRanking,
    getQuestLeaderboard,
    getUserQuestPoints,
} from "@/lib/apiClient";

import WithOptionalAuthedUser from "@/features/auth/components/WithOptionalAuthedUser";
import DailyQuestCard from "@/features/quests/components/DailyQuestCard";
import DailyQuestCardLoggedOut from "@/features/quests/components/DailyQuestCardLoggedOut";
import DailyQuestRankCard from "@/features/quests/components/DailyQuestRankCard";
import QuestLeaderboard from "@/features/quests/components/QuestLeaderboard";
import dataOrThrow from "@/lib/apiClient/dataOrThrow";
import constants from "@/lib/constants";

export const metadata = { title: "Quests - Chess 2" };

export default async function QuestsPage() {
    return (
        <WithOptionalAuthedUser>
            {async ({ accessToken, user }) => {
                const [
                    leaderboard,
                    dailyQuest,
                    userCurrentRank,
                    userQuestPoints,
                ] = await Promise.all([
                    dataOrThrow(
                        getQuestLeaderboard({
                            query: {
                                Page: 0,
                                PageSize:
                                    constants.PAGINATION_PAGE_SIZE
                                        .QUEST_LEADERBOARD,
                            },
                        }),
                    ),
                    (async () => {
                        if (!accessToken) return;

                        return await dataOrThrow(
                            getDailyQuest({
                                auth: () => accessToken,
                            }),
                        );
                    })(),
                    (async () => {
                        if (!accessToken) return;

                        return await dataOrThrow(
                            getMyQuestRanking({ auth: () => accessToken }),
                        );
                    })(),
                    (async () => {
                        if (!user) return;

                        return await dataOrThrow(
                            getUserQuestPoints({
                                path: { userId: user.userId },
                            }),
                        );
                    })(),
                ]);

                return (
                    <div className="mx-auto flex w-full max-w-6xl flex-col items-center gap-6 p-5">
                        {dailyQuest ? (
                            <DailyQuestCard initialQuest={dailyQuest} />
                        ) : (
                            <DailyQuestCardLoggedOut />
                        )}

                        <DailyQuestRankCard
                            questPoints={userQuestPoints}
                            currentRank={userCurrentRank}
                            totalPlayers={leaderboard.totalCount}
                        />

                        <QuestLeaderboard initialLeaderboard={leaderboard} />
                    </div>
                );
            }}
        </WithOptionalAuthedUser>
    );
}
