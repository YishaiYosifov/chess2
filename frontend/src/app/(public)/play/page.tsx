import StaticChessboard from "@/features/chessboard/components/StaticChessboard";
import OpenSeekDirectory from "@/features/play/components/OpenSeekDirectory";
import PlayOptions from "@/features/play/components/PlayOptions";

export const metadata = { title: "Play - Chess 2" };

const PlayPage = () => {
    return (
        <div
            className="jutsify-center flex w-full flex-col items-center justify-center gap-5 p-5
                lg:h-screen lg:flex-row"
        >
            <div className="flex md:max-h-screen">
                <StaticChessboard
                    breakpoints={[
                        {
                            maxScreenSize: 768,
                            paddingOffset: { width: 40, height: 110 },
                        },
                        {
                            maxScreenSize: 1024,
                            paddingOffset: { width: 200, height: 50 },
                        },
                    ]}
                    defaultOffset={{ width: 626, height: 100 }}
                    className="m-auto"
                />
            </div>

            <aside className="grid h-full w-full max-w-xl min-w-xs grid-rows-[auto_1fr] gap-3 lg:max-w-sm">
                <PlayOptions />
                <OpenSeekDirectory />
            </aside>
        </div>
    );
};
export default PlayPage;
