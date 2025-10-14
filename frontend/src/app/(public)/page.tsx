import Image from "next/image";

import StaticChessboard from "@/features/chessboard/components/StaticChessboard";
import Knook from "@public/assets/pieces-svg/knook-white.svg";
import HomePageReplay from "@public/data/homePageReplay.json";
import Button from "@/components/ui/Button";
import Pog from "@public/assets/pog.png";
import Card from "@/components/ui/Card";
import { GameReplay } from "@/features/chessboard/lib/types";
import Link from "next/link";
import constants from "@/lib/constants";

export const metadata = { title: "Home - Chess 2" };

const typedHomePageReplay = HomePageReplay as GameReplay[];

const HomePage = async () => {
    return (
        <div className="flex flex-1 flex-col gap-10">
            <header
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
                        priority
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
            </header>

            <section
                className="grid w-full grid-rows-[auto_auto] justify-center gap-10 p-5
                    lg:grid-cols-[auto_auto] lg:gap-x-20"
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
                        <span className="inline lg:hidden">
                            bellow
                        </span> though{" "}
                        <Image
                            src={Pog}
                            alt="pog"
                            width={20}
                            height={20}
                            className="inline-block"
                        />
                    </p>

                    <Button className="bg-secondary mt-5 w-full text-3xl text-black lg:max-w-sm">
                        PLAY NOW
                    </Button>
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
                    defaultOffset={{ width: 500, height: 0, maxSize: 400 }}
                    className="justify-self-center"
                    replays={typedHomePageReplay}
                    canDrag={false}
                />
            </section>

            <footer className="flex-1 bg-[#16101c]">
                <div
                    className="mx-auto grid max-w-5xl grid-cols-2 grid-rows-2 p-3 py-10 md:grid-cols-3
                        md:grid-rows-1"
                >
                    <div
                        className="col-span-2 flex flex-col items-center gap-3 text-center md:col-span-1
                            md:items-start md:text-start"
                    >
                        <h2 className="text-2xl font-bold">About Chess 2</h2>
                        <p className="text-text/70 text-sm">
                            Chess 2 is my reimagining of chess inspired by the
                            ridiculous and stupid ideas of{" "}
                            <Link
                                href="https://reddit.com/r/anarchychess"
                                className="text-link"
                            >
                                r/AnarchyChess
                            </Link>
                            . Developed by a single person (that person being me
                            :]) over a couple of years, it&apos;s full of weird
                            pieces, rules, and whatever came out of that
                            subreddit.
                        </p>
                    </div>

                    <div className="flex flex-col gap-3 text-center">
                        <h2 className="text-2xl font-bold">Quick Links</h2>
                        <ul className="text-text/70 flex flex-col gap-3">
                            <li>
                                <Link href={constants.PATHS.PLAY}>
                                    Play Now
                                </Link>
                            </li>
                            <li>
                                <Link href={constants.PATHS.QUESTS}>
                                    Daily Quests
                                </Link>
                            </li>
                            <li>
                                <Link href={constants.PATHS.GUIDE}>Guide</Link>
                            </li>
                        </ul>
                    </div>

                    <div className="flex flex-col items-center gap-3 md:items-end">
                        <h2 className="text-2xl font-bold">Follow Us</h2>
                        <div className="text-text/70 flex gap-3">
                            <Link href={constants.SOCIALS.DISCORD}>
                                Discord
                            </Link>
                            <Link href={constants.SOCIALS.YOUTUBE}>
                                YouTube
                            </Link>
                        </div>
                    </div>
                </div>
            </footer>
        </div>
    );
};
export default HomePage;
