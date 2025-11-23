"use client";

import Button from "@/components/ui/Button";
import WinStreakLeaderboardCountdown from "@/features/winStreak/components/WinStreakLeaderboardCountdown";
import constants from "@/lib/constants";
import thumbnail from "@public/assets/win-streak/thumbnail.png";
import Image from "next/image";
import Link from "next/link";

const HomeWinStreakAd = () => {
    return (
        <section
            className="grid w-full grid-rows-[auto_auto] justify-center gap-10 bg-[#0a1117] p-15
                lg:grid-cols-[auto_auto] lg:grid-rows-1 lg:gap-x-20"
        >
            <div
                className="flex w-full flex-col items-center gap-3 text-center text-balance lg:max-w-lg
                    lg:items-start lg:text-start"
            >
                <h1 className="text-5xl">How Far Can You Go?</h1>
                <p className="text-text/80 text-xl">
                    Build the longest win streak and claim the cash prize
                </p>
                <WinStreakLeaderboardCountdown />

                <Link
                    href={constants.PATHS.WIN_STREAK}
                    className="w-full max-w-sm"
                >
                    <Button className="mt-5 w-full text-3xl">JOIN NOW</Button>
                </Link>
            </div>

            <Image
                src={thumbnail}
                alt="challenge thumbnail"
                width={400}
                height={225}
                className="mx-auto my-auto h-min rounded-xl object-contain"
            />
        </section>
    );
};
export default HomeWinStreakAd;
