import { Metadata } from "next";

import StaticChessboardWithSidebar from "@/features/chessboard/components/StaticChessboardWithSidebar";
import OpenSeekDirectory from "@/features/play/components/OpenSeekDirectory";
import WithOptionalSession from "@/features/auth/hocs/WithOptionalSession";
import PlayOptions from "@/features/play/components/PlayOptions";
import PlayActions from "@/features/play/components/PlayActions";

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
                    <aside className="flex h-full w-full min-w-xs flex-col gap-3 overflow-auto lg:max-w-sm">
                        <PlayOptions />
                        <PlayActions />
                        <OpenSeekDirectory />
                    </aside>
                }
            />
        </WithOptionalSession>
    );
}
