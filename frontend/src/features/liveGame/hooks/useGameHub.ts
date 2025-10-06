import useSignalREmitter from "@/features/signalR/hooks/useSignalREmitter";
import useSignalREvent from "@/features/signalR/hooks/useSignalREvent";
import {
    Clocks,
    DrawState,
    GameColor,
    GameResultData,
    MoveSnapshot,
} from "@/lib/apiClient";
import constants from "@/lib/constants";
import { useMemo } from "react";

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

    DrawStateChangeAsync: [drawState: DrawState];

    ChatMessageAsync: [senderUsername: string, message: string];
    ChatMessageDeliveredAsync: [cooldownLeftMs: number];
    ChatConnectedAsync: [];
};

type GameHubEvents = {
    MovePieceAsync: [gameToken: string, key: string];

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
