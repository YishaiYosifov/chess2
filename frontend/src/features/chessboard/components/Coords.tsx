import clsx from "clsx";

import { viewPoint } from "@/features/point/pointUtils";
import CoordSquare from "./CoordSquare";
import { useChessboardStore } from "../hooks/useChessboard";
import { GameColor } from "@/lib/apiClient";

const Coords = () => {
    const viewingFrom = useChessboardStore((x) => x.viewingFrom);
    const boardDimensions = useChessboardStore((x) => x.boardDimensions);

    const files = Array.from({ length: boardDimensions.height }, (_, i) =>
        String.fromCharCode("a".charCodeAt(0) + i),
    );
    const ranks = Array.from(
        { length: boardDimensions.height },
        (_, i) => boardDimensions.height - i,
    );

    return (
        <>
            {files.map((file, x) => {
                const viewerX = viewingFrom == GameColor.WHITE ? x : 9 - x;
                return (
                    <CoordSquare
                        key={file}
                        data-testid={`coordsFile-${file}`}
                        position={viewPoint({
                            x: viewerX,
                            y: 9,
                        })}
                        className={clsx(
                            "flex items-end px-1 select-none",
                            viewerX % 2 === 0
                                ? "text-[#e9e9d4]"
                                : "text-[#577298]",
                        )}
                    >
                        {file}
                    </CoordSquare>
                );
            })}

            {ranks.map((rank, y) => {
                const viewerY = viewingFrom == GameColor.WHITE ? y : 9 - y;
                return (
                    <CoordSquare
                        key={rank}
                        data-testid={`coordsRank-${rank}`}
                        position={viewPoint({ x: 9, y: viewerY })}
                        className={clsx(
                            "flex justify-end px-1 select-none",
                            viewerY % 2 === 0
                                ? "text-[#e9e9d4]"
                                : "text-[#577298]",
                        )}
                    >
                        {rank}
                    </CoordSquare>
                );
            })}
        </>
    );
};

export default Coords;
