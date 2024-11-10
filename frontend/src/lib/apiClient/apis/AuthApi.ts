import type {
    PrivateAuthedProfileOut,
    AccessToken,
    AuthTokens,
    UserIn,
} from "@/lib/models";

import {
    ApiResponse,
    BaseAPI,
    JSONApiResponse,
    VoidApiResponse,
} from "../runtime";

export interface LoginRequest {
    username: string;
    password: string;
}

export class AuthApi extends BaseAPI {
    /**
     * Authenticates a user by generating a jwt access and refresh token if the credentials match.
     * Login
     */
    async loginRaw(
        requestParameters: LoginRequest,
        initOverrides?: RequestInit
    ): Promise<ApiResponse<AuthTokens>> {
        const form = new URLSearchParams();
        form.append("username", requestParameters.username);
        form.append("password", requestParameters.password);

        const response = await this.request(
            {
                path: "/auth/login",
                method: "POST",
                body: form,
            },
            initOverrides
        );

        return new JSONApiResponse(response);
    }
    login = this.createFriendlyRoute(this.loginRaw);

    /**
     * Remove all auth cookies
     * Logout
     */
    async logoutRaw(initOverrides?: RequestInit): Promise<ApiResponse<void>> {
        const response = await this.request(
            {
                path: "/auth/logout",
                method: "POST",
            },
            initOverrides
        );
        return new VoidApiResponse(response);
    }
    logout = this.createFriendlyRoute(this.logoutRaw);

    /**
     * Generate a new access token using a refresh token
     * Refresh Access Token
     */
    async refreshAccessTokenRaw(
        refreshToken: string,
        initOverrides?: RequestInit
    ): Promise<ApiResponse<AccessToken>> {
        const response = await this.request(
            {
                path: "/auth/refresh-access-token",
                headers: { "Set-Cookie": refreshToken },
                method: "GET",
            },
            initOverrides
        );

        return new JSONApiResponse(response);
    }
    refreshAccessToken = this.createFriendlyRoute(this.refreshAccessTokenRaw);

    /**
     * Takes a username, email and password and creates registers a new user. This path operation will also send a verification email.
     * Signup
     */
    async signupRaw(
        requestParameters: UserIn,
        initOverrides?: RequestInit
    ): Promise<ApiResponse<PrivateAuthedProfileOut>> {
        const response = await this.request(
            {
                path: "/auth/signup",
                method: "POST",
                body: requestParameters,
            },
            initOverrides
        );

        return new JSONApiResponse(response);
    }
    signup = this.createFriendlyRoute(this.signupRaw);
}
