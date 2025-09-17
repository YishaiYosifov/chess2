"use client";

import { useRouter } from "next/navigation";
import { useState } from "react";

import {
    collectQuestReward,
    Quest,
    QuestDifficulty,
    replaceDailyQuest,
} from "@/lib/apiClient";

import NewQuestCountdown from "./NewQuestCountdown";
import Button from "@/components/ui/Button";
import Card from "@/components/ui/Card";
import constants from "@/lib/constants";

const DailyQuestCard = ({ initialQuest }: { initialQuest: Quest }) => {
    const [quest, setQuest] = useState(initialQuest);
    const [error, setError] = useState("");
    const [isFetching, setIsFetching] = useState(false);
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
        setIsFetching(true);

        try {
            const { error, data: newQuest } = await replaceDailyQuest();
            if (error || !newQuest) {
                setError("Failed to replace quest");
                console.error(error);
                return;
            }

            setQuest(newQuest);
            router.refresh();
        } finally {
            setIsFetching(false);
        }
    }

    async function collectReward() {
        setIsFetching(true);
        try {
            const { error } = await collectQuestReward();
            if (error) {
                setError("Failed to collect reward");
                console.error(error);
                return;
            }

            setQuest({ ...quest, rewardCollected: true });
            router.refresh();
        } finally {
            setIsFetching(false);
        }
    }

    return (
        <Card className="h-fit w-full gap-6 p-6">
            {/* header */}
            <h1
                className="text-center text-4xl text-balance sm:text-start"
                data-testid="dailyQuestTitle"
            >
                Daily Quest:{" "}
                {constants.QUEST_WEEKDAY_NAMES[new Date().getDay()]}
            </h1>

            {/* description */}
            <div className="flex flex-col">
                <p
                    className="text-center text-lg text-balance sm:text-start"
                    data-testid="dailyQuestDescription"
                >
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
                            className="py-1"
                            disabled={isFetching}
                        >
                            Replace
                        </Button>
                    )}
                    {isCompleted && !quest.rewardCollected && (
                        <Button
                            data-testid="dailyQuestCollectButton"
                            onClick={collectReward}
                            className="py-1"
                            disabled={isFetching}
                        >
                            Collect Reward
                        </Button>
                    )}
                    {isCompleted && quest.rewardCollected && (
                        <p
                            data-testid="dailyQuestCollectedRewardText"
                            className={difficultyColor[quest.difficulty]}
                        >
                            +{quest.difficulty} points
                        </p>
                    )}
                </div>

                {error && (
                    <p className="text-error" data-testid="dailyQueryError">
                        {error}
                    </p>
                )}
            </div>

            {/* footer */}
            <div className="text-text/70 flex flex-wrap justify-center gap-x-3 sm:justify-between">
                <NewQuestCountdown />

                <span data-testid="dailyQuestStreak">
                    {quest.streak > 0 && "🔥"}
                    {quest.streak} Day{quest.streak == 1 ? "" : "s"} Streak
                </span>
            </div>
        </Card>
    );
};
export default DailyQuestCard;
