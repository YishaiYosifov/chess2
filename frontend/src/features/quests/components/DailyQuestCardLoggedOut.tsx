import Button from "@/components/ui/Button";
import Card from "@/components/ui/Card";
import constants from "@/lib/constants";
import Link from "next/link";

const DailyQuestCardLoggedOut = () => {
    return (
        <Card className="h-fit w-full gap-6 p-6">
            <h1 className="text-4xl" data-testid="dailyQuestTitle">
                Daily Quest
            </h1>

            <div className="flex flex-col gap-4">
                <p
                    className="text-text/70 text-lg"
                    data-testid="dailyQuestMessage"
                >
                    Register to start completing daily quests, earn streaks, and
                    track your progress!
                </p>

                <Button>
                    <Link
                        href={constants.PATHS.REGISTER}
                        className="flex h-full w-full items-center justify-center"
                        data-testid="dailyQuestRegisterLink"
                    >
                        Register
                    </Link>
                </Button>
            </div>
        </Card>
    );
};
export default DailyQuestCardLoggedOut;
