import Link from "next/link";

import constants from "@/lib/constants";

const HomeFooter = () => {
    return (
        <footer className="flex-1 bg-[#080e12]">
            <div
                className="mx-auto grid max-w-5xl grid-cols-2 grid-rows-2 gap-5 p-3 py-10 md:grid-cols-3
                    md:grid-rows-1"
            >
                <div
                    className="col-span-2 flex flex-col items-center gap-3 text-center md:col-span-1
                        md:items-start md:text-start"
                >
                    <h2 className="text-2xl font-bold">About Anarchy Chess</h2>
                    <p className="text-text/70 text-sm">
                        Anarchy Chess is my reimagining of chess inspired by the
                        ridiculous and stupid ideas of{" "}
                        <Link
                            href="https://reddit.com/r/anarchychess"
                            className="text-link"
                        >
                            r/AnarchyChess
                        </Link>
                        . This website is literally just a shitpost that I spend
                        a stupid amount of time on.
                    </p>
                </div>

                <div className="flex flex-col gap-3 text-center">
                    <h2 className="text-2xl font-bold">Quick Links</h2>
                    <ul className="text-text/70 flex flex-col gap-3">
                        <li>
                            <Link href={constants.PATHS.PLAY}>Play Now</Link>
                        </li>
                        <li>
                            <Link href={constants.PATHS.DONATE}>Donate</Link>
                        </li>
                        <li>
                            <Link href={constants.PATHS.QUESTS}>
                                Daily Quests
                            </Link>
                        </li>
                        <li>
                            <Link href={constants.PATHS.GUIDE}>Guide</Link>
                        </li>
                        <li>
                            <Link href={constants.PATHS.GITHUB}>
                                Source Code
                            </Link>
                        </li>
                    </ul>
                </div>

                <div className="flex flex-col items-center gap-3 md:items-end">
                    <h2 className="text-2xl font-bold">Follow Us</h2>
                    <div className="text-text/70 flex gap-3">
                        <Link href={constants.PATHS.DISCORD}>Discord</Link>
                        <Link href={constants.PATHS.YOUTUBE}>YouTube</Link>
                    </div>
                </div>
            </div>
        </footer>
    );
};
export default HomeFooter;
