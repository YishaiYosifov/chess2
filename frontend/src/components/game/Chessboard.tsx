"use client";

import {
    forwardRef,
    ForwardRefRenderFunction,
    useImperativeHandle,
    useRef,
} from "react";

import constants from "@/lib/constants";
import { LegalMoveMap, Point, type PieceMap } from "@/types/tempModels";

import { GameColor } from "@/lib/apiClient";
import { StoreApi, useStore } from "zustand";
import { ChessStore, createChessStore } from "@/stores/chessStore";
import ChessboardLayout, {
    ChessboardBreakpoint,
    PaddingOffset,
} from "./ChessboardLayout";
import { ChessStoreContext } from "@/contexts/chessStoreContext";
import { decodeLegalMoves } from "@/lib/chessDecoders/moveDecoder";

export interface ChessboardProps {
    breakpoints?: ChessboardBreakpoint[];
    defaultOffset?: PaddingOffset;

    startingPieces?: PieceMap;
    boardWidth?: number;
    boardHeight?: number;
    legalMoves?: LegalMoveMap;

    viewingFrom?: GameColor;
    sideToMove?: GameColor;
    playingAs?: GameColor;

    onPieceMovement?: (from: Point, to: Point) => Promise<void>;

    className?: string;
}

export interface ChessboardRef {
    makeMove: (
        encodedMove: string,
        encodedLegalMoves: string[],
        playerTurn: GameColor,
    ) => void;
}

/**
 * Display a chessboard
 *
 * @param breakpoints - the offset for each dimention of the screen.
 *  for example, if the screen is 1920x1080 and the current breakpoint width offset is 500,
 *  it will parse the width as 1420 before choosing the board size.
 *  The largest width breakpoint will be used for any screen size larger than it.
 * @param sideToMove - the color of the side whose turn it is to play
 * @param playingAs - the color of the player that is controlling the chessboard.
 *  leave undefined if no player should be controlling this chessboard, thus making it a fixed position
 */
const Chessboard: ForwardRefRenderFunction<ChessboardRef, ChessboardProps> = (
    {
        breakpoints = [],
        defaultOffset,

        startingPieces = constants.DEFAULT_CHESS_BOARD,
        boardHeight = constants.BOARD_HEIGHT,
        boardWidth = constants.BOARD_WIDTH,
        legalMoves,

        viewingFrom,
        sideToMove,
        playingAs,

        onPieceMovement,

        className,
    }: ChessboardProps,
    ref,
) => {
    viewingFrom ??= playingAs ?? GameColor.WHITE;

    const storeRef = useRef<StoreApi<ChessStore>>(null);
    if (!storeRef.current)
        storeRef.current = createChessStore({
            pieces: startingPieces,
            legalMoves,
            viewingFrom,
            sideToMove,
            playingAs,
            boardWidth,
            boardHeight,
            onPieceMovement,
        });
    const chessStore = storeRef.current;
    const playMove = useStore(chessStore, (state) => state.playMove);

    useImperativeHandle(ref, () => ({
        makeMove(
            encodedMove: string,
            encodedLegalMoves: string[],
            playerTurn: GameColor,
        ) {
            //const legalMoves = decodeLegalMoves(encodedLegalMoves);
            const move = decodeLegalMoves([encodedMove])
                .entries()
                .toArray()
                .at(0)![1][0];
            playMove(move);
        },
    }));

    return (
        <ChessStoreContext.Provider value={chessStore}>
            <ChessboardLayout
                breakpoints={breakpoints}
                defaultOffset={defaultOffset}
                className={className}
            />
        </ChessStoreContext.Provider>
    );
};

export default forwardRef(Chessboard);
