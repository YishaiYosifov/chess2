export interface Rating {
    elo: number;
    achievedAt: Date;
}

export interface RatingOverview {
    min: number;
    max: number;

    current: number;
    history: Array<Rating>;
}
