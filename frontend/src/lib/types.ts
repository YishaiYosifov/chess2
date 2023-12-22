import { Color, GameResult, Variant } from "./constants";

export interface Player {
    userId: number;
    username: string;
    color: Color;
}

export interface Game {
    token: string;
    userWhite?: SimplePublicProfile;
    userBlack?: SimplePublicProfile;
    results: GameResult;

    createdAt: string;

    variant: Variant;
    timeControl: number;
    increment: number;
}

export interface RatingHistory {
    achievedAt: string;
    elo: number;
}
export interface RatingData {
    history: RatingHistory[];
    current: number;
    min: number;
    max: number;
}

export type RatingsMap = { [key in Variant]: RatingData };

export interface SimplePublicProfile {
    userId: number;
    username: string;
}

export interface PublicProfile extends SimplePublicProfile {
    about: string;
    country: string;
    pfpLastChanged: string;
}

export interface LocalProfile extends Partial<PublicProfile> {
    email?: string;
    usernameLastChanged?: string;
}
