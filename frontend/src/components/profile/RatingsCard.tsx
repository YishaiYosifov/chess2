"use client";

import dynamic from "next/dynamic";
import clsx from "clsx";

import type { RatingOverview } from "@/types/tempModels";
import Card from "../helpers/Card";

const Chart = dynamic(() => import("react-apexcharts"), { ssr: false });

interface DataPoint {
    x: number;
    y: number;
}

const RatingCard = ({
    ratingData,
    className,
}: {
    ratingData: RatingOverview;
    className?: string;
}) => {
    const { history, current: currentRating, max: maxRating } = ratingData;

    // Format the rating history for the chart
    const formattedRartings: DataPoint[] = history.map((rating) => ({
        x: rating.achievedAt,
        y: rating.elo,
    }));
    formattedRartings.push({ x: new Date().getTime(), y: currentRating });

    const ratingChange = currentRating - (history.at(0)?.elo ?? 0);

    return (
        <Card className={clsx("flex flex-col gap-3", className)}>
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
                        name: "Elo",
                        data: formattedRartings,
                    },
                ]}
                height="100"
            />

            <section
                className="grid grid-cols-[1fr_min-content]"
                data-testid="ratingInfoSection"
            >
                <span>Current</span>
                <span className="justify-self-end" data-testid="currentRating">
                    {currentRating}
                </span>
                <span>Height</span>
                <span className="justify-self-end" data-testid="maxRating">
                    {maxRating}
                </span>
                <span>
                    Rating Chaing
                    <span className="text-text/50 ms-2">last month</span>
                </span>
                <span className="justify-self-end" data-testid="ratingChange">
                    {formatNumberWithSign(ratingChange)}
                </span>
            </section>
        </Card>
    );
};
export default RatingCard;

function formatNumberWithSign(num: number): string {
    if (num === 0) return "±0";
    return num > 0 ? `+${num}` : `${num}`;
}
