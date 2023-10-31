"use client";

import { BsAlarmFill, BsPlayFill } from "react-icons/bs";
import { Card } from "react-bootstrap";

import scssVariables from "@/lib/variables.module.scss";
import { Variants } from "@/lib/constants";
import styles from "./play.module.scss";

import Chessboard from "@/components/Chessboard";

const PlayPage = () => {
    const breakpoint = parseInt(scssVariables.lg);
    return (
        <div className={styles.container}>
            <Chessboard
                variant={Variants.Anarchy}
                offsetBreakpoints={[
                    {
                        breakpoint,
                        offset: { width: 100, height: 200 },
                    },
                    {
                        breakpoint,
                        offset: { width: 500, height: 100 },
                    },
                ]}
            />

            <Card className={styles["options-card"]}>
                <section className={styles["variant-options"]}>
                    <p className={styles["option-title"]}>
                        <BsPlayFill /> Variant
                    </p>

                    <div className={styles["options-container"]}>
                        {Object.values(Variants).map((variant) => (
                            <VariantButton variant={variant} key={variant} />
                        ))}
                    </div>
                </section>

                <section className={styles["timecontrol-options"]}>
                    <p className={styles["option-title"]}>
                        <BsAlarmFill /> Time Control
                    </p>

                    <div className={styles["options-container"]}>
                        <Card className={styles["option-button"]}>
                            <span>1 + 0</span>
                            <span>bullet</span>
                        </Card>
                        <Card className={styles["option-button"]}>
                            <span>1 + 1</span>
                            <span>bullet</span>
                        </Card>
                        <Card className={styles["option-button"]}>
                            <span>2 + 1</span>
                            <span>bullet</span>
                        </Card>
                        <Card className={styles["option-button"]}>
                            <span>1 + 0</span>
                            <span>bullet</span>
                        </Card>
                        <Card className={styles["option-button"]}>
                            <span>1 + 1</span>
                            <span>bullet</span>
                        </Card>
                        <Card className={styles["option-button"]}>
                            <span>2 + 1</span>
                            <span>bullet</span>
                        </Card>
                        <Card className={styles["option-button"]}>
                            <span>1 + 0</span>
                            <span>bullet</span>
                        </Card>
                        <Card className={styles["option-button"]}>
                            <span>1 + 1</span>
                            <span>bullet</span>
                        </Card>
                        <Card className={styles["option-button"]}>
                            <span>2 + 1</span>
                            <span>bullet</span>
                        </Card>
                    </div>
                </section>
            </Card>
        </div>
    );
};
export default PlayPage;

const VariantButton = ({ variant }: { variant: string }) => {
    return <Card className={styles["option-button"]}>{variant}</Card>;
};
