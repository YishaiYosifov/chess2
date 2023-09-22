export interface Dictionary<T> {
    [Key: string]: T;
}

export interface PublicProfile {
    userId?: number;
    username?: string;

    about?: string;
    country?: string;
    pfpLastChanged?: number;
}

export const enum Variants {
    Anarchy = "anarchy",
    FogOfWar = "fogofwar",
    Chss = "chss",
}

export interface Player {
    username: string;
    color: "white" | "black";
    score: number;
}

export interface Game {
    token: string;
    white: Player;
    black: Player;

    createdAt: string;

    variant: Variants;
    increment: number;
    timeControl: number;
}
