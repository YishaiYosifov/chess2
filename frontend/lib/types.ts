export interface Dictionary<T> {
    [Key: string]: T;
}

export const enum Variants {
    Anarchy = "anarchy",
    FogOfWar = "fogofwar",
    Chss = "chss",
}

export interface Player {
    userId: number;
    username: string;
    color: "white" | "black";
}

export interface Game {
    token: string;
    white: Player;
    black: Player;

    winnerId: number;

    createdAt: string;

    variant: Variants;
    increment: number;
    timeControl: number;
}

export interface RatingHistory {
    achievedAt: number;
    elo: number;
}
export interface RatingData {
    history: RatingHistory[];
    current: number;
    minRating: number;
    maxRating: number;
}

export type RatingsMap = { [key in Variants]: RatingData };

export interface PublicProfile {
    userId: number;
    username: string;

    about: string;
    country: string;
    pfpLastChanged: string;
}
