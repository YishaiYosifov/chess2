export interface Rating {
    elo: number;
    achievedAt: number;
}

export interface RatingOverview {
    max: number;
    current: number;
    history: Array<Rating>;
}
