import Card from "@/components/ui/Card";

const WinStreakObjective = () => {
    return (
        <Card className="from-card to-background bg-card gap-10 p-5 sm:p-10 lg:bg-gradient-to-b">
            <div className="flex flex-col gap-10 text-center">
                <h1 className="text-secondary text-5xl italic">
                    THE OBJECTIVE:
                </h1>

                <h2 className="text-3xl text-balance">
                    Get the longest win streak you possibly can.
                </h2>
                <p className="text-xl text-balance">
                    Players with the highest win streak will rank higher on the
                    leaderboard!
                </p>
            </div>

            <ObjectiveCard
                title="Prizes"
                text="$500 total prize pool split between the top three players: 1st $300, 2nd $125, 3rd $75."
            />
            <ObjectiveCard
                title="How to Play"
                text="Play rated matches until the leaderboard locks, each consecutive
                    win adds to your streak. Lose once, and your streak resets!"
            />
            <ObjectiveCard
                title="Leaderboards"
                text="Your highest streak will be verified and publicly displayed
                    on the leaderboard"
            />
        </Card>
    );
};
export default WinStreakObjective;

const ObjectiveCard = ({ title, text }: { title: string; text: string }) => {
    return (
        <div className="border-secondary bg-background flex flex-col gap-3 rounded-3xl border p-5">
            <h1 className="text-2xl text-blue-200">{title}</h1>
            <p className="text-xl">{text}</p>
        </div>
    );
};
