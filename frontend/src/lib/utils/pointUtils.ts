import { Point, StrPoint } from "@/types/tempModels";

export function stringToPoint(point: StrPoint): Point {
    return point.split(",").map((x) => Number(x)) as Point;
}

export function pointToString(point: Point): StrPoint {
    return point.toString() as StrPoint;
}
