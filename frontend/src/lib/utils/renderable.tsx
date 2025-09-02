import { MaybePromise } from "@/types/types";
import { JSX } from "react";

export type Renderable<T = void> =
    | ((props: T) => MaybePromise<JSX.Element>)
    | JSX.Element;

export async function renderRenderable<T>(
    children: Renderable<T>,
    props: T,
): Promise<JSX.Element> {
    return typeof children === "function" ? children(props) : children;
}
