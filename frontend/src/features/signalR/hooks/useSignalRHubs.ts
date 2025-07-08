import constants from "@/lib/constants";
import useSignalREvent, { signalREventHookFactory } from "./useSignalREvent";
import useSignalREmitter, {
    signalREmitterHookFactory,
} from "./useSignalREmitter";
import { GameResult, Point } from "@/types/tempModels";
import { useMemo } from "react";
import {
    Clocks,
    GameColor,
    MoveSnapshot,
    TimeControlSettings,
} from "@/lib/apiClient";

type MatchmakingClientEvents = {
    MatchFoundAsync: [token: string];
    MatchFailedAsync: [];
};

type MatchmakingHubEvents = {
    SeekRatedAsync: [timeControl: TimeControlSettings];
    SeekCasualAsync: [timeControl: TimeControlSettings];
    CancelSeekAsync: [];
};

export const useMatchmakingEvent =
    signalREventHookFactory<MatchmakingClientEvents>(
        constants.SIGNALR_PATHS.MATCHMAKING,
    );

export const useMatchmakingEmitter =
    signalREmitterHookFactory<MatchmakingHubEvents>(
        constants.SIGNALR_PATHS.MATCHMAKING,
    );

type GameClientEvents = {
    MoveMadeAsync: [
        move: MoveSnapshot,
        sideToMove: GameColor,
        moveNumber: number,
        clocks: Clocks,
    ];
    LegalMovesChangedAsync: [legalMoves: string[]];
    GameEndedAsync: [
        result: GameResult,
        resultDescription: string,
        newWhiteRating: number | undefined,
        newBlackRating: number | undefined,
    ];
};

type GameHubEvents = {
    MovePieceAsync: [gameToken: string, from: Point, to: Point];
    EndGameAsync: [gameToken: string];
};

export function useGameEvent<
    TEventName extends Extract<keyof GameClientEvents, string>,
>(
    gameToken: string,
    eventName: TEventName,
    onEvent?: (...args: GameClientEvents[TEventName]) => void,
) {
    const url = useMemo(() => {
        const u = new URL(constants.SIGNALR_PATHS.GAME);
        u.searchParams.append("gameToken", gameToken);
        return u.toString();
    }, [gameToken]);

    return useSignalREvent<GameClientEvents, TEventName>(
        url,
        eventName,
        onEvent,
    );
}

export function useGameEmitter(gameToken: string) {
    const url = useMemo(() => {
        const u = new URL(constants.SIGNALR_PATHS.GAME);
        u.searchParams.append("gameToken", gameToken);
        return u.toString();
    }, [gameToken]);

    return useSignalREmitter<GameHubEvents>(url);
}
