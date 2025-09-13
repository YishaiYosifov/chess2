import Button from "@/components/ui/Button";
import Card from "@/components/ui/Card";
import { Quest, QuestDifficulty } from "@/lib/apiClient";

const DailyQuestCard = ({ quest }: { quest: Quest }) => {
    const percentDone = (quest.progress / quest.target) * 100;
    const difficultyText =
        QuestDifficulty[quest.difficulty].charAt(0).toUpperCase() +
        QuestDifficulty[quest.difficulty].slice(1).toLowerCase();

    const difficultyColor = {
        [QuestDifficulty.EASY]: "text-green-400",
        [QuestDifficulty.MEDIUM]: "text-yellow-400",
        [QuestDifficulty.HARD]: "text-red-400",
    };

    return (
        <Card className="h-fit w-full max-w-3xl gap-6 p-6">
            <div className="flex flex-col justify-between sm:flex-row">
                <h1 className="text-4xl">Daily Quest</h1>
                <p className="text-text/70 font-medium">
                    {quest.streak} Day Streak
                </p>
            </div>

            <div className="flex flex-col gap-2">
                <p className="text-lg">
                    <span className={difficultyColor[quest.difficulty]}>
                        {difficultyText}:
                    </span>{" "}
                    {quest.description}
                </p>

                <div className="flex items-center gap-3">
                    <div className="bg-primary h-4 flex-1 overflow-hidden rounded-full">
                        <div
                            className="bg-secondary h-4 rounded-full"
                            style={{ width: `${percentDone}%` }}
                        />
                    </div>

                    <p className="text-text/70 min-w-[40px] text-center text-sm font-medium">
                        {quest.progress}/{quest.target}
                    </p>

                    <Button>Replace</Button>
                </div>
            </div>
        </Card>
    );
};
export default DailyQuestCard;
