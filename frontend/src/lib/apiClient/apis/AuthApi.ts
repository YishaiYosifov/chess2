import type { UserIn, UserLogin } from "@/lib/models";
import { VoidApiResponse } from "../apiResponse";
import { ApiResponse } from "../apiResponse";
import { BaseAPI } from "../baseApi";

export class AuthApi extends BaseAPI {
    /**
     * Authenticates a user by generating a jwt access and refresh token if the credentials match.
     * Login
     */
    async loginRaw(
        requestParams: UserLogin,
        initOverrides?: RequestInit,
    ): Promise<ApiResponse<void>> {
        const response = await this.request("/auth/login", {
            ...initOverrides,
            method: "POST",
            body: requestParams,
        });
        return new VoidApiResponse(response);
    }
    login = this.createFriendlyRoute(this.loginRaw);

    async signupRaw(
        requestParams: UserIn,
        initOverrides?: RequestInit,
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
