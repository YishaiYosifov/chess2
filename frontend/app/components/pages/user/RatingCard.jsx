"use client";

import { Chart } from "react-google-charts";
import { Card } from "react-bootstrap";
import Image from "next/image";

import classes from "./RatingCard.module.scss";

const RatingCard = ({ variant, minRating, maxRating, archive }) => {
    const currentRating = archive.at(-1).elo;

    const archiveFormatted = archive.map((rating) => {
        return [new Date(rating.achievedAt).toLocaleString(), rating.elo];
    });
    archiveFormatted.push([new Date().toLocaleString(), currentRating]);

    const ratingChange = currentRating - archive[0].elo;

    let ratingChangeColor;
    let ratingChangeIcon = "±";
    if (ratingChange > 0) {
        ratingChangeColor = "text-success";
        ratingChangeIcon = "+";
    } else if (ratingChange < 0) {
        ratingChangeColor = "text-danger";
        ratingChangeIcon = "";
    }

    return (
        <Card
            xs={11}
            sm={8}
            md={5}
            xl={4}
            xxl={3}
            className={classes["rating-card"]}
        >
            <Card.Header className={classes["header"]}>
                <div>
                    <Image
                        className="img-fluid rounded-circle border border-3"
                        src={`/assets/modes/${variant}.webp`}
                        alt="Mode icon"
                        width={30}
                        height={30}
                    />
                    <span className="ms-2">{variant}</span>
                </div>

                <span className="text-white">{currentRating}</span>
            </Card.Header>
            <Card.Body className="text-start">
                <div className="mx-auto chart mb-3 w-100">
                    <Chart
                        chartType="AreaChart"
                        data={[["Date", "Elo"]].concat(archiveFormatted)}
                        options={{
                            legend: "none",
                            backgroundColor: { fill: "transparent" },
                            colors: ["#3ABA5A"],
                            height: 50,
                            vAxis: {
                                textPosition: "none",
                                gridlines: { color: "transparent" },
                            },
                            hAxis: {
                                textPosition: "none",
                                gridlines: { color: "transparent" },
                            },
                            chartArea: {
                                width: "100%",
                                height: "100%",
                            },
                            areaOpacity: 0.5,
                            focusTarget: "category",
                        }}
                    />
                </div>

                <div className={classes["info"]}>
                    <div>
                        <span>Higest</span>
                        <span className="text-success">{maxRating}</span>
                    </div>

                    <div>
                        <span>Lowest</span>
                        <span className="text-danger">{minRating}</span>
                    </div>

                    <div>
                        <span>Rating Change (last month)</span>
                        <span className={ratingChangeColor}>
                            {ratingChangeIcon}
                            {ratingChange}
                        </span>
                    </div>
                </div>
            </Card.Body>
        </Card>
    );
};
export default RatingCard;
