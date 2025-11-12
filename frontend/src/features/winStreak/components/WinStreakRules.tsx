import Card from "@/components/ui/Card";

const WinStreakRules = () => {
    return (
        <Card className="from-card to-background gap-10 bg-gradient-to-b p-5 sm:p-10">
            <h1 className="text-secondary text-center text-5xl italic">
                THE RULES:
            </h1>

            <div className="bg-background flex flex-col gap-2 rounded-e-3xl border border-green-300 p-5">
                <h1 className="text-3xl">CONTRIBUTES TO STREAK:</h1>
                <p className="text-xl">Rated games through matchmaking only.</p>
            </div>

            <div className="bg-background flex flex-col gap-2 rounded-e-3xl border border-orange-300 p-5">
                <h1 className="text-3xl">DOES NOT CONTRIBUTE TO STREAK:</h1>
                <ul className="list-inside list-disc text-xl">
                    <li>Rematches</li>
                    <li>Direct challenges</li>
                    <li>Aborted games</li>
                    <li>Draws</li>
                    <li>Casual games</li>
                </ul>
                <p>
                    Games in these categories won&apos;t impact your streak, so
                    losing one won&apos;t break your run.
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
    );
};
export default WinStreakRules;
