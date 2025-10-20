"use client";

import useLiveChessStore from "../../hooks/useLiveChessStore";
import Card from "@/components/ui/Card";
import LiveGameControls from "./LiveGameControls";
import GameOverControls from "./GameOverControls";
import { JSX } from "react";

const GameControlsCard = () => {
    const { resultData, viewer } = useLiveChessStore((state) => ({
        viewer: state.viewer,
        resultData: state.resultData,
    }));

    let ControlsComponent: JSX.Element;
    if (viewer.playerColor === null || resultData) {
        ControlsComponent = <GameOverControls />;
    } else {
        ControlsComponent = <LiveGameControls />;
    }

    return (
        <Card className="flex-row justify-center gap-2">
            {ControlsComponent}
        </Card>
    );
};
export default GameControlsCard;
