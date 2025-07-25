"use client";

import { GameState } from "@/lib/apiClient";
import { memo } from "react";
import LiveChessboard from "./LiveChessboard";
import { createStoreProps } from "../lib/gameStateProcessor";

const GameStatePreprocessor = ({
    gameToken,
    gameState,
    userId,
}: {
    gameToken: string;
    gameState: GameState;
    userId: string;
}) => {
    const { live, board } = createStoreProps(gameToken, userId, gameState);
    return (
        <LiveChessboard
            gameToken={gameToken}
            userId={userId}
            liveProps={live}
            boardProps={board}
        />
    );
};
export default memo(GameStatePreprocessor);
