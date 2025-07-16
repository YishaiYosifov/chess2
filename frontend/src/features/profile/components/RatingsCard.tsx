"use client";

import dynamic from "next/dynamic";

import Card from "@/components/ui/Card";
import { Rating } from "@/lib/apiClient";

const Chart = dynamic(() => import("react-apexcharts"), { ssr: false });

interface DataPoint {
    x: number;
    y: number;
}

const RatingCard = ({ ratings }: { ratings: Rating[] }) => {
    const currentRating = ratings.at(-1)?.rating ?? 0;
    const maxRating = Math.max(...ratings.map((x) => x.rating));

    // Format the rating history for the chart
    const formattedRartings: DataPoint[] = ratings.map((rating) => ({
        x: rating.at,
        y: rating.rating,
    }));
    if (formattedRartings.length === 1)
        formattedRartings.push({
            x: new Date().getTime(),
            y: currentRating,
        });

    const earliestRating = ratings.at(0)?.rating ?? 0;
    const ratingChange = currentRating - earliestRating;

    function formatNumberWithSign(num: number): string {
        if (num === 0) return "±0";
        return num > 0 ? `+${num}` : `${num}`;
    }

    return (
        <Card className="flex min-w-96 flex-col gap-3">
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
                    Rating Change
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
