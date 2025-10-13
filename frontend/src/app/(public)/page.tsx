import Image from "next/image";

import StaticChessboard from "@/features/chessboard/components/StaticChessboard";
import Knook from "@public/assets/pieces-svg/knook-white.svg";
import Button from "@/components/ui/Button";
import Pog from "@public/assets/pog.png";
import Card from "@/components/ui/Card";

export const metadata = { title: "Home - Chess 2" };

const HomePage = async () => {
    return (
        <div className="flex flex-1 flex-col gap-10">
            <section
                className="bg-checkerboard relative flex flex-col items-center justify-center gap-3
                    overflow-hidden bg-[#151515] bg-[length:10rem_10rem] bg-center p-3 py-30
                    md:gap-10 md:px-10 xl:flex-row xl:items-start xl:gap-20"
            >
                <div className="relative flex h-full flex-col text-center text-nowrap xl:text-start">
                    <p className="text-4xl sm:text-5xl">WELCOME TO</p>
                    <h1 className="text-7xl min-[375px]:text-8xl sm:text-9xl">
                        CHESS 2
                    </h1>

                    <Image
                        src={Knook}
                        alt="knook"
                        width={700}
                        className="absolute -bottom-100 -left-60 hidden max-w-none rotate-15 xl:block"
                    />
                </div>

                <div className="grid h-min max-w-180 grid-cols-2 items-stretch gap-10">
                    <Card className="border-secondary relative col-span-2 gap-5 border-4 p-2">
                        <div className="flex flex-col gap-2">
                            <h1 className="text-4xl text-balance sm:text-5xl">
                                PLAY CHESS 2!
                            </h1>
                            <p className="text-sm md:text-lg">
                                Experience the stupidest chess variant of all
                                time
                            </p>
                        </div>

                        <Button className="bg-secondary text-3xl text-black">
                            PLAY NOW
                        </Button>
                    </Card>

                    <Card className="bg-background border-secondary border-4 p-2 sm:gap-10">
                        <div className="flex flex-col gap-2">
                            <h1 className="text-3xl">DAILY QUEST</h1>
                            <p className="text-sm text-balance md:text-base">
                                Every day, new challenge
                            </p>
                        </div>

                        <Button className="mt-auto text-[clamp(0.8rem,4vw,2rem)] sm:text-3xl">
                            PLAY QUEST
                        </Button>
                    </Card>

                    <Card className="bg-background border-secondary border-4 p-2 sm:gap-10">
                        <div className="flex flex-col gap-2">
                            <h1 className="text-3xl">NEW RULES</h1>
                            <p className="text-sm text-balance md:text-base">
                                Rules? In this economy?
                            </p>
                        </div>

                        <Button className="mt-auto text-[clamp(0.8rem,4vw,2rem)] sm:text-3xl">
                            VIEW GUIDE
                        </Button>
                    </Card>
                </div>
            </section>

            <section
                className="grid w-full grid-rows-[min-content_auto] justify-center gap-20 p-5
                    lg:grid-cols-[minmax(auto,max-content)_auto]"
            >
                <div className="flex w-full flex-col gap-3 lg:max-w-lg">
                    <h1 className="text-5xl">
                        Discover the Madness of Anarchy Chess
                    </h1>

                    <p className="text-text/80 text-xl">
                        Don&apos;t worry, no one knows what&apos;s going on
                    </p>
                    <p className="text-text/80 flex gap-2 text-sm">
                        look at this cool game on the side though
                        <Image src={Pog} alt="pog" width={20} height={20} />
                    </p>

                    <Button className="bg-secondary mt-5 max-w-sm text-3xl text-black">
                        PLAY NOW
                    </Button>
                </div>

                <StaticChessboard
                    breakpoints={[
                        {
                            maxScreenSize: 767,
                            paddingOffset: { width: 45, height: 0 },
                        },
                        {
                            maxScreenSize: 1024,
                            paddingOffset: { width: 145, height: 0 }, // p-5 + p-5 + sidebar
                        },
                    ]}
                    defaultOffset={{ width: 500, height: 0, maxSize: 400 }}
                    className="justify-self-center"
                />
            </section>
        </div>
    );
};
export default HomePage;
