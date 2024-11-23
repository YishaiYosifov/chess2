"use client";

import dynamic from "next/dynamic";

import type { RatingOverview } from "@/lib/apiClient/models";
import Card from "../helpers/Card";

const Chart = dynamic(() => import("react-apexcharts"), { ssr: false });

interface DataPoint {
    x: number;
    y: number;
}

const RatingCard = ({ ratingData }: { ratingData: RatingOverview }) => {
    const {
        history,
        current: currentRating,
        min: minRating,
        max: maxRating,
    } = ratingData;

    // Format the rating history for the chart
    const formattedRartings: DataPoint[] = history.map((rating) => ({
        x: new Date(rating.achievedAt).getTime(),
        y: rating.elo,
    }));
    formattedRartings.push({ x: new Date().getTime(), y: currentRating });

    // This is for the "rating changed last month"
    // This code decides whether the text color and icon (+, - or ±)
    const ratingChange = currentRating - history[0].elo;
    let ratingChangeColorClass = "";
    let ratingChangeIcon = "";

    if (ratingChange > 0) {
        ratingChangeColorClass = "text-success";
        ratingChangeIcon = "+";
    } else if (ratingChange < 0) ratingChangeColorClass = "text-danger";
    else ratingChangeIcon = "±";

    return (
        <Card className="flex-col gap-3">
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
            <section className="flex flex-col gap-3">
                <span>Current</span>
                <span>Height</span>
                <span>Lowest</span>
            </section>
        </Card>
    );
};
export default RatingCard;
