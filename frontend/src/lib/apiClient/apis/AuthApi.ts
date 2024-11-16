import { BaseAPI, HttpMethod } from "../baseApi";
import { VoidApiResponse } from "../apiResponse";
import { ApiResponse } from "../apiResponse";

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
        const response = await this.request(
            {
                path: "/auth/login",
                method: HttpMethod.Post,
                body: requestParameters,
            },
            initOverrides,
        );

        return new VoidApiResponse(response);
    }
    login = this.createFriendlyRoute(this.loginRaw);
}
