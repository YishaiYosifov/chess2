import { ApiResponse, JSONApiResponse } from "../apiResponse";
import { BaseAPI, RequestOptions } from "../baseApi";
import type { PrivateUser } from "../models";

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
}
