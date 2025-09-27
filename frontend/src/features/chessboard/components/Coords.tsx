import clsx from "clsx";

import { logicalPoint } from "@/features/point/pointUtils";
import ChessSquare from "./ChessSquare";

const Coords = () => {
    const files = ["a", "b", "c", "d", "e", "f", "g", "h", "i", "j"];
    const ranks = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10];

    return (
        <>
            {files.map((file, x) => (
                <ChessSquare
                    key={file}
                    data-testid={`coordsFile-${file}`}
                    position={logicalPoint({ x, y: 0 })}
                    className={clsx(
                        "flex items-end px-1 select-none",
                        x % 2 === 0 ? "text-[#b58863]" : "text-[#f0d9b5]",
                    )}
                >
                    {file}
                </ChessSquare>
            ))}

            {ranks.map((rank, y) => (
                <ChessSquare
                    key={rank}
                    data-testid={`coordsRank-${rank}`}
                    position={logicalPoint({ x: 9, y })}
                    className={clsx(
                        "flex justify-end px-1 select-none",
                        y % 2 === 0 ? "text-[#f0d9b5]" : "text-[#b58863]",
                    )}
                >
                    {rank}
                </ChessSquare>
            ))}
        </>
    );
};

export default Coords;
