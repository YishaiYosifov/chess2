import Button from "@/components/ui/Button";
import Card from "@/components/ui/Card";

export default async function QuestsPage() {
    return (
        <div className="flex w-full flex-col items-center gap-6 p-5">
            <Card className="h-fit w-full max-w-3xl gap-6 p-6">
                <div className="flex flex-col justify-between sm:flex-row">
                    <h1 className="text-4xl">Daily Quest</h1>
                    <p className="text-text/70 font-medium">0 Day Streak</p>
                </div>

                <div className="flex flex-col gap-2">
                    <p className="text-lg">
                        <span className="text-yellow-400">Medium:</span> Win a
                        game without a piece capture in the first 10 moves
                    </p>

                    <div className="flex items-center gap-3">
                        <div className="bg-primary h-4 flex-1 overflow-hidden rounded-full">
                            <div
                                className="bg-secondary h-4 rounded-full"
                                style={{ width: "50%" }}
                            />
                        </div>

                        <p className="text-text/70 min-w-[40px] text-center text-sm font-medium">
                            0.5/1
                        </p>

                        <Button>Replace</Button>
                    </div>
                </div>
            </Card>

            <Card className="w-full max-w-3xl">
                <h2 className="text-3xl">Leaderboard</h2>
            </Card>
        </div>
    );
}
