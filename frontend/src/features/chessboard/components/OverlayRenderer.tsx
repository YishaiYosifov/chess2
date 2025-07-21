import React, { JSX, useId } from "react";

import { useChessboardStore } from "../hooks/useChessboard";
import { Point } from "@/types/tempModels";
import { OverlayItem } from "../stores/overlaySlice";
import { pointEquals } from "@/lib/utils/pointUtils";

const OverlayRenderer = ({
    strokeColor = "#15781B",
    strokeWidth = 0.2,
    opacity = 0.8,
}: {
    strokeColor?: string;
    strokeWidth?: number;
    opacity?: number;
}) => {
    const dimensions = useChessboardStore((x) => x.boardDimensions);
    const overlays = useChessboardStore((x) => x.overlays);
    const currentlyDrawing = useChessboardStore((x) => x.currentlyDrawing);
    const headId = useId();

    function drawOverlay(item: OverlayItem, key?: number): JSX.Element {
        const { from, to } = item;
        if (pointEquals(from, to)) {
            return (
                <circle
                    key={key}
                    cx={from.x + 0.5}
                    cy={from.y + 0.5}
                    r={strokeWidth / 2}
                    fill={strokeColor}
                />
            );
        }
        return (
            <LineRenderer
                key={key}
                color={strokeColor}
                strokeWidth={strokeWidth}
                opacity={opacity}
                headId={headId}
                shape={item}
            />
        );
    }

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
                {overlays.map((item, i) => drawOverlay(item, i))}
                {currentlyDrawing && drawOverlay(currentlyDrawing)}
            </g>
        </svg>
    );
};
export default OverlayRenderer;

const LineRenderer = ({
    color,
    strokeWidth,
    opacity,
    headId,
    shape,
}: {
    color: string;
    strokeWidth: number;
    opacity: number;
    headId: string;
    shape: OverlayItem;
}) => {
    const centerPoint = ({ x, y }: Point): Point => ({
        x: x + 0.5,
        y: y + 0.5,
    });

    function shortenLine(
        { x: x1, y: y1 }: Point,
        { x: x2, y: y2 }: Point,
        shortenBy = 0.2,
    ): { x1: number; y1: number; x2: number; y2: number } {
        const dx = x2 - x1;
        const dy = y2 - y1;
        const length = Math.sqrt(dx * dx + dy * dy);
        const ratio = (length - shortenBy) / length;

        return {
            x1,
            y1,
            x2: x1 + dx * ratio,
            y2: y1 + dy * ratio,
        };
    }

    const from = centerPoint(shape.from);
    const to = centerPoint(shape.to);
    const { x1, y1, x2, y2 } = shortenLine(from, to);

    return (
        <line
            stroke={color}
            strokeWidth={strokeWidth - 0.02}
            strokeLinecap="round"
            markerEnd={`url(#${headId})`}
            opacity={opacity}
            x1={x1}
            y1={y1}
            x2={x2}
            y2={y2}
        />
    );
};
