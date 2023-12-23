/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { AccessToken } from '../models/AccessToken';
import type { AuthTokens } from '../models/AuthTokens';
import type { Body_auth_login } from '../models/Body_auth_login';
import type { UserIn } from '../models/UserIn';
import type { UserOutSensitive } from '../models/UserOutSensitive';

import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';

export class AuthService {

    /**
     * Signup
     * Takes a username, email and password and creates registers a new user.
 *
 * This path operation will also:
 * - send a verification email
 * - create the necessary files
     * @returns UserOutSensitive Successful Response
     * @throws ApiError
     */
    public static signup({
requestBody,
}: {
requestBody: UserIn,
}): CancelablePromise<UserOutSensitive> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/auth/signup',
            body: requestBody,
            mediaType: 'application/json',
            errors: {
                401: `Could not verify credentials`,
                409: `Username / email already taken`,
                422: `Validation Error`,
            },
        });
    }

    /**
     * Login
     * Authenticates a user by generating a jwt access and refresh token if the credentials match.
     * @returns AuthTokens Successful Response
     * @throws ApiError
     */
    public static login({
formData,
}: {
formData: Body_auth_login,
}): CancelablePromise<AuthTokens> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/auth/login',
            formData: formData,
            mediaType: 'application/x-www-form-urlencoded',
            errors: {
                401: `Could not verify credentials`,
                422: `Validation Error`,
            },
        });
    }

    /**
     * Logout
     * @returns any Successful Response
     * @throws ApiError
     */
    public static logout(): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/auth/logout',
            errors: {
                401: `Could not verify credentials`,
            },
        });
    }

    /**
     * Refresh Access Token
     * Generate a new access token using a refresh token
     * @returns AccessToken Successful Response
     * @throws ApiError
     */
    public static refreshAccessToken(): CancelablePromise<AccessToken> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/auth/refresh-access-token',
            errors: {
                401: `Could not verify credentials`,
            },
        });
    }

    /**
     * Create Guest Account
     * @returns AccessToken Successful Response
     * @throws ApiError
     */
    public static createGuestAccount(): CancelablePromise<AccessToken> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/auth/guest-account',
            errors: {
                401: `Could not verify credentials`,
            },
        });
    }

    /**
     * Poggies Test
     * @returns any Successful Response
     * @throws ApiError
     */
    public static poggiesTest(): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/auth/test',
            errors: {
                401: `Could not verify credentials`,
            },
        });
    }

}
