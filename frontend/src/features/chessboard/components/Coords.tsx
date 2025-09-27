import clsx from "clsx";

import { viewPoint } from "@/features/point/pointUtils";
import CoordSquare from "./CoordSquare";

const Coords = () => {
    const files = ["a", "b", "c", "d", "e", "f", "g", "h", "i", "j"];
    const ranks = [10, 9, 8, 7, 6, 5, 4, 3, 2, 1];

    return (
        <>
            {files.map((file, x) => (
                <CoordSquare
                    key={file}
                    data-testid={`coordsFile-${file}`}
                    position={viewPoint({ x, y: 9 })}
                    className={clsx(
                        "flex items-end px-1 select-none",
                        x % 2 === 0 ? "text-[#b58863]" : "text-[#f0d9b5]",
                    )}
                >
                    {file}
                </CoordSquare>
            ))}

            {ranks.map((rank, y) => (
                <CoordSquare
                    key={rank}
                    data-testid={`coordsRank-${rank}`}
                    position={viewPoint({ x: 9, y })}
                    className={clsx(
                        "flex justify-end px-1 select-none",
                        y % 2 === 0 ? "text-[#b58863]" : "text-[#f0d9b5]",
                    )}
                >
                    {rank}
                </CoordSquare>
            ))}
        </>
    );
};

export default Coords;
