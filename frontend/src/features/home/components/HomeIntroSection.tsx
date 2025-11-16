import Image from "next/image";

import StaticChessboard from "@/features/chessboard/components/StaticChessboard";
import homePageReplay from "@public/data/homePageReplay.json";
import { GameReplay } from "@/features/chessboard/lib/types";
import Button from "@/components/ui/Button";
import Pog from "@public/assets/pog.png";
import constants from "@/lib/constants";
import Link from "next/link";

const typedHomePageReplay = homePageReplay as GameReplay[];

const HomeIntroSection = () => {
    return (
        <section
            className="grid w-full grid-rows-[auto_auto] justify-center gap-10 bg-[#0d0a10] p-15
                lg:grid-cols-[auto_auto] lg:grid-rows-1 lg:gap-x-20"
        >
            <div
                className="flex w-full flex-col items-center gap-3 text-center lg:max-w-lg lg:items-start
                    lg:text-start"
            >
                <h1 className="text-5xl text-balance">
                    Discover the Madness of Anarchy Chess
                </h1>

                <p className="text-text/80 text-xl">
                    Don&apos;t worry, no one knows what&apos;s going on
                </p>
                <p className="text-text/80 text-sm">
                    look at this cool game{" "}
                    <span className="hidden lg:inline">to the side</span>
                    <span className="inline lg:hidden">bellow</span> though{" "}
                    <Image
                        src={Pog}
                        alt="pog"
                        width={20}
                        height={20}
                        className="inline-block"
                    />
                </p>

                <Link href={constants.PATHS.PLAY} className="w-full max-w-sm">
                    <Button className="bg-secondary mt-5 w-full text-3xl text-black">
                        PLAY NOW
                    </Button>
                </Link>
            </div>

            <StaticChessboard
                breakpoints={[
                    {
                        maxScreenSize: 767,
                        paddingOffset: {
                            width: 45,
                            height: 0,
                            maxSize: 600,
                        },
                    },
                    {
                        maxScreenSize: 1024,
                        paddingOffset: {
                            width: 145,
                            height: 0,
                            maxSize: 600,
                        }, // p-5 + p-5 + sidebar
                    },
                ]}
                defaultOffset={{ width: 700, height: 0, maxSize: 400 }}
                className="my-auto justify-self-center"
                replays={typedHomePageReplay}
                canDrag={false}
            />
        </section>
    );
};
export default HomeIntroSection;
