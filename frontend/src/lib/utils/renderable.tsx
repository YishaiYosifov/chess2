import { MaybePromise } from "@/types/types";
import { JSX } from "react";

export type Renderable<T = void> =
    | ((props: T) => MaybePromise<JSX.Element | JSX.Element[]>)
    | JSX.Element
    | JSX.Element[];

export function renderRenderable<T>(
    children: Renderable<T>,
    props: T,
): MaybePromise<JSX.Element | JSX.Element[]> {
    return typeof children === "function" ? children(props) : children;
}
