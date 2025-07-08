import Card from "@/components/ui/Card";
import { useLiveChessStore } from "../hooks/useLiveChessStore";
import React from "react";

const MoveHistoryTable = () => {
    const moveHistory = useLiveChessStore((x) => x.moveHistory);
    const moveRows: React.ReactElement[] = [];

    let rowIndex = 0;
    for (let i = 0; i < moveHistory.length; i += 2) {
        const currentMove = moveHistory[i];
        const nextMove = moveHistory[i + 1];

        moveRows.push(
            <MoveRow
                key={i}
                index={rowIndex}
                moveWhite={currentMove.san}
                moveBlack={nextMove?.san ?? ""}
            />,
        );

        rowIndex++;
    }

    return (
        <Card className="block max-h-96 overflow-x-auto p-0">
            <table className="min-w-full overflow-hidden text-center text-sm text-white">
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
    const color = index % 2 === 0 ? "bg-black/15" : "";
    return (
        <tr className={color}>
            <td className="w-1/2 py-3">{moveWhite}</td>
            <td className="w-1/2 py-3">{moveBlack}</td>
        </tr>
    );
};
