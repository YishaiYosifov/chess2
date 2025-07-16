"use client";

import dynamic from "next/dynamic";

import Card from "@/components/ui/Card";
import { RatingOverview } from "@/lib/apiClient";
import constants from "@/lib/constants";
import { getTimeControlIcon } from "../utils/timeControlIcons";
import clsx from "clsx";

const Chart = dynamic(() => import("react-apexcharts"), { ssr: false });

interface DataPoint {
    x: number;
    y: number;
}

const RatingCard = ({ overview }: { overview: RatingOverview }) => {
    const {
        timeControl,
        ratings,
        highest: highestRating,
        lowest: lowestRating,
        current: currentRating,
    } = overview;

    let formattedRatings: DataPoint[];
    // if there are enough recent rating points, use them directly
    if (ratings.length >= 2) {
        formattedRatings = ratings.map(({ at, rating }) => ({
            x: at,
            y: rating,
        }));
    } else {
        // otherwise show a flat line at the current rating over the last month
        const monthAgo = Date.now() - 1000 * 60 * 60 * 24 * 30;
        formattedRatings = [
            { x: monthAgo, y: currentRating },
            { x: Date.now(), y: currentRating },
        ];
    }

    const earliestRating = ratings.at(0)?.rating ?? 0;
    const ratingChange = currentRating - earliestRating;

    function formatNumberWithSign(num: number): string {
        if (num === 0) return "±0";
        return num > 0 ? `+${num}` : `${num}`;
    }

    function getRatingChangeColor(): string {
        if (ratingChange === 0) return "text-neutral-400";
        return ratingChange > 0 ? "text-green-400" : "text-red-400";
    }

    return (
        <Card className="min-w-96 flex-col gap-3">
            <section className="flex justify-between">
                <span className="flex gap-2">
                    {constants.TIME_CONTROL_LABELS[timeControl]}
                    {getTimeControlIcon(timeControl)}
                </span>
                {currentRating}
            </section>

            <Chart
                options={{
                    chart: {
                        type: "line",
                        background: "#0F0C14",
                        sparkline: {
                            enabled: true,
                        },
                        zoom: {
                            enabled: false,
                        },
                        toolbar: {
                            show: false,
                        },
                    },
                    xaxis: {
                        labels: {
                            show: false,
                        },
                        crosshairs: {
                            show: false,
                        },
                    },
                    yaxis: {
                        labels: {
                            show: false,
                        },
                    },
                    fill: {
                        type: "solid",
                        colors: ["#B8ABCE"],
                    },
                    tooltip: {
                        theme: "dark",
                        x: {
                            formatter: (val) =>
                                new Date(val).toLocaleString("en-GB", {
                                    day: "2-digit",
                                    month: "2-digit",
                                    year: "2-digit",
                                    hour: "2-digit",
                                    minute: "2-digit",
                                    hour12: true,
                                }),
                        },
                    },
                }}
                series={[
                    {
                        name: "Rating",
                        data: formattedRatings,
                    },
                ]}
                height="100"
            />

            <section
                className="grid grid-cols-[1fr_min-content]"
                data-testid="ratingInfoSection"
            >
                <span>Current</span>
                <span data-testid="currentRating">{currentRating}</span>
                <span>Height</span>
                <span
                    className="text-end text-green-400"
                    data-testid="maxRating"
                >
                    {highestRating}
                </span>
                <span>Lowest</span>
                <span className="text-end text-red-400" data-testid="maxRating">
                    {lowestRating}
                </span>
                <span>
                    Rating Change
                    <span className="text-text/50 ms-2">last month</span>
                </span>
                <span
                    className={clsx(getRatingChangeColor(), "text-end")}
                    data-testid="ratingChange"
                >
                    {formatNumberWithSign(ratingChange)}
                </span>
            </section>
        </Card>
    );
};
export default RatingCard;
