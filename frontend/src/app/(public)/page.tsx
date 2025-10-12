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
                className="bg-checkerboard relative flex justify-center gap-20 bg-[#151515]
                    bg-[length:10rem_10rem] bg-center p-3 py-30"
            >
                <div className="absolute inset-0 overflow-clip">
                    <Image
                        src={Knook}
                        alt="knook"
                        draggable={false}
                        className="absolute bottom-[-35%] left-1/2 w-auto max-w-none -translate-x-1/1 rotate-15
                            object-contain select-none md:block"
                    />
                </div>

                <div className="flex flex-col">
                    <p className="text-5xl">WELCOME TO</p>
                    <h1 className="text-9xl">CHESS 2</h1>
                </div>

                <div className="grid h-min grid-cols-2 items-stretch gap-10">
                    <Card className="border-secondary col-span-2 w-full max-w-128 flex-col gap-5 border-4">
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

                    <Card className="bg-background border-secondary aspect-square gap-10 border-4">
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

                    <Card className="bg-background border-secondary aspect-square gap-10 border-4">
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
        </div>
    );
};
export default HomePage;
