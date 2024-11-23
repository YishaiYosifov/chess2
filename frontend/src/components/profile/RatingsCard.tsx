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
        x: rating.achievedAt.getTime(),
        y: rating.elo,
    }));
    formattedRartings.push({ x: new Date().getTime(), y: currentRating });

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
            <section className="grid grid-cols-2 gap-3">
                <span>Current</span>
                <span className="justify-self-end">{currentRating}</span>
                <span>Height</span>
                <span className="justify-self-end">{maxRating}</span>
                <span>Lowest</span>
                <span className="justify-self-end">{minRating}</span>
            </section>
        </Card>
    );
};
export default RatingCard;
