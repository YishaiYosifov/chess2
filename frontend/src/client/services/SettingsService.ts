/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { BaseUserProfile } from '../models/BaseUserProfile';
import type { Body_settings_upload_profile_picture } from '../models/Body_settings_upload_profile_picture';
import type { UserOut } from '../models/UserOut';

import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';

export class SettingsService {

    /**
     * Update Profile
     * @returns UserOut Successful Response
     * @throws ApiError
     */
    public static updateProfile({
requestBody,
}: {
requestBody: BaseUserProfile,
}): CancelablePromise<UserOut> {
        return __request(OpenAPI, {
            method: 'PATCH',
            url: '/settings/profile',
            body: requestBody,
            mediaType: 'application/json',
            errors: {
                401: `Could not verify credentials`,
                422: `Validation Error`,
            },
        });
    }

    /**
     * Change Email
     * Update the email and send an email verification.
 * This will also unverify the user email.
 *
 * Requires a fresh JWT token.
     * @returns UserOut Successful Response
     * @throws ApiError
     */
    public static changeEmail({
requestBody,
}: {
requestBody: string,
}): CancelablePromise<UserOut> {
        return __request(OpenAPI, {
            method: 'PUT',
            url: '/settings/email',
            body: requestBody,
            mediaType: 'application/json',
            errors: {
                401: `Could not verify credentials`,
                409: `Email taken`,
                422: `Validation Error`,
            },
        });
    }

    /**
     * Change Password
     * Hash the password and update it. Requires a fresh JWT token.
     * @returns UserOut Successful Response
     * @throws ApiError
     */
    public static changePassword({
requestBody,
}: {
requestBody: string,
}): CancelablePromise<UserOut> {
        return __request(OpenAPI, {
            method: 'PUT',
            url: '/settings/password',
            body: requestBody,
            mediaType: 'application/json',
            errors: {
                401: `Could not verify credentials`,
                422: `Validation Error`,
            },
        });
    }

    /**
     * Change Username
     * Update the username
     * @returns UserOut Successful Response
     * @throws ApiError
     */
    public static changeUsername({
requestBody,
}: {
requestBody: string,
}): CancelablePromise<UserOut> {
        return __request(OpenAPI, {
            method: 'PUT',
            url: '/settings/username',
            body: requestBody,
            mediaType: 'application/json',
            errors: {
                401: `Could not verify credentials`,
                409: `Username taken`,
                422: `Validation Error`,
                429: `Username changed too recently`,
            },
        });
    }

    /**
     * Upload Profile Picture
     * Change a user's profile picture.
 * The picture must be < 1mb and a valid image.
     * @returns any Successful Response
     * @throws ApiError
     */
    public static uploadProfilePicture({
formData,
}: {
formData: Body_settings_upload_profile_picture,
}): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'PUT',
            url: '/settings/upload-profile-picture',
            formData: formData,
            mediaType: 'multipart/form-data',
            errors: {
                400: `Bad image`,
                401: `Could not verify credentials`,
                413: `Profile picture too large`,
                422: `Validation Error`,
            },
        });
    }

}
