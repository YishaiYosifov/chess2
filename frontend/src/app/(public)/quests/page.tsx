import Card from "@/components/ui/Card";
import WithOptionalAuthedUser from "@/features/auth/components/WithOptionalAuthedUser";
import DailyQuestCard from "@/features/quests/components/DailyQuestCard";
import DailyQuestCardLoggedOut from "@/features/quests/components/DailyQuestCardLoggedOut";
import { getDailyQuest, Quest } from "@/lib/apiClient";
import dataOrThrow from "@/lib/apiClient/dataOrThrow";

export default async function QuestsPage() {
    return (
        <WithOptionalAuthedUser>
            {async ({ accessToken }) => {
                let dailyQuest: Quest | null = null;
                if (accessToken) {
                    dailyQuest = await dataOrThrow(
                        getDailyQuest({
                            auth: () => accessToken,
                        }),
                    );
                }

                return (
                    <div className="flex w-full flex-col items-center gap-6 p-5">
                        {dailyQuest ? (
                            <DailyQuestCard initialQuest={dailyQuest} />
                        ) : (
                            <DailyQuestCardLoggedOut />
                        )}

                        <Card className="w-full max-w-3xl">
                            <h2 className="text-3xl">Leaderboard</h2>
                        </Card>
                    </div>
                );
            }}
        </WithOptionalAuthedUser>
    );
}
