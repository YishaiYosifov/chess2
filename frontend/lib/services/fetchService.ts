import type { Game, PublicProfile, RatingsMap } from "../types";

import { apiRequest, snakeToCamel } from "../utils/common";

/**
 * Fetch a user's public profile by their username
 *
 * @param username - the username of the user to fetch
 * @returns a promise that resolves to the users profile or null if the user was not found
 */
export async function fetchProfile(
    username: string
): Promise<PublicProfile | null> {
    const profileRequest = await apiRequest(`/profile/${username}/info`, {
        method: "GET",
        next: { revalidate: 60 },
    });

    if (profileRequest.status == 404) return null;
    return (snakeToCamel(await profileRequest.json()) || {}) as PublicProfile;
}

/**
 * Fetch a user's ratings since a specified date
 *
 * @param username - the username of the user whose ratings to fetch
 * @param since - the date since which to fetch ratings (in ISO date format)
 * @returns a promise that resolves to a mapping of game variants to rating data or null if the user was not found.
 */
export async function fetchRatings(
    username: string,
    since: Date
): Promise<RatingsMap | null> {
    const ratingsRequest = await apiRequest(
        `/profile/${username}/rating-history?since=${
            since.toISOString().split("T")[0]
        }`,
        {
            method: "GET",
            next: { revalidate: 60 },
        }
    );

    if (ratingsRequest.status == 404) return null;
    return (snakeToCamel(await ratingsRequest.json()) || {}) as RatingsMap;
}

/**
 * Fetch a user's games
 *
 * @param username - the username of the user whose games to fetch
 * @returns a promise that resolves to an array of game objects or null if the user was not found
 */
export async function fetchGames(username: string): Promise<Game[] | null> {
    const gamesRequest = await apiRequest(`/profile/${username}/games`, {
        method: "GET",
        next: { revalidate: 60 },
    });
    if (gamesRequest.status == 404) return null;

    return (snakeToCamel(await gamesRequest.json()) || []) as Game[];
}
