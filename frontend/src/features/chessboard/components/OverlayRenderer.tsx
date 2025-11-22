import React, { JSX, useId, useRef } from "react";

import { useChessboardStore } from "../hooks/useChessboard";
import { ViewPoint } from "@/features/point/types";
import { Point } from "@/features/point/types";
import { OverlayItem } from "../stores/overlaySlice";
import { pointEquals } from "@/features/point/pointUtils";
import useBoardInteraction from "../hooks/useBoardInteraction";

const COLOR = "#5e3b59";
const OPACITY = 0.7;
const STROKE_DRAWING_REDUCTION = 20;

const ARROW_STROKE_WIDTH = 0.2;

const CIRCLE_STROKE_WIDTH = 0.08;
const CIRCLE_PADDING = 0.05;

const OverlayRenderer = () => {
    const {
        commitCurrentlyDrawing,
        setCurrentlyDrawing,
        screenToViewPoint,
        clearOverlays,
    } = useChessboardStore((x) => ({
        commitCurrentlyDrawing: x.commitCurrentlyDrawing,
        setCurrentlyDrawing: x.setCurrentlyDrawing,
        clearOverlays: x.clearOverlays,
        screenToViewPoint: x.screenToViewPoint,
    }));
    const currentlyDrawing = useChessboardStore((x) => x.currentlyDrawing);
    const dimensions = useChessboardStore((x) => x.boardDimensions);
    const overlays = useChessboardStore((x) => x.overlays);
    const headId = useId();

    const startPointRef = useRef<ViewPoint | null>(null);
    const lastPointRef = useRef<ViewPoint | null>(null);
    useBoardInteraction({
        shouldStartDrag(info) {
            if (info.button === 2) return true;

            clearOverlays();
            return false;
        },

        onDragStart(point) {
            const viewPoint = screenToViewPoint(point);
            if (!viewPoint) return;

            setCurrentlyDrawing(viewPoint, viewPoint);
            lastPointRef.current = viewPoint;
            startPointRef.current = viewPoint;
        },
        onDragMove(point) {
            const viewPoint = screenToViewPoint(point);
            if (
                !viewPoint ||
                !startPointRef.current ||
                !lastPointRef.current ||
                pointEquals(viewPoint, lastPointRef.current)
            )
                return;

            lastPointRef.current = viewPoint;
            setCurrentlyDrawing(startPointRef.current, viewPoint);
        },
        onDragEnd: () => commitCurrentlyDrawing(),
    });

    function getAdjustedStrokeWidth(base: number, isDrawing: boolean): number {
        return isDrawing ? base * (1 - STROKE_DRAWING_REDUCTION / 100) : base;
    }

    function drawOverlay({
        item,
        isDrawing,
        key,
    }: {
        item: OverlayItem;
        isDrawing: boolean;
        key?: number;
    }): JSX.Element {
        const { from, to } = item;
        const color = item.color ?? COLOR;

        if (pointEquals(from, to))
            return (
                <CircleRenderer
                    position={from}
                    color={color}
                    strokeWidth={getAdjustedStrokeWidth(
                        CIRCLE_STROKE_WIDTH,
                        isDrawing,
                    )}
                    opacity={OPACITY}
                    padding={CIRCLE_PADDING}
                    key={key}
                />
            );
        else
            return (
                <LineRenderer
                    color={color}
                    strokeWidth={getAdjustedStrokeWidth(
                        ARROW_STROKE_WIDTH,
                        isDrawing,
                    )}
                    opacity={OPACITY}
                    headId={headId}
                    from={from}
                    to={to}
                    key={key}
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
                    <path d="M0,0 V3.5 L2.8,1.75 Z" fill="context-stroke" />
                </marker>
            </defs>

            <g>
                {[...overlays.values()].map((item, i) =>
                    drawOverlay({ item, isDrawing: false, key: i }),
                )}
                {currentlyDrawing &&
                    drawOverlay({
                        item: currentlyDrawing,
                        isDrawing: true,
                    })}
            </g>
        </svg>
    );
};
export default OverlayRenderer;

const CircleRenderer = ({
    position,
    color,
    strokeWidth,
    opacity,
    padding,
}: {
    position: ViewPoint;
    color: string;
    strokeWidth: number;
    opacity: number;
    padding: number;
}) => {
    const r = 0.5 - strokeWidth / 2 - padding;

    return (
        <circle
            cx={position.x + 0.5}
            cy={position.y + 0.5}
            r={r}
            fill="none"
            stroke={color}
            opacity={opacity}
            strokeWidth={strokeWidth}
        />
    );
};

const LineRenderer = ({
    color,
    strokeWidth,
    opacity,
    headId,
    from,
    to,
}: {
    color: string;
    strokeWidth: number;
    opacity: number;
    headId: string;
    from: ViewPoint;
    to: ViewPoint;
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

    const centeredFrom = centerPoint(from);
    const centeredTo = centerPoint(to);
    const { x1, y1, x2, y2 } = shortenLine(centeredFrom, centeredTo);

    return (
        <line
            stroke={color}
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
};
