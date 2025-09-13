"use client";

import { useState } from "react";

import Button from "@/components/ui/Button";
import Card from "@/components/ui/Card";
import { Quest, QuestDifficulty, replaceDailyQuest } from "@/lib/apiClient";
import { useRouter } from "next/navigation";

const DailyQuestCard = ({ initialQuest }: { initialQuest: Quest }) => {
    const [quest, setQuest] = useState(initialQuest);
    const [error, setError] = useState("");
    const [isReplacing, setIsReplacing] = useState(false);
    const router = useRouter();

    const percentDone = (quest.progress / quest.target) * 100;
    const difficultyText =
        QuestDifficulty[quest.difficulty].charAt(0).toUpperCase() +
        QuestDifficulty[quest.difficulty].slice(1).toLowerCase();
    const isCompleted = quest.progress >= quest.target;

    const difficultyColor = {
        [QuestDifficulty.EASY]: "text-green-400",
        [QuestDifficulty.MEDIUM]: "text-yellow-400",
        [QuestDifficulty.HARD]: "text-red-400",
    };

    async function replaceQuest() {
        setIsReplacing(true);

        try {
            const { error, data: newQuest } = await replaceDailyQuest();
            if (error || !newQuest) {
                setError("Faled to replace quest");
                console.error(error);
                return;
            }

            setQuest(newQuest);
            router.refresh();
        } finally {
            setIsReplacing(false);
        }
    }

    return (
        <Card className="h-fit w-full max-w-3xl gap-6 p-6">
            <div className="flex flex-col justify-between sm:flex-row">
                <h1 className="text-4xl" data-testid="dailyQuestTitle">
                    Daily Quest
                </h1>
                <p
                    className="text-text/70 font-medium"
                    data-testid="dailyQuestStreak"
                >
                    {quest.streak > 0 && "🔥 "}
                    {quest.streak} Day Streak
                </p>
            </div>

            <div className="flex flex-col gap-2">
                <p className="text-lg" data-testid="dailyQuestDescription">
                    <span
                        className={difficultyColor[quest.difficulty]}
                        data-testid="dailyQuestDifficulty"
                    >
                        {difficultyText}:
                    </span>{" "}
                    {quest.description}
                </p>

                <div className="flex items-center gap-3">
                    <div
                        className="bg-primary h-4 flex-1 overflow-hidden rounded-full"
                        data-testid="dailyQuestProgressBar"
                    >
                        <div
                            className="bg-secondary h-4 rounded-full"
                            style={{ width: `${percentDone}%` }}
                            data-testid="dailyQuestProgressFill"
                        />
                    </div>

                    <p
                        className="text-text/70 min-w-[40px] text-center text-sm font-medium"
                        data-testid="dailyQuestProgressText"
                    >
                        {quest.progress}/{quest.target}
                    </p>

                    {!isCompleted && quest.canReplace && (
                        <Button
                            data-testid="dailyQuestReplaceButton"
                            onClick={replaceQuest}
                            disabled={isReplacing}
                        >
                            Replace
                        </Button>
                    )}
                </div>
                {error && <span className="text-error">{error}</span>}
            </div>
        </Card>
    );
};
export default DailyQuestCard;
