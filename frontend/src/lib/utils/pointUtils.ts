import {
    LogicalPoint,
    Point,
    ScreenPoint,
    StrPoint,
    ViewPoint,
} from "@/types/tempModels";

export function pointToStr(point: Point): StrPoint {
    return `${point.x},${point.y}`;
}

export function idxToLogicalPoint(
    index: number,
    boardWidth: number,
): LogicalPoint {
    return logicalPoint({
        x: index % boardWidth,
        y: Math.floor(index / boardWidth),
    });
}

export function logicalPoint(point: Point): LogicalPoint {
    return point as LogicalPoint;
}

export function viewPoint(point: Point): ViewPoint {
    return point as ViewPoint;
}

export function screenPoint(point: Point): ScreenPoint {
    return point as ScreenPoint;
}

export function pointEquals(a: Point, b: Point): boolean {
    return a.x === b.x && a.y === b.y;
}
