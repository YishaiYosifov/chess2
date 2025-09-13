import { render, screen } from "@testing-library/react";

import { createFakeQuest } from "@/lib/testUtils/fakers/questFaker";
import DailyQuestCard from "../DailyQuestCard";
import { QuestDifficulty } from "@/lib/apiClient";

describe("DailyQuestCard", () => {
    it("should render the title and streak", () => {
        const quest = createFakeQuest({ streak: 7 });
        render(<DailyQuestCard quest={quest} />);

        expect(screen.getByTestId("dailyQuestTitle")).toHaveTextContent(
            "Daily Quest",
        );
        expect(screen.getByTestId("dailyQuestStreak")).toHaveTextContent(
            "7 Day Streak",
        );
    });

    it("should render the description with difficulty text", () => {
        const quest = createFakeQuest({
            difficulty: QuestDifficulty.HARD,
        });
        render(<DailyQuestCard quest={quest} />);

        expect(screen.getByTestId("dailyQuestDifficulty")).toHaveTextContent(
            "Hard:",
        );
        expect(screen.getByTestId("dailyQuestDescription")).toHaveTextContent(
            quest.description,
        );
    });

    it("should render progress bar with correct width", () => {
        const quest = createFakeQuest({ progress: 3, target: 6 });
        render(<DailyQuestCard quest={quest} />);

        expect(screen.getByTestId("dailyQuestProgressFill")).toHaveStyle({
            width: "50%",
        });
    });

    it("should render progress text correctly", () => {
        const quest = createFakeQuest({ progress: 2, target: 5 });
        render(<DailyQuestCard quest={quest} />);

        expect(screen.getByTestId("dailyQuestProgressText")).toHaveTextContent(
            "2/5",
        );
    });

    it("should render replace button", () => {
        const quest = createFakeQuest();
        render(<DailyQuestCard quest={quest} />);

        expect(screen.getByTestId("dailyQuestReplaceButton")).toHaveTextContent(
            "Replace",
        );
    });

    it.each([
        [QuestDifficulty.EASY, "text-green-400"],
        [QuestDifficulty.MEDIUM, "text-yellow-400"],
        [QuestDifficulty.HARD, "text-red-400"],
    ])("should apply correct difficulty color class", (difficulty, style) => {
        const quest = createFakeQuest({ difficulty });
        render(<DailyQuestCard quest={quest} />);

        expect(screen.getByTestId("dailyQuestDifficulty")).toHaveClass(style);
    });
});
