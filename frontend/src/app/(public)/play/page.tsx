import PlayOptions from "@/components/play/PlayOptions";
import Chessboard from "@/components/game/Chessboard";

export const metadata = { title: "Play - Chess 2" };

const PlayPage = () => {
    return (
        <div
            className="grid w-full max-w-full auto-cols-fr auto-rows-min items-center justify-center
                gap-5 p-5 lg:auto-rows-fr lg:grid-cols-[auto_auto]"
        >
            <Chessboard
                breakpoints={[
                    {
                        maxScreenSize: 768,
                        paddingOffset: { width: 10, height: 200 },
                    },
                    {
                        maxScreenSize: 1024,
                        paddingOffset: { width: 10, height: 100 },
                    },
                ]}
                defaultOffset={{ width: 626, height: 100 }}
                className="justify-self-center"
            />
            <PlayOptions />
        </div>
    );
};
export default PlayPage;
