import React, { useEffect, useMemo, useRef } from "react";

import useLiveChessStore from "../hooks/useLiveChessStore";
import Card from "@/components/ui/Card";
import { useChessboardStore } from "@/features/chessboard/hooks/useChessboard";
import clsx from "clsx";
import useAutoScroll from "@/hooks/useAutoScroll";
import { HistoryStep } from "../lib/types";

const MoveHistoryTable = () => {
    const { shiftMoveViewBy, teleportToMove, teleportToLastMove } =
        useLiveChessStore((x) => ({
            shiftMoveViewBy: x.shiftMoveViewBy,
            teleportToMove: x.teleportToMove,
            teleportToLastMove: x.teleportToLastMove,
        }));
    const positionHistory = useLiveChessStore((x) => x.positionHistory);
    const goToPosition = useChessboardStore((x) => x.goToPosition);

    const tableRef = useRef<HTMLDivElement | null>(null);
    useAutoScroll(tableRef, [positionHistory]);

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
        async function onKeyDown(event: KeyboardEvent): Promise<void> {
            let historyStep: HistoryStep | undefined;
            switch (event.key) {
                case "ArrowLeft":
                    historyStep = shiftMoveViewBy(-1);
                    break;
                case "ArrowRight":
                    historyStep = shiftMoveViewBy(1);
                    break;
                case "ArrowUp":
                    historyStep = teleportToMove(0);
                    break;
                case "ArrowDown":
                    historyStep = teleportToLastMove();
                    break;
            }
            if (!historyStep) return;

            await goToPosition(historyStep.state, {
                animateIntermediates:
                    historyStep.isOneStepForward && !event.repeat,
            });
        }

        window.addEventListener("keydown", onKeyDown);
        return () => window.removeEventListener("keydown", onKeyDown);
    }, [goToPosition, shiftMoveViewBy, teleportToMove, teleportToLastMove]);

    return (
        <Card
            className="block max-h-96 overflow-x-auto p-0 lg:max-h-full"
            ref={tableRef}
        >
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
    const whiteMoveIdx = index * 2 - 1;
    const blackMoveIdx = whiteMoveIdx + 1;

    const { teleportToMove, isViewingWhite, isViewingBlack } =
        useLiveChessStore((x) => ({
            teleportToMove: x.teleportToMove,
            isViewingWhite: x.viewingMoveNumber === whiteMoveIdx,
            isViewingBlack: x.viewingMoveNumber === blackMoveIdx,
        }));

    const goToPosition = useChessboardStore((x) => x.goToPosition);

    async function handleClick(moveIdx: number): Promise<void> {
        const historyStep = teleportToMove(moveIdx);
        if (!historyStep) return;

        await goToPosition(historyStep.state, {
            animateIntermediates: historyStep.isOneStepForward,
        });
    }

    const color = index % 2 === 0 ? "bg-white/10" : "";
    const selectedClass = "bg-blue-300/30";
    return (
        <tr className={color}>
            <td className="w-10 bg-zinc-900 p-3">{index}.</td>
            <td
                className={clsx(
                    "cursor-pointer overflow-x-auto p-3",
                    isViewingWhite && selectedClass,
                )}
                onClick={() => handleClick(whiteMoveIdx)}
            >
                <div className="overflow-x-auto">{moveWhite}</div>
            </td>
            <td
                className={clsx(
                    "cursor-pointer overflow-x-auto p-3",
                    isViewingBlack && selectedClass,
                )}
                onClick={() => handleClick(blackMoveIdx)}
            >
                <div className="overflow-x-auto">{moveBlack}</div>
            </td>
        </tr>
    );
};
