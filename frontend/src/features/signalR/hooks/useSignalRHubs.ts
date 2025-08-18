import { useMemo } from "react";
import constants from "@/lib/constants";
import useSignalREvent, { signalREventHookFactory } from "./useSignalREvent";
import useSignalREmitter, {
    signalREmitterHookFactory,
} from "./useSignalREmitter";
import { MoveKey } from "@/features/chessboard/lib/types";
import {
    Clocks,
    DrawState,
    GameColor,
    GameResultData,
    MoveSnapshot,
    PoolKey,
    TimeControlSettings,
} from "@/lib/apiClient";
import { OpenSeek } from "@/features/lobby/lib/types";

export type LobbyClientEvents = {
    MatchFoundAsync: [token: string];
    SeekFailedAsync: [pool: PoolKey];
};

type LobbyHubEvents = {
    SeekRatedAsync: [timeControl: TimeControlSettings];
    SeekCasualAsync: [timeControl: TimeControlSettings];
    CancelSeekAsync: [pool: PoolKey];
    CleanupConnectionAsync: [];

    MatchWithOpenSeekAsync: [userId: string, pool: PoolKey];
};

export const useLobbyEvent = signalREventHookFactory<LobbyClientEvents>(
    constants.SIGNALR_PATHS.LOBBY,
);

export const useLobbyEmitter = signalREmitterHookFactory<LobbyHubEvents>(
    constants.SIGNALR_PATHS.LOBBY,
);

export type OpenSeekClientEvents = {
    NewOpenSeeksAsync: [seeks: OpenSeek[]];
    OpenSeekEndedAsync: [userId: string, pool: PoolKey];
};

type OpenSeekHubEvents = {
    SubscribeAsync: [];
};

export const useOpenSeekEvent = signalREventHookFactory<OpenSeekClientEvents>(
    constants.SIGNALR_PATHS.OPENSEEK,
);
export const useOpenSeekEmitter = signalREmitterHookFactory<OpenSeekHubEvents>(
    constants.SIGNALR_PATHS.OPENSEEK,
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

    DrawStateChangeAsync: [drawState: DrawState];

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
