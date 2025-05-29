import PlayOptions from "@/components/play/PlayOptions";
import Chessboard from "@/components/game/Chessboard";

export const metadata = { title: "Play - Chess 2" };

const PlayPage = () => {
    return (
        <div
            className="grid w-full max-w-full auto-cols-fr auto-rows-min justify-center gap-5 p-5
                lg:auto-rows-fr lg:grid-cols-[auto_auto]"
        >
            <div className="flex md:max-h-screen">
                <Chessboard
                    breakpoints={[
                        {
                            maxScreenSize: 768,
                            paddingOffset: { width: 10, height: 110 },
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
            <PlayOptions />
        </div>
    );
};
export default PlayPage;
