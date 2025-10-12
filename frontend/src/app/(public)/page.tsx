import Image from "next/image";

import Knook from "@public/assets/pieces-svg/knook-white.svg";
import FullBoard from "@public/assets/fullboard.webp";
import Button from "@/components/ui/Button";
import Card from "@/components/ui/Card";

export const metadata = { title: "Home - Chess 2" };

const HomePage = async () => {
    return (
        <div className="grid flex-1 grid-rows-[auto_1fr]">
            <section
                className="bg-checkerboard relative flex flex-col items-center justify-center gap-3
                    bg-[#151515] bg-[length:10rem_10rem] bg-center p-3 py-30 md:gap-10 md:px-10
                    xl:flex-row xl:items-start xl:gap-20"
            >
                <div className="relative flex h-full flex-col text-center text-nowrap xl:text-start">
                    <p className="text-4xl sm:text-5xl">WELCOME TO</p>
                    <h1 className="text-7xl min-[375px]:text-8xl sm:text-9xl">
                        CHESS 2
                    </h1>

                    <Image
                        src={Knook}
                        alt="knook"
                        width={800}
                        className="absolute -bottom-110 -left-80 hidden max-w-none rotate-15 xl:block"
                    />
                </div>

                <div className="grid h-min max-w-128 grid-cols-2 items-stretch gap-10">
                    <Card className="border-secondary col-span-2 gap-5 border-4">
                        <header className="flex items-center gap-3">
                            <div className="flex h-full flex-col gap-3">
                                <h1 className="text-3xl text-balance sm:text-4xl">
                                    PLAY CHESS 2
                                </h1>
                                <p className="text-sm md:text-base">
                                    Experience the stupidest chess variant of
                                    all time
                                </p>
                            </div>

                            <Image
                                src={FullBoard}
                                className="max-h-32 w-auto object-contain"
                                alt="example board"
                            />
                        </header>

                        <Button className="bg-secondary text-3xl text-black">
                            PLAY NOW!
                        </Button>
                    </Card>

                    <Card className="bg-background border-secondary border-4">
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

                    <Card className="bg-background border-secondary border-4">
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

            <div className="bg-background z-5 h-full w-full"></div>
        </div>
    );
};
export default HomePage;
