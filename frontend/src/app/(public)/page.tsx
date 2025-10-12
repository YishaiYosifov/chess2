import Image from "next/image";

import FullBoard from "@public/assets/fullboard.webp";
import Button from "@/components/ui/Button";
import Card from "@/components/ui/Card";

export const metadata = { title: "Home - Chess 2" };

const HomePage = async () => {
    return (
        <div className="grid flex-1 grid-rows-[auto_1fr]">
            <section
                className="bg-checkerboard flex justify-center gap-10 bg-[#151515] bg-[length:10rem_10rem]
                    bg-center p-3"
            >
                <div className="grid grid-cols-2 items-stretch gap-10">
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
                            GET HELP
                        </Button>
                    </Card>
                </div>
            </section>
        </div>
    );
};
export default HomePage;
