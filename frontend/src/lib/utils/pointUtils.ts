import { Point, StrPoint } from "@/types/tempModels";

export function strToPoint(point: StrPoint): Point {
    const [x, y] = point.split(",");
    return { x: Number.parseInt(x), y: Number.parseInt(y) };
}

export function pointToStr(point: Point): StrPoint {
    return `${point.x},${point.y}`;
}
