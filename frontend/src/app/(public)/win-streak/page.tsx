import Card from "@/components/ui/Card";
import Image from "next/image";

import thumbnail from "@public/assets/win-streak/thumbnail.png";

export default async function WinStreakPage() {
    return (
        <main className="mx-auto flex max-w-6xl flex-1 flex-col items-center gap-3 p-5">
            <Card className="w-full gap-5 lg:flex-row">
                <Image
                    src={thumbnail}
                    alt="challenge thumbnail"
                    width={400}
                    className="border-secondary mx-auto my-auto h-min rounded-md border-5 object-contain
                        lg:border-l-0"
                />

                <div className="flex flex-col justify-between gap-5">
                    <h1 className="text-3xl">
                        The Anarchy Win-Streak Challenge
                    </h1>
                    <p className="text-text/80 text-lg">
                        In celebration of the launch of this website, we&apos;re
                        inviting all daring players to put their fast learning
                        skills to the test and prove their dominance on whatever
                        this game is.
                    </p>
                    <p className="text-text/80 text-lg">
                        Join the Anarchy Win-Streak Challenge and try to get the
                        longest running win streak on the website.
                    </p>
                </div>
            </Card>

            <div className="grid w-full grid-rows-2 gap-3 lg:grid-cols-2 lg:grid-rows-1">
                <Card className="from-card to-background bg-card gap-10 p-5 sm:p-10 lg:bg-gradient-to-b">
                    <div className="flex flex-col gap-10 text-center">
                        <h1 className="text-secondary text-5xl italic">
                            THE OBJECTIVE:
                        </h1>

                        <h2 className="text-3xl text-balance">
                            Get the longest win streak you possibly can.
                        </h2>
                        <p className="text-xl text-balance">
                            Players with the highest win streak will rank higher
                            on the leaderboard!
                        </p>
                    </div>

                    <div className="border-secondary bg-background flex flex-col gap-3 rounded-3xl border p-5">
                        <h1 className="text-2xl text-blue-200">Prizes</h1>
                        <p className="text-xl">
                            $500 total prize pool split between the top three
                            players: 1st $300, 2nd $125, 3rd $75.
                        </p>
                    </div>

                    <div className="border-secondary bg-background flex flex-col gap-3 rounded-3xl border p-5">
                        <h1 className="text-2xl text-blue-200">How to Play</h1>
                        <p className="text-xl">
                            Play ranked matches throughout the week, each
                            consecutive win adds to your streak. Lose once, and
                            your streak resets!
                        </p>
                    </div>

                    <div className="border-secondary bg-background flex flex-col gap-3 rounded-3xl border p-5">
                        <h1 className="text-2xl text-blue-200">Leaderboards</h1>
                        <p className="text-xl">
                            Your highest streak will be verified and publicly
                            displayed on the leaderboard
                        </p>
                    </div>
                </Card>

                <Card className="from-card to-background gap-10 bg-gradient-to-b p-5 sm:p-10">
                    <h1 className="text-secondary text-center text-5xl italic">
                        THE RULES:
                    </h1>

                    <div className="bg-background flex flex-col gap-2 rounded-e-3xl border border-green-300 p-5">
                        <h1 className="text-3xl">CONTRIBUTES TO STREAK:</h1>
                        <p className="text-xl">
                            Rated games through matchmaking only.
                        </p>
                    </div>

                    <div className="bg-background flex flex-col gap-2 rounded-e-3xl border border-orange-300 p-5">
                        <h1 className="text-3xl">
                            DOES NOT CONTRIBUTE TO STREAK:
                        </h1>
                        <ul className="list-inside list-disc text-xl">
                            <li>Rematches</li>
                            <li>Direct challenges</li>
                            <li>Aborted games</li>
                            <li>Draws</li>
                            <li>Casual games</li>
                        </ul>
                        <p>
                            Games in these categories won&apos;t impact your
                            streak, so losing one won&apos;t break your run.
                        </p>
                    </div>

                    <div className="bg-background flex flex-col gap-2 rounded-e-3xl border border-red-300 p-5">
                        <h1 className="text-3xl">DO NOT:</h1>
                        <ul className="list-inside list-disc text-xl">
                            <li>Abuse accounts or share them</li>
                            <li>Collude or manipulate matches</li>
                            <li>Use cheats, bots, or exploits</li>
                        </ul>
                        <p>
                            We can see which games contributed to your streak.
                            Violations will lead to disqualification.
                        </p>
                    </div>
                </Card>
            </div>
        </main>
    );
}
