import Image from "next/image";

import Knook from "@public/assets/pieces-svg/knook-white.svg";
import Button from "@/components/ui/Button";
import Card from "@/components/ui/Card";
import constants from "@/lib/constants";
import Link from "next/link";

const HomeHero = () => {
    return (
        <header
            className="bg-checkerboard relative flex flex-col items-center justify-center gap-10
                overflow-hidden bg-center p-3 py-10 md:gap-10 md:px-10 xl:flex-row
                xl:items-start xl:gap-20 xl:py-30"
        >
            <div
                className="relative flex h-full flex-col text-center text-nowrap xl:text-start"
                data-testid="homeHeroBanner"
            >
                <p className="text-4xl sm:text-5xl">WELCOME TO</p>
                <h1 className="text-7xl min-[375px]:text-8xl sm:text-9xl xl:ml-10">
                    CHESS
                </h1>
                <h2 className="text-3xl xl:text-end">The Anarchy Update</h2>

                <Image
                    src={Knook}
                    alt="knook"
                    width={700}
                    className="absolute -bottom-107 -left-70 hidden max-w-none rotate-15 xl:block"
                    priority
                />
            </div>

            <div className="grid h-min max-w-180 grid-cols-2 items-stretch gap-10">
                <Card className="border-secondary relative col-span-2 gap-5 border-4 p-2">
                    <div className="flex flex-col gap-2">
                        <h1 className="text-4xl text-balance sm:text-5xl">
                            PLAY ANARCHY CHESS!
                        </h1>
                        <p className="text-sm md:text-lg">
                            Experience the stupidest chess variant of all time
                        </p>
                    </div>

                    <Link href={constants.PATHS.PLAY} className="w-full">
                        <Button className="bg-secondary w-full text-3xl text-black">
                            PLAY NOW
                        </Button>
                    </Link>
                </Card>

                <Card className="bg-background border-secondary border-4 p-2 sm:gap-10">
                    <div className="flex flex-col gap-2">
                        <h1 className="text-3xl">DAILY QUEST</h1>
                        <p className="text-sm text-balance md:text-base">
                            Every day, new challenge
                        </p>
                    </div>

                    <Link href={constants.PATHS.QUESTS} className="w-full">
                        <Button className="mt-auto w-full text-[clamp(0.8rem,4vw,2rem)] sm:text-3xl">
                            PLAY QUEST
                        </Button>
                    </Link>
                </Card>

                <Card className="bg-background border-secondary border-4 p-2 sm:gap-10">
                    <div className="flex flex-col gap-2">
                        <h1 className="text-3xl">NEW RULES</h1>
                        <p className="text-sm text-balance md:text-base">
                            Rules? In this economy?
                        </p>
                    </div>

                    <Link href={constants.PATHS.GUIDE} className="w-full">
                        <Button className="mt-auto w-full text-[clamp(0.8rem,4vw,2rem)] sm:text-3xl">
                            VIEW GUIDE
                        </Button>
                    </Link>
                </Card>
            </div>
        </header>
    );
};
export default HomeHero;
