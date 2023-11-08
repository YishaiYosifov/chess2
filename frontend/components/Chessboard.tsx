"use client";

import { useEffect, useState } from "react";

import { Piece, PieceData, Variant, defaultChessboard } from "@/lib/constants";
import styles from "./Chessboard.module.scss";

interface Breakpoint {
    breakpoint: number;
    offset: {
        width: number;
        height: number;
    };
}

/**
 * Display a chessboard
 *
 * @param variant - the variant of the game
 * @param offsetBreakpoints - in ascending order, the offset for each width of the screen.
 * The last width will be used for any screen size larger than it.
 * @returns
 */
const Chessboard = ({
    variant,
    offsetBreakpoints = [{ breakpoint: 0, offset: { width: 0, height: 0 } }],
    startingBoard = defaultChessboard,
    fixed = false,
}: {
    variant: Variant;
    offsetBreakpoints?: Breakpoint[];
    startingBoard?: Record<number, PieceData>;
    fixed?: boolean;
}) => {
    const [boardSize, setBoardSize] = useState(0);

    /**
     * Calculate the width and height offset based on the offsetBreakpoints param and window width
     */
    function calculateOffset(): { width: number; height: number } {
        const width = window.innerWidth;
        for (const { breakpoint, offset } of offsetBreakpoints) {
            if (breakpoint > width) return offset;
        }

        return offsetBreakpoints.at(-1)?.offset || { width: 0, height: 0 };
    }

    /**
     * Set the board size based on the viewport size and the offset
     */
    function resizeBoard(): void {
        const { width: offsetWidth, height: offsetHeight } = calculateOffset();
        const width = window.innerWidth - offsetWidth;
        const height = window.innerHeight - offsetHeight;

        const minSize = Math.min(width, height);
        setBoardSize(minSize);
    }

    useEffect(() => {
        // Make sure the board is only resized once the user has stopped resizing the window
        // to prevent too many rerenders
        let timerId: number;
        window.addEventListener("resize", (ev) => {
            if (timerId) clearTimeout(timerId);
            timerId = setTimeout(resizeBoard, 100, ev);
        });

        resizeBoard();
        return () => window.removeEventListener("resize", resizeBoard);
    }, []);

    return (
        <div
            className={styles.chessboard}
            style={{
                width: `${boardSize}px`,
                height: `${boardSize}px`,
            }}
        >
            {Array.from(Array(100).keys()).map((i) => {
                i = 99 - i;
                return (
                    <ChessSquare
                        index={i}
                        pieceData={startingBoard[i]}
                        key={i}
                    />
                );
            })}
        </div>
    );
};

export default Chessboard;

const ChessSquare = ({
    index,
    pieceData,
}: {
    index: number;
    pieceData?: PieceData;
}) => {
    return (
        <div className={styles.square}>
            {pieceData && <ChessPiece pieceData={pieceData} />}
        </div>
    );
};

const ChessPiece = ({ pieceData }: { pieceData: PieceData }) => {
    return (
        <div
            className={styles.piece}
            style={{
                background: `url("/assets/pieces/${pieceData.piece}-${pieceData.color}.png") 100% / 100% no-repeat`,
            }}
        />
    );
};
