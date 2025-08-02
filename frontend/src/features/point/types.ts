import { Brand } from "@/types/types";

export interface Point {
    x: number;
    y: number;
}

export declare const logicalPointBrand: unique symbol;
export declare const viewPointBrand: unique symbol;
export declare const screenPointBrand: unique symbol;

export type ScreenPoint = Brand<Point, typeof screenPointBrand>;
export type LogicalPoint = Brand<Point, typeof logicalPointBrand>;
export type ViewPoint = Brand<Point, typeof viewPointBrand>;

export type StrPoint = `${number},${number}`;
