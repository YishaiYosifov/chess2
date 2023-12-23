/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { GameResults } from '../models/GameResults';
import type { Rating } from '../models/Rating';
import type { RatingOverview } from '../models/RatingOverview';
import type { UserOut } from '../models/UserOut';
import type { UserOutSensitive } from '../models/UserOutSensitive';
import type { Variant } from '../models/Variant';

import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';

export class ProfileService {

    /**
     * Get Info
     * Fetch a user's profile
     * @returns UserOut Successful Response
     * @throws ApiError
     */
    public static getInfo({
target,
}: {
target: string,
}): CancelablePromise<UserOut> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/profile/{target}/info',
            path: {
                'target': target,
            },
            errors: {
                401: `Could not verify credentials`,
                404: `Target user not found`,
                422: `Validation Error`,
            },
        });
    }

    /**
     * Get Info Sensitive
     * Fetch the sensitive profile of user
     * @returns UserOutSensitive Successful Response
     * @throws ApiError
     */
    public static getInfoSensitive(): CancelablePromise<UserOutSensitive> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/profile/me/info-sensitive',
            errors: {
                401: `Could not verify credentials`,
            },
        });
    }

    /**
     * Paginate Games
     * Paginate through game history for a specified target.
 * Retrieve a paginated list of game results.
     * @returns GameResults Successful Response
     * @throws ApiError
     */
    public static paginateGames({
target,
page,
perPage = 10,
}: {
target: string,
page?: number,
perPage?: number,
}): CancelablePromise<Array<GameResults>> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/profile/{target}/games',
            path: {
                'target': target,
            },
            query: {
                'page': page,
                'per-page': perPage,
            },
            errors: {
                401: `Could not verify credentials`,
                404: `Target user not found`,
                422: `Validation Error`,
            },
        });
    }

    /**
     * Total Game Count
     * Count how many games a user has
     * @returns number Successful Response
     * @throws ApiError
     */
    public static totalGameCount({
target,
}: {
target: string,
}): CancelablePromise<number> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/profile/{target}/total-game-count',
            path: {
                'target': target,
            },
            errors: {
                401: `Could not verify credentials`,
                404: `Target user not found`,
                422: `Validation Error`,
            },
        });
    }

    /**
     * Get Ratings
     * Get the current ratings of a user.
 * If a user is unrated in a certain variant, that variant will not be returned.
     * @returns Rating Successful Response
     * @throws ApiError
     */
    public static getRatings({
target,
variants,
}: {
target: string,
variants?: Array<Variant>,
}): CancelablePromise<Record<string, Rating>> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/profile/{target}/ratings',
            path: {
                'target': target,
            },
            query: {
                'variants': variants,
            },
            errors: {
                401: `Could not verify credentials`,
                404: `Target user not found`,
                422: `Validation Error`,
            },
        });
    }

    /**
     * Get Ratings History
     * Get the rating history of a user.
 * If a user is unrated in a certain variant, that variant will not be returned.
     * @returns any Successful Response
     * @throws ApiError
     */
    public static getRatingsHistory({
target,
since,
variants,
}: {
target: string,
since: string,
variants?: Array<Variant>,
}): CancelablePromise<Record<string, (RatingOverview | null)>> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/profile/{target}/rating-history',
            path: {
                'target': target,
            },
            query: {
                'since': since,
                'variants': variants,
            },
            errors: {
                400: `Bad 'since' value`,
                401: `Could not verify credentials`,
                404: `Target user not found`,
                422: `Validation Error`,
            },
        });
    }

    /**
     * Profile Picture
     * Get a user's profile picture.
 * If the user hasn't uploaded a picture yet, the default one will be returned.
     * @returns any Successful Response
     * @throws ApiError
     */
    public static profilePicture({
target,
}: {
target: string,
}): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/profile/{target}/profile-picture',
            path: {
                'target': target,
            },
            errors: {
                401: `Could not verify credentials`,
                422: `Validation Error`,
            },
        });
    }

}
