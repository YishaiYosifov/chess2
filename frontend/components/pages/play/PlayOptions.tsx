"use client";

import { BsAlarmFill, BsPlayFill } from "react-icons/bs";
import { Card, Spinner } from "react-bootstrap";

import { useState } from "react";

import { TIME_CONTROLS, Variant } from "@/lib/constants";
import { apiRequest } from "@/lib/utils/common";
import styles from "./PlayOptions.module.scss";

interface TimeControl {
    timeControl: number;
    increment: number;
}

interface GameRequest extends TimeControl {
    variant: Variant;
}

/**
 * Card containing the variant and time control options.
 * When the one of the time control buttons is clicked, a request to enter the pool will be sent.
 */
const PlayOptions = () => {
    const [selectedVariant, setSelectedVariant] = useState<Variant>(
        Variant.Anarchy
    );
    const [selectedTimeControl, setSelectedTimeControl] =
        useState<TimeControl>();
    const [gameRequest, setGameRequest] = useState<GameRequest>();
    const [status, setStatus] = useState<string>("");

    /**
     * Cancels the outgoing game request
     */
    async function cancelRequest(): Promise<void> {
        if (!gameRequest) return;

        await apiRequest("/game/cancel", {
            method: "delete",
        });
        setSelectedTimeControl(undefined);
        setGameRequest(undefined);
    }

    /**
     * Send a request to enter the game pool.
     * If the game options are the same as the last request, it will send a request to cancel it.
     */
    async function enterPool(
        timeControl: number,
        increment: number,
        variant: Variant
    ): Promise<void> {
        if (
            gameRequest?.increment == increment &&
            gameRequest.timeControl == timeControl &&
            gameRequest.variant == variant
        ) {
            cancelRequest();
            return;
        }

        const response = await apiRequest("/game/pool/start", {
            json: {
                variant: variant,
                time_control: timeControl,
                increment,
            },
        });
        if (!response.ok) {
            cancelRequest();
            setStatus("something went wrong...");
            return;
        }

        setGameRequest({ timeControl, increment, variant });
        setSelectedTimeControl({ timeControl, increment });
    }

    return (
        <Card className={styles["options-card"]}>
            <section className={styles["variant-options"]}>
                <p className={styles["option-title"]}>
                    <BsPlayFill /> Variant
                </p>

                <div className={styles["options-container"]}>
                    {Object.values(Variant).map((buttonVariant) => (
                        <VariantButton
                            buttonVariant={buttonVariant}
                            selectedVariant={selectedVariant}
                            onVariantChange={(variant) => {
                                cancelRequest();
                                setSelectedVariant(variant);
                            }}
                            key={buttonVariant}
                        />
                    ))}
                </div>
            </section>

            <section className={styles["timecontrol-options"]}>
                <p className={styles["option-title"]}>
                    <BsAlarmFill /> Time Control
                </p>

                <div className={styles["options-container"]}>
                    {TIME_CONTROLS.map((timeControl, index) => (
                        <TimeControlButton
                            {...timeControl}
                            key={index}
                            selectedTimeControl={selectedTimeControl}
                            onTimeControlChange={(timeControl, increment) =>
                                enterPool(
                                    timeControl,
                                    increment,
                                    selectedVariant
                                )
                            }
                        />
                    ))}
                </div>
            </section>

            <p className={styles["status-text"]}>{status}</p>
        </Card>
    );
};
export default PlayOptions;

/**
 * Display the variant options.
 *
 * @param buttonVariant - the variant to display on the button
 * @param selectedVariant - the currently selected variant
 * @param onVariantChange - the function to call when the button is clicked
 */
const VariantButton = ({
    buttonVariant,
    selectedVariant,
    onVariantChange,
}: {
    buttonVariant: Variant;
    selectedVariant: Variant;
    onVariantChange: (newVariant: Variant) => void;
}) => {
    return (
        <Card
            className={`${styles["option-button"]} ${
                selectedVariant === buttonVariant && styles["selected-variant"]
            }`}
            onClick={() => onVariantChange(buttonVariant)}
        >
            {buttonVariant}
        </Card>
    );
};

/**
 * Display the time control options.
 * The top text will be `timeCotrol + increment`, and the bottom text will be `type`.
 *
 * @param timeControl - the time control to display
 * @param increment - the increment to display
 * @param type - the type of time control (bullet, blitz or rapid)
 * @param selectedTimeControl - the currently selected time control
 * @param onTimeControlChange - the function to call when the option is clicked
 */
const TimeControlButton = ({
    timeControl,
    increment,
    type,

    selectedTimeControl,
    onTimeControlChange,
}: {
    timeControl: number;
    increment: number;
    type: string;

    selectedTimeControl?: TimeControl;
    onTimeControlChange: (timeControl: number, increment: number) => void;
}) => {
    const isSelected =
        selectedTimeControl &&
        selectedTimeControl.timeControl === timeControl &&
        selectedTimeControl.increment === increment;
    return (
        <Card
            className={`${styles["time-control-option-button"]} ${
                isSelected && styles["selected-time-control"]
            }`}
            onClick={() => onTimeControlChange(timeControl, increment)}
        >
            {isSelected && (
                <Spinner
                    animation="border"
                    role="status"
                    className={styles.spinner}
                >
                    <span className="visually-hidden">searching...</span>
                </Spinner>
            )}
            <span>
                {timeControl / 60} + {increment}
            </span>
            <span>{type}</span>
        </Card>
    );
};
