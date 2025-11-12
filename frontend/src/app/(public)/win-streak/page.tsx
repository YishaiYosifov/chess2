import WinStreakObjective from "@/features/winStreak/components/WinStreakObjective";
import WinStreakHeader from "@/features/winStreak/components/WinStreakHeader";
import WinStreakRules from "@/features/winStreak/components/WinStreakRules";

export default async function WinStreakPage() {
    return (
        <main className="mx-auto flex max-w-6xl flex-1 flex-col items-center gap-3 p-5">
            <WinStreakHeader />

            <div className="grid w-full grid-rows-2 gap-3 lg:grid-cols-2 lg:grid-rows-1">
                <WinStreakObjective />
                <WinStreakRules />
            </div>
        </main>
    );
}
