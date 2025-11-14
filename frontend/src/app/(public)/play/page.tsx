import StaticChessboardWithSidebar from "@/features/chessboard/components/StaticChessboardWithSidebar";
import OpenSeekDirectory from "@/features/play/components/OpenSeekDirectory";
import WithOptionalSession from "@/features/auth/hocs/WithOptionalSession";
import PlayOptions from "@/features/play/components/PlayOptions";

export const metadata = { title: "Play - Anarchy Chess" };

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
