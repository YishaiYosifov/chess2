import { useMemo } from "react";
import constants from "@/lib/constants";
import useSignalREvent, { signalREventHookFactory } from "./useSignalREvent";
import useSignalREmitter, {
    signalREmitterHookFactory,
} from "./useSignalREmitter";
import { MoveKey } from "@/features/chessboard/lib/types";
import {
    Clocks,
    GameColor,
    GameResultData,
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

export type GameClientEvents = {
    MoveMadeAsync: [
        move: MoveSnapshot,
        sideToMove: GameColor,
        moveNumber: number,
        clocks: Clocks,
    ];
    LegalMovesChangedAsync: [
        encodedLegalMoves: string,
        hasForcedMoves: boolean,
    ];
    GameEndedAsync: [result: GameResultData];

    ChatMessageAsync: [sender: string, message: string];
    ChatMessageDeliveredAsync: [cooldownLeftMs: number];
    ChatConnectedAsync: [];
};

type GameHubEvents = {
    MovePieceAsync: [gameToken: string, key: MoveKey];

    EndGameAsync: [gameToken: string];
    RequestDrawAsync: [gameToken: string];
    DeclineDrawAsync: [gameToken: string];

    SendChatAsync: [gameToken: string, message: string];
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
