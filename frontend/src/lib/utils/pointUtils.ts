import { Point, StrPoint } from "@/types/tempModels";

export function stringToPoint(point: StrPoint): Point {
    const [x, y] = point.split(",");
    return { x: Number.parseInt(x), y: Number.parseInt(y) };
}

export function pointToString(point: Point): StrPoint {
    return `${point.x},${point.y}`;
}
