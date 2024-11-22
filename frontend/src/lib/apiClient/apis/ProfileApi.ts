import { ApiResponse, JSONApiResponse } from "../apiResponse";
import { BaseAPI, RequestOptions } from "../baseApi";
import type { PrivateUser, User } from "../models";

export class ProfileApi extends BaseAPI {
    async getAuthedUserRaw(
        initOverrides?: RequestOptions,
    ): Promise<ApiResponse<PrivateUser>> {
        const response = await this.request("/profile/me", {
            ...initOverrides,
            method: "GET",
        });
        return new JSONApiResponse(response);
    }
    getAuthedUser = this.createFriendlyRoute(this.getAuthedUserRaw);

    async getUserRaw(
        username: string,
        initOverrides?: RequestOptions,
    ): Promise<ApiResponse<User>> {
        const response = await this.request(
            `/profile/by-username/${username}`,
            { ...initOverrides, method: "GET" },
        );
        return new JSONApiResponse(response);
    }
    getUser = this.createFriendlyRoute(this.getAuthedUserRaw);
}
