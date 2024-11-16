import { VoidApiResponse } from "../apiResponse";
import { ApiResponse } from "../apiResponse";
import { BaseAPI } from "../baseApi";

export interface LoginRequest {
    usernameOrEmail: string;
    password: string;
}

export class AuthApi extends BaseAPI {
    /**
     * Authenticates a user by generating a jwt access and refresh token if the credentials match.
     * Login
     */
    async loginRaw(
        requestParameters: LoginRequest,
        initOverrides?: RequestInit,
    ): Promise<ApiResponse<void>> {
        const response = await this.request("/auth/login", {
            ...initOverrides,
            method: "POST",
            body: requestParameters,
        });

        return new VoidApiResponse(response);
    }
    login = this.createFriendlyRoute(this.loginRaw);
}
