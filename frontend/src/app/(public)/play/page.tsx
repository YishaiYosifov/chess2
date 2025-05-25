import PlayOptions from "@/components/play/PlayOptions";
import Chessboard from "@/components/game/Chessboard";
import Card from "@/components/helpers/Card";

export const metadata = { title: "Play - Chess 2" };

const PlayPage = () => {
    return (
        <div>
            <Chessboard
                offsetBreakpoints={[
                    {
                        widthBreakpoint: 10,
                        offset: { width: 10, height: 200 },
                    },
                    {
                        widthBreakpoint: 1000,
                        offset: { width: 626, height: 100 },
                    },
                ]}
            />
            {/* <PlayOptions /> */}
        </div>
    );
};
export default PlayPage;
