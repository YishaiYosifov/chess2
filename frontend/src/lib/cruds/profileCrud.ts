import type { Game, PublicProfile, RatingsMap } from "../types";

import { getResource } from "../utils/fetchUtils";

/**
 * Fetch a user's public profile by their username
 *
 * @param username - the username of the user to fetch
 * @returns a promise that resolves to the users profile or null if the user was not found
 */
export async function fetchProfile(
    username: string
): Promise<PublicProfile | null> {
    return await getResource(`/profile/${username}/info`);
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
    return await getResource(
        `/profile/${username}/rating-history?since=${
            since.toISOString().split("T")[0]
        }`
    );
}

/**
 * Fetch a user's games
 *
 * @param username - the username of the user whose games to fetch
 * @returns a promise that resolves to an array of game objects or null if the user was not found
 */
export async function fetchGames(username: string): Promise<Game[] | null> {
    return await getResource(`/profile/${username}/games`);
}
