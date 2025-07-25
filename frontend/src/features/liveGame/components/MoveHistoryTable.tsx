import React, { useEffect, useMemo } from "react";

import { useLiveChessStore } from "../hooks/useLiveChessStore";
import Card from "@/components/ui/Card";
import { useChessboardStore } from "@/features/chessboard/hooks/useChessboard";
import { BoardState } from "@/types/tempModels";

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

            if (newPosition) setPosition(newPosition);
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
    const color = index % 2 === 0 ? "bg-white/10" : "";
    return (
        <tr className={color}>
            <td className="w-10 bg-zinc-900 p-3">{index}.</td>
            <td className="p-3">{moveWhite}</td>
            <td className="p-3"> {moveBlack}</td>
        </tr>
    );
};
