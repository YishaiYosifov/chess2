import { Point } from "@/types/tempModels";
import React, { useId } from "react";
import { useChessboardStore } from "../hooks/useChessboard";

const ArrowRenderer = ({
    strokeColor = "#15781B",
    strokeWidth = 0.2,
    opacity = 0.8,
}: {
    strokeColor?: string;
    strokeWidth?: number;
    opacity?: number;
}) => {
    const dimensions = useChessboardStore((x) => x.boardDimensions);
    const arrows = useChessboardStore((x) => x.arrows);
    const headId = useId();

    const calculateArrowCoordinates = (
        point1: Point,
        point2: Point,
    ): { x1: number; y1: number; x2: number; y2: number } => ({
        x1: point1.x + 0.5,
        y1: point1.y + 0.5,
        x2: point2.x + 0.5,
        y2: point2.y + 0.5,
    });

    return (
        <svg
            className="pointer-events-none absolute inset-0 z-30"
            width="100%"
            height="100%"
            viewBox={`0 0 ${dimensions.width} ${dimensions.height}`}
            preserveAspectRatio="xMidYMid meet"
        >
            <defs>
                <marker
                    id={headId}
                    orient="auto-start-reverse"
                    overflow="visible"
                    refX="1.4"
                    refY="1.75"
                    markerUnits="strokeWidth"
                >
                    <path d="M0,0 V3.5 L2.8,1.75 Z" fill={strokeColor} />
                </marker>
            </defs>

            <g>
                {arrows.map(({ from, to }, i) => {
                    const { x1, y1, x2, y2 } = calculateArrowCoordinates(
                        from,
                        to,
                    );

                    return (
                        <line
                            key={i}
                            stroke={strokeColor}
                            strokeWidth={strokeWidth}
                            strokeLinecap="round"
                            markerEnd={`url(#${headId})`}
                            opacity={opacity}
                            x1={x1}
                            y1={y1}
                            x2={x2}
                            y2={y2}
                        />
                    );
                })}
            </g>
        </svg>
    );
};
export default ArrowRenderer;
