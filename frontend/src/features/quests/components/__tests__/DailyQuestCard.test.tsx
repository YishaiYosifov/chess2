import { render, screen } from "@testing-library/react";

import { createFakeQuest } from "@/lib/testUtils/fakers/questFaker";
import DailyQuestCard from "../DailyQuestCard";
import { QuestDifficulty, replaceDailyQuest } from "@/lib/apiClient";
import userEvent from "@testing-library/user-event";
import { mockRouter } from "@/lib/testUtils/mocks/mockRouter";

vi.mock("@/lib/apiClient/definition");

describe("DailyQuestCard", () => {
    const replaceDailyQuestMock = vi.mocked(replaceDailyQuest);

    it("should render the title and streak", () => {
        const quest = createFakeQuest({ streak: 0 });
        render(<DailyQuestCard initialQuest={quest} />);

        expect(screen.getByTestId("dailyQuestTitle")).toHaveTextContent(
            "Daily Quest",
        );
        expect(screen.getByTestId("dailyQuestStreak")).toHaveTextContent(
            "0 Day Streak",
        );
    });

    it("should render the fire emoji when streak > 0", () => {
        const quest = createFakeQuest({ streak: 7 });
        render(<DailyQuestCard initialQuest={quest} />);

        expect(screen.getByTestId("dailyQuestStreak")).toHaveTextContent(
            "🔥 7 Day Streak",
        );
    });

    it("should render the description with difficulty text", () => {
        const quest = createFakeQuest({
            difficulty: QuestDifficulty.HARD,
        });
        render(<DailyQuestCard initialQuest={quest} />);

        expect(screen.getByTestId("dailyQuestDifficulty")).toHaveTextContent(
            "Hard:",
        );
        expect(screen.getByTestId("dailyQuestDescription")).toHaveTextContent(
            quest.description,
        );
    });

    it("should render progress bar with correct width", () => {
        const quest = createFakeQuest({ progress: 3, target: 6 });
        render(<DailyQuestCard initialQuest={quest} />);

        expect(screen.getByTestId("dailyQuestProgressFill")).toHaveStyle({
            width: "50%",
        });
    });

    it("should render progress text correctly", () => {
        const quest = createFakeQuest({ progress: 2, target: 5 });
        render(<DailyQuestCard initialQuest={quest} />);

        expect(screen.getByTestId("dailyQuestProgressText")).toHaveTextContent(
            "2/5",
        );
    });

    it("should render replace button", () => {
        const quest = createFakeQuest();
        render(<DailyQuestCard initialQuest={quest} />);

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
        render(<DailyQuestCard initialQuest={quest} />);

        expect(screen.getByTestId("dailyQuestDifficulty")).toHaveClass(style);
    });

    it("should call replaceDailyQuest and update quest on success", async () => {
        const initialQuest = createFakeQuest({ canReplace: true });
        const newQuest = createFakeQuest({ description: "New quest" });

        const routerMock = mockRouter();
        replaceDailyQuestMock.mockResolvedValue({
            data: newQuest,
            response: new Response(),
        });

        const user = userEvent.setup();
        render(<DailyQuestCard initialQuest={initialQuest} />);
        const replaceButton = screen.getByTestId("dailyQuestReplaceButton");

        await user.click(replaceButton);

        expect(replaceButton).toBeDisabled();

        expect(screen.getByTestId("dailyQuestDescription")).toHaveTextContent(
            newQuest.description,
        );
        expect(routerMock.refresh).toHaveBeenCalled();

        expect(replaceButton).not.toBeInTheDocument();
    });

    it("should display error message if replaceDailyQuest fails", async () => {
        const initialQuest = createFakeQuest({ canReplace: true });

        replaceDailyQuestMock.mockResolvedValue({
            data: undefined,
            error: { errors: [], extensions: {} },
            response: new Response(),
        });

        const user = userEvent.setup();
        render(<DailyQuestCard initialQuest={initialQuest} />);
        const replaceButton = screen.getByTestId("dailyQuestReplaceButton");

        await user.click(replaceButton);

        expect(screen.getByText("Faled to replace quest")).toBeInTheDocument();
        expect(replaceButton).not.toBeDisabled();
    });

    it("should not render replace button if quest cannot be replaced", () => {
        const quest = createFakeQuest({ canReplace: false });
        render(<DailyQuestCard initialQuest={quest} />);

        expect(
            screen.queryByTestId("dailyQuestReplaceButton"),
        ).not.toBeInTheDocument();
    });
});
