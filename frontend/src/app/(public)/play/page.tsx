import StaticChessboardWithSidebar from "@/features/chessboard/components/StaticChessboardWithSidebar";
import OpenSeekDirectory from "@/features/play/components/OpenSeekDirectory";
import PlayOptions from "@/features/play/components/PlayOptions";

export const metadata = { title: "Play - Chess 2" };

const PlayPage = () => {
    return (
        <StaticChessboardWithSidebar
            aside={
                <aside
                    className="grid h-full w-full max-w-xl min-w-xs grid-rows-[auto_1fr] gap-3 overflow-auto
                        lg:max-w-sm"
                >
                    <PlayOptions />
                    <OpenSeekDirectory />
                </aside>
            }
        />
    );
};
export default PlayPage;
