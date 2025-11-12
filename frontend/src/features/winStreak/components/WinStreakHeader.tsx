import Card from "@/components/ui/Card";
import Image from "next/image";

import thumbnail from "@public/assets/win-streak/thumbnail.png";

const WinStreakHeader = () => {
    return (
        <Card className="w-full gap-5 lg:flex-row">
            <Image
                src={thumbnail}
                alt="challenge thumbnail"
                width={400}
                height={225}
                className="mx-auto my-auto h-min rounded-xl object-contain"
            />

            <div className="flex flex-col justify-between gap-5">
                <h1 className="text-3xl">The Anarchy Win-Streak Challenge</h1>
                <p className="text-text/80 text-lg">
                    In celebration of the launch of this website, we&apos;re
                    inviting all daring players to put their fast learning
                    skills to the test and prove their dominance on whatever
                    this game is.
                </p>
                <p className="text-text/80 text-lg">
                    Join the Anarchy Win-Streak Challenge and try to get the
                    longest running win streak on the website.
                </p>
            </div>
        </Card>
    );
};
export default WinStreakHeader;
