import { Button, Modal } from "react-bootstrap";
import Image from "next/image";

import styles from "./RatingModal.module.scss";

import { RatingData } from "@/app/user/[username]/page";
import Chart from "react-google-charts";

/** The popup modal with the rating details */
const RatingModal = ({
    variant,
    ratingData,
    onHide,
    show,
}: {
    variant: string;
    ratingData: RatingData;
    onHide?: () => void;
    show?: boolean;
}) => {
    const {
        history,
        current: currentRating,
        minRating,
        maxRating,
    } = ratingData;

    // Format the rating history for google charts
    const title = [["Date", "Elo"]];
    const formattedRartings: [string, number][] = history.map((rating) => [
        new Date(rating.achievedAt).toLocaleString(),
        rating.elo,
    ]);
    formattedRartings.push([new Date().toLocaleString(), currentRating]);

    // This is for the "rating changed last month"
    // This code decides whether the text color and icon (+, - or ±)
    const ratingChange = currentRating - history[0].elo;
    const ratingChangeColorClass =
        ratingChange > 0
            ? "text-success"
            : ratingChange < 0
            ? "text-danger"
            : "";
    const ratingChangeIcon =
        ratingChange > 0 ? "+" : ratingChange < 0 ? "-" : "±";

    return (
        <Modal show={show} onHide={onHide} size="lg" centered>
            <Modal.Header closeButton>
                <Modal.Title>
                    <Image
                        className="img-fluid rounded-circle border border-3"
                        src={`/assets/modes/${variant}.webp`}
                        alt="Mode icon"
                        width={30}
                        height={30}
                    />
                    <span className="ms-2">{variant}</span>
                </Modal.Title>
            </Modal.Header>
            <Modal.Body>
                <Chart
                    chartType="AreaChart"
                    data={[...title, ...formattedRartings]}
                    options={{
                        legend: "none",
                        backgroundColor: "transparent",
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

                <div className={styles.info}>
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
                        <span className={ratingChangeColorClass}>
                            {ratingChangeIcon}
                            {ratingChange}
                        </span>
                    </div>
                </div>
            </Modal.Body>
            <Modal.Footer>
                <Button onClick={onHide}>Close</Button>
            </Modal.Footer>
        </Modal>
    );
};
export default RatingModal;
