import WithSession from "@/features/auth/components/WithSession";
import StaticChessboardWithSidebar from "@/features/chessboard/components/StaticChessboardWithSidebar";
import OpenSeekDirectory from "@/features/play/components/OpenSeekDirectory";
import PlayOptions from "@/features/play/components/PlayOptions";

export const metadata = { title: "Play - Chess 2" };

export default function PlayPage() {
    return (
        <WithSession>
            <StaticChessboardWithSidebar
                aside={
                    <aside className="grid h-full w-full min-w-xs grid-rows-[auto_1fr] gap-3 lg:max-w-sm">
                        <PlayOptions />
                        <OpenSeekDirectory />
                    </aside>
                }
            />
        </WithSession>
    );
}
