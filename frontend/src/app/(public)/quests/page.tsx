import WithOptionalAuthedUser from "@/features/auth/components/WithOptionalAuthedUser";
import DailyQuestCard from "@/features/quests/components/DailyQuestCard";
import DailyQuestCardLoggedOut from "@/features/quests/components/DailyQuestCardLoggedOut";
import QuestLeaderboard from "@/features/quests/components/QuestLeaderboard";
import { getDailyQuest, getQuestLeaderboard } from "@/lib/apiClient";
import dataOrThrow from "@/lib/apiClient/dataOrThrow";
import constants from "@/lib/constants";

export const metadata = { title: "Quests - Chess 2" };

export default async function QuestsPage() {
    return (
        <WithOptionalAuthedUser>
            {async ({ accessToken }) => {
                const [leaderboard, dailyQuest] = await Promise.all([
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
                ]);

                for (let i = 0; i < 10; i++) {
                    leaderboard.items.push(leaderboard.items[0]);
                }

                return (
                    <div className="flex w-full flex-col items-center gap-6 p-5">
                        {dailyQuest ? (
                            <DailyQuestCard initialQuest={dailyQuest} />
                        ) : (
                            <DailyQuestCardLoggedOut />
                        )}

                        <QuestLeaderboard initialLeaderboard={leaderboard} />
                    </div>
                );
            }}
        </WithOptionalAuthedUser>
    );
}
