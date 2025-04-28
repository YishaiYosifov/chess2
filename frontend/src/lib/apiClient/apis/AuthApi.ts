import type { UserIn, UserLogin } from "@/lib/apiClient/models";
import { BaseAPI, RequestOptions } from "../baseApi";
import { VoidApiResponse } from "../apiResponse";
import { ApiResponse } from "../apiResponse";

export class AuthApi extends BaseAPI {
    /**
     * Authenticates a user by generating a jwt access and refresh token if the credentials match.
     * Login
     */
    async loginRaw(
        requestParams: UserLogin,
        initOverrides?: RequestOptions,
    ): Promise<ApiResponse<void>> {
        const response = await this.request("/auth/signin", {
            ...initOverrides,
            method: "POST",
            body: requestParams,
        });
        return new VoidApiResponse(response);
    }
    login = this.createFriendlyRoute(this.loginRaw);

    async signupRaw(
        requestParams: UserIn,
        initOverrides?: RequestOptions,
    ): Promise<ApiResponse<void>> {
        const response = await this.request("/auth/signup", {
            ...initOverrides,
            method: "POST",
            body: requestParams,
        });
        return new VoidApiResponse(response);
    }
    signup = this.createFriendlyRoute(this.signupRaw);
}
