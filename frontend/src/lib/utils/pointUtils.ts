import { Point, StrPoint } from "@/types/tempModels";

export function pointToStr(point: Point): StrPoint {
    return `${point.x},${point.y}`;
}
