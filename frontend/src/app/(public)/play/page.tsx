import PlayOptions from "@/components/play/PlayOptions";
import Chessboard from "@/components/game/Chessboard";

export const metadata = { title: "Play - Chess 2" };

const PlayPage = () => {
    const breakpoint = parseInt(scssVariables.xl);
    return (
        <div className={styles.container}>
            <Chessboard
                offsetBreakpoints={[
                    {
                        widthBreakpoint: breakpoint,
                        offset: { width: 10, height: 200 },
                    },
                    {
                        widthBreakpoint: breakpoint,
                        offset: { width: 626, height: 100 },
                    },
                ]}
            />

            <PlayOptions />
        </div>
    );
};
export default PlayPage;
