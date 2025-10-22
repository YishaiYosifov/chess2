"use client";

import { StaticImageData } from "next/image";
import Carousel from "@/components/Carousel";

const GuideCard = ({
    title,
    points,
    images,
}: {
    title: string;
    points: string[];
    images: StaticImageData[];
}) => {
    return (
        <div
            className="grid grid-rows-[auto_auto] items-center gap-5 rounded-md bg-white/5 p-5
                md:grid-cols-[auto_min-content] md:grid-rows-1"
        >
            <div className="flex h-full flex-1 flex-col gap-5">
                <h1 className="text-3xl" data-testid="guideCardTitle">
                    {title}
                </h1>
                {points.length > 1 ? (
                    <ul
                        className="list-inside list-disc space-y-3 text-sm text-balance"
                        data-testid="guideCardPoints"
                    >
                        {points.map((point, i) => (
                            <li key={i}>{point}</li>
                        ))}
                    </ul>
                ) : (
                    <p
                        className="text-sm text-balance"
                        data-testid="guideCardSinglePoint"
                    >
                        {points[0]}
                    </p>
                )}
            </div>

            <Carousel
                images={images}
                width={200}
                height={200}
                alt={`${title} Example`}
            />
        </div>
    );
};
export default GuideCard;
