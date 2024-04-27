import {
    JSONApiResponse,
    VoidApiResponse,
    ApiResponse,
    BaseAPI,
} from "../runtime";
import type { EditableProfile, PrivateAuthedProfileOut } from "@/models";

export class SettingsApi extends BaseAPI {
    /**
     * Update the email and send an email verification. This will also unverify the user email.  Requires a fresh JWT token.
     * Change Email
     */
    async changeEmailRaw(
        newEmail: string,
        initOverrides?: RequestInit
    ): Promise<ApiResponse<PrivateAuthedProfileOut>> {
        const response = await this.request(
            {
                path: "/settings/email",
                method: "PUT",
                body: newEmail,
            },
            initOverrides
        );

        return new JSONApiResponse(response);
    }
    changeEmail = this.createFriendlyRoute(this.changeEmailRaw);

    /**
     * Hash the password and update it. Requires a fresh JWT token.
     * Change Password
     */
    async changePasswordRaw(
        newPassword: string,
        initOverrides?: RequestInit
    ): Promise<ApiResponse<PrivateAuthedProfileOut>> {
        const response = await this.request(
            {
                path: "/settings/password",
                method: "PUT",
                body: newPassword,
            },
            initOverrides
        );

        return new JSONApiResponse(response);
    }
    changePassword = this.createFriendlyRoute(this.changePasswordRaw);

    /**
     * Update the username
     * Change Username
     */
    async changeUsernameRaw(
        newUsername: string,
        initOverrides?: RequestInit
    ): Promise<ApiResponse<PrivateAuthedProfileOut>> {
        const response = await this.request(
            {
                path: "/settings/username",
                method: "PUT",
                body: newUsername,
            },
            initOverrides
        );

        return new JSONApiResponse(response);
    }
    changeUsername = this.createFriendlyRoute(this.changeUsernameRaw);

    /**
     * Update Profile
     */
    async updateProfileRaw(
        requestParameters: EditableProfile,
        initOverrides?: RequestInit
    ): Promise<ApiResponse<PrivateAuthedProfileOut>> {
        const response = await this.request(
            {
                path: "/settings/profile",
                method: "PATCH",
                body: requestParameters,
            },
            initOverrides
        );

        return new JSONApiResponse(response);
    }
    updateProfile = this.createFriendlyRoute(this.updateProfileRaw);

    /**
     * Change a user\'s profile picture. The picture must be < 1mb and a valid image.
     * Upload Profile Picture
     */
    async uploadProfilePictureRaw(
        profilePicture: Blob,
        initOverrides?: RequestInit
    ): Promise<ApiResponse<void>> {
        const response = await this.request(
            {
                path: "/settings/upload-profile-picture",
                method: "PUT",
                headers: { contentType: "multipart/form-data" },
                body: profilePicture,
            },
            initOverrides
        );

        return new VoidApiResponse(response);
    }
    uploadProfilePicture = this.createFriendlyRoute(
        this.uploadProfilePictureRaw
    );
}
