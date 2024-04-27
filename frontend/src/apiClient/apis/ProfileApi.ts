import type {
    PrivateAuthedProfileOut,
    AuthedProfileOut,
    RatingOverview,
    FinishedGame,
    Rating,
    Variant,
} from "@/models/index";

import {
    BaseAPI,
    ApiResponse,
    BlobApiResponse,
    JSONApiResponse,
} from "../runtime";

export interface GetRatingsHistoryRequest {
    since: Date;
    variants?: Array<Variant>;
}

export interface PaginateGamesRequest {
    page?: number;
    perPage?: number;
}

export class ProfileApi extends BaseAPI {
    /**
     * Fetch a user\'s profile
     * Get Info
     */
    async getInfoRaw(
        target: string,
        initOverrides?: RequestInit
    ): Promise<ApiResponse<AuthedProfileOut>> {
        const response = await this.request(
            {
                path: `/profile/${encodeURIComponent(target)}/info`,
                method: "GET",
            },
            initOverrides
        );

        return new JSONApiResponse(response);
    }
    getInfo = this.createFriendlyRoute(this.getInfoRaw);

    /**
     * Fetch the sensitive profile of user
     * Get Info Sensitive
     */
    async getInfoSensitiveRaw(
        initOverrides?: RequestInit
    ): Promise<ApiResponse<PrivateAuthedProfileOut>> {
        const response = await this.request(
            {
                path: "/profile/me/info-sensitive",
                method: "GET",
            },
            initOverrides
        );

        return new JSONApiResponse(response);
    }
    getInfoSensitive = this.createFriendlyRoute(this.getInfoSensitiveRaw);

    /**
     * Get the current ratings of a user. If a user is unrated in a certain variant, that variant will not be returned.
     * Get Ratings
     */
    async getRatingsRaw(
        target: string,
        variants: Array<Variant>,
        initOverrides?: RequestInit
    ): Promise<ApiResponse<Record<string, Rating>>> {
        const response = await this.request(
            {
                path: `/profile/${encodeURIComponent(target)}/ratings`,
                method: "GET",
                query: variants,
            },
            initOverrides
        );

        return new JSONApiResponse(response);
    }
    getRatings = this.createFriendlyRoute(this.getRatingsRaw);

    /**
     * Get the rating history of a user. If a user is unrated in a certain variant, that variant will not be returned.
     * Get Ratings History
     */
    async getRatingsHistoryRaw(
        target: string,
        requestParameters: GetRatingsHistoryRequest,
        initOverrides?: RequestInit
    ): Promise<ApiResponse<Record<string, RatingOverview>>> {
        const response = await this.request(
            {
                path: `/profile/${encodeURIComponent(target)}/rating-history`,
                method: "GET",
                query: {
                    ...requestParameters,
                    since: requestParameters.since
                        .toISOString()
                        .substring(0, 10),
                },
            },
            initOverrides
        );

        return new JSONApiResponse(response);
    }
    getRatingsHistory = this.createFriendlyRoute(this.getRatingsHistoryRaw);

    /**
     * Paginate through game history for a specified target. Retrieve a paginated list of game results.
     * Paginate Games
     */
    async paginateGamesRaw(
        target: string,
        requestParameters: PaginateGamesRequest,
        initOverrides?: RequestInit
    ): Promise<ApiResponse<Array<FinishedGame>>> {
        const response = await this.request(
            {
                path: `/profile/${encodeURIComponent(target)}/games`,
                method: "GET",
                query: requestParameters,
            },
            initOverrides
        );

        return new JSONApiResponse(response);
    }
    paginateGames = this.createFriendlyRoute(this.paginateGamesRaw);

    /**
     * Get a user\'s profile picture. If the user hasn\'t uploaded a picture yet, the default one will be returned.
     * Profile Picture
     */
    async profilePictureRaw(
        target: string,
        initOverrides?: RequestInit
    ): Promise<ApiResponse<Blob>> {
        const response = await this.request(
            {
                path: `/profile/${encodeURIComponent(target)}/profile-picture`,
                method: "GET",
            },
            initOverrides
        );

        return new BlobApiResponse(response);
    }
    profilePicture = this.createFriendlyRoute(this.profilePictureRaw);

    /**
     * Count how many games a user has
     * Total Game Count
     */
    async totalGameCountRaw(
        target: string,
        initOverrides?: RequestInit
    ): Promise<ApiResponse<number>> {
        const response = await this.request(
            {
                path: `/profile/${encodeURIComponent(target)}/total-game-count`,
                method: "GET",
            },
            initOverrides
        );

        return new JSONApiResponse<number>(response);
    }
    totalGameCount = this.createFriendlyRoute(this.totalGameCountRaw);
}
