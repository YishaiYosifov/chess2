"use client";

import useLiveChessStore from "../../hooks/useLiveChessStore";
import Card from "@/components/ui/Card";
import LiveGameControls from "./LiveGameControls";
import GameOverControls from "./GameOverControls";

const GameControlsCard = () => {
    const resultData = useLiveChessStore((state) => state.resultData);

    return (
        <Card className="h flex-row justify-center gap-2">
            {resultData ? <GameOverControls /> : <LiveGameControls />}
        </Card>
    );
};
export default GameControlsCard;
