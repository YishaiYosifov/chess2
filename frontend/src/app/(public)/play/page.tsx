import PlayOptions from "@/components/play/PlayOptions";
import Chessboard from "@/components/game/Chessboard";

export const metadata = { title: "Play - Chess 2" };

const PlayPage = () => {
    return (
        <div className="w-full items-center justify-center gap-5 p-5 lg:grid lg:grid-cols-[auto_1fr]">
            <Chessboard
                className="hidden lg:block"
                offsetBreakpoints={[
                    {
                        maxScreenSize: 992,
                        paddingOffset: { width: 626, height: 100 },
                    },
                ]}
            />
            <PlayOptions />
        </div>
    );
};
export default PlayPage;
