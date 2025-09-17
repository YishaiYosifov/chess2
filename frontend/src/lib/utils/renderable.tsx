import { MaybePromise } from "@/types/types";
import { JSX } from "react";

type RenderableTypes = JSX.Element | JSX.Element[] | string;

export type Renderable<T = void> =
    | ((props: T) => MaybePromise<RenderableTypes>)
    | RenderableTypes;

export function renderRenderable<T>(
    children: Renderable<T>,
    props: T,
): MaybePromise<RenderableTypes> {
    return typeof children === "function" ? children(props) : children;
}
