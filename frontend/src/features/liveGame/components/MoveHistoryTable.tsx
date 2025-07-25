import React, { useEffect, useMemo } from "react";

import { useLiveChessStore } from "../hooks/useLiveChessStore";
import Card from "@/components/ui/Card";
import { useChessboardStore } from "@/features/chessboard/hooks/useChessboard";
import { BoardState } from "@/types/tempModels";
import clsx from "clsx";

const MoveHistoryTable = () => {
    const positionHistory = useLiveChessStore((x) => x.positionHistory);
    const shiftMoveViewBy = useLiveChessStore((x) => x.shiftMoveViewBy);
    const teleportToMove = useLiveChessStore((x) => x.teleportToMove);
    const teleportToLastMove = useLiveChessStore((x) => x.teleportToLastMove);
    const setPosition = useChessboardStore((x) => x.setPosition);

    const moveRows: React.ReactElement[] = useMemo(() => {
        let rowIndex = 1;
        const moveRows: React.ReactElement[] = [];
        for (let i = 1; i < positionHistory.length; i += 2) {
            const currentMove = positionHistory[i].san;
            const nextMove = positionHistory[i + 1]?.san;

            moveRows.push(
                <MoveRow
                    key={i}
                    index={rowIndex}
                    moveWhite={currentMove}
                    moveBlack={nextMove}
                />,
            );

            rowIndex++;
        }
        return moveRows;
    }, [positionHistory]);

    useEffect(() => {
        function onKeyDown(event: KeyboardEvent): void {
            let newPosition: BoardState | undefined;
            if (event.key === "ArrowLeft") newPosition = shiftMoveViewBy(-1);
            else if (event.key === "ArrowRight")
                newPosition = shiftMoveViewBy(1);
            else if (event.key === "ArrowUp") newPosition = teleportToMove(0);
            else if (event.key === "ArrowDown")
                newPosition = teleportToLastMove();

            setPosition(newPosition);
        }

        window.addEventListener("keydown", onKeyDown);
        return () => window.removeEventListener("keydown", onKeyDown);
    }, [shiftMoveViewBy, setPosition, teleportToMove, teleportToLastMove]);

    return (
        <Card className="block max-h-96 overflow-x-auto p-0 lg:max-h-full">
            <table className="w-full table-fixed">
                <tbody>{moveRows}</tbody>
            </table>
        </Card>
    );
};
export default MoveHistoryTable;

const MoveRow = ({
    moveWhite,
    moveBlack,
    index,
}: {
    moveWhite?: string;
    moveBlack?: string;
    index: number;
}) => {
    const teleportToMove = useLiveChessStore((x) => x.teleportToMove);
    const setPosition = useChessboardStore((x) => x.setPosition);

    const whiteMoveIdx = index * 2 - 1;
    const blackMoveIdx = whiteMoveIdx + 1;

    const isViewingWhite = useLiveChessStore(
        (x) => x.viewingMoveNumber === whiteMoveIdx,
    );
    const isViewingBlack = useLiveChessStore(
        (x) => x.viewingMoveNumber === blackMoveIdx,
    );

    const color = index % 2 === 0 ? "bg-white/10" : "";
    const selectedClass = "bg-blue-300/30";
    return (
        <tr className={color}>
            <td className="w-10 bg-zinc-900 p-3">{index}.</td>
            <td
                className={clsx(
                    "cursor-pointer p-3",
                    isViewingWhite && selectedClass,
                )}
                onClick={() => setPosition(teleportToMove(whiteMoveIdx))}
            >
                {moveWhite}
            </td>
            <td
                className={clsx(
                    "cursor-pointer p-3",
                    isViewingBlack && selectedClass,
                )}
                onClick={() => setPosition(teleportToMove(blackMoveIdx))}
            >
                {moveBlack}
            </td>
        </tr>
    );
};
