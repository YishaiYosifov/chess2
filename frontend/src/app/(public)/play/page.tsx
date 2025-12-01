import { Metadata } from "next";

import StaticChessboardWithSidebar from "@/features/chessboard/components/StaticChessboardWithSidebar";
import OpenSeekDirectory from "@/features/play/components/OpenSeekDirectory";
import WithOptionalSession from "@/features/auth/hocs/WithOptionalSession";
import PlayOptions from "@/features/play/components/PlayOptions";

export const metadata: Metadata = {
    title: "Play - Anarchy Chess",
    description:
        "Play Anarchy Chess online with chaotic rules and custom pieces. Join a live game or explore open challenges.",
    keywords: [
        "play chess online",
        "anarchy chess",
        "chess variants",
        "live chess",
        "custom chess",
    ],
};

export default function PlayPage() {
    return (
        <WithOptionalSession>
            <StaticChessboardWithSidebar
                aside={
                    <aside className="grid h-full w-full min-w-xs grid-rows-[auto_1fr] gap-3 lg:max-w-sm">
                        <PlayOptions />
                        <OpenSeekDirectory />
                    </aside>
                }
            />
        </WithOptionalSession>
    );
}
