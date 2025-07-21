import { Point, StrPoint } from "@/types/tempModels";

export function pointToStr(point: Point): StrPoint {
    return `${point.x},${point.y}`;
}

export function idxToPoint(index: number, boardWidth: number): Point {
    return {
        x: index % boardWidth,
        y: Math.floor(index / boardWidth),
    };
}

export function pointEquals(a: Point, b: Point): boolean {
    return a.x === b.x && a.y === b.y;
}
