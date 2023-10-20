"use client";

import { Card } from "react-bootstrap";
import Image from "next/image";

import { RatingData } from "@/app/user/[username]/page";
import styles from "./Ratings.module.scss";
import { Dictionary } from "@/lib/types";
import RatingModal from "./RatingModal";
import { useState } from "react";

const Ratings = ({ ratings }: { ratings: Dictionary<RatingData> }) => {
    return (
        <Card className={styles.ratings}>
            {Object.entries(ratings).map(([variant, data]) => (
                <RatingHolder
                    key={variant}
                    variant={variant}
                    ratingData={data}
                />
            ))}
        </Card>
    );
};
export default Ratings;

/** The square button that will show the ratings popup */
const RatingHolder = ({
    variant,
    ratingData,
}: {
    variant: string;
    ratingData: RatingData;
}) => {
    const [showModal, setShowModal] = useState(false);

    return (
        <>
            <Card
                className={styles["rating-holder"]}
                role="button"
                onClick={() => setShowModal(true)}
            >
                <Image
                    src="/assets/modes/anarchy.webp"
                    alt="test"
                    width={60}
                    height={60}
                />
                <p>{ratingData.current}</p>
            </Card>

            <RatingModal
                variant={variant}
                ratingData={ratingData}
                show={showModal}
                onHide={() => setShowModal(false)}
            />
        </>
    );
};
