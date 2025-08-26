import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";

import {
    deleteProfilePicture,
    ErrorCode,
    PrivateUser,
    uploadProfilePicture,
} from "@/lib/apiClient";
import constants from "@/lib/constants";
import { createFakePrivateUser } from "@/lib/testUtils/fakers/userFaker";
import SessionProvider from "@/features/auth/contexts/sessionContext";
import ProfilePictureForm from "../ProfilePictureForm";

vi.mock("@/lib/apiClient");

describe("ProfilePictureForm", () => {
    let userMock: PrivateUser;
    const uploadProfilePictureMock = vi.mocked(uploadProfilePicture);
    const deleteProfilePictureMock = vi.mocked(deleteProfilePicture);

    beforeEach(() => {
        userMock = createFakePrivateUser();
    });

    beforeEach(() => {
        vi.clearAllMocks();
    });

    it("should render upload button and trash icon", async () => {
        render(
            <SessionProvider user={userMock}>
                <ProfilePictureForm />
            </SessionProvider>,
        );

        expect(screen.getByTestId("uploadProfilePicture")).toBeInTheDocument();
        expect(screen.getByTestId("clearProfilePicture")).toBeInTheDocument();

        const uploadInput = screen.getByTestId("profilePictureFileInput");
        expect(uploadInput).toBeInTheDocument();
        expect(uploadInput.hidden).toBe(true);
    });

    it("should show error if uploaded file exceeds max size", async () => {
        const user = userEvent.setup();
        render(
            <SessionProvider user={userMock}>
                <ProfilePictureForm />
            </SessionProvider>,
        );

        const file = new File(
            ["a".repeat(constants.PROFILE_PICTURE_MAX_SIZE + 1)],
            "big.png",
            {
                type: "image/png",
            },
        );

        const hiddenInput = screen.getByTestId("profilePictureFileInput");
        await user.upload(hiddenInput, file);

        expect(
            await screen.findByTestId("profilePictureError"),
        ).toHaveTextContent("Profile picture is cannot exceed 2MB");
    });

    it("should call uploadProfilePicture on valid file upload and clear error", async () => {
        const user = userEvent.setup();
        uploadProfilePictureMock.mockResolvedValue({
            data: undefined,
            response: new Response(),
        });

        render(
            <SessionProvider user={userMock}>
                <ProfilePictureForm />
            </SessionProvider>,
        );

        let profilePictureImg: HTMLImageElement =
            screen.getByTestId("profilePicture");
        expect(profilePictureImg.src.endsWith("?0")).toBe(true);

        const file = new File(["content"], "small.png", { type: "image/png" });
        const hiddenInput = screen.getByTestId("profilePictureFileInput");

        await user.upload(hiddenInput, file);

        expect(uploadProfilePicture).toHaveBeenCalledWith({
            body: { File: file },
        });
        expect(screen.queryByTestId("profilePictureError")).toBeNull();

        profilePictureImg = screen.getByTestId("profilePicture");
        expect(profilePictureImg.src.endsWith("?1")).toBe(true);
    });

    it("should show API error if upload fails", async () => {
        const user = userEvent.setup();
        uploadProfilePictureMock.mockResolvedValue({
            error: {
                errors: [
                    {
                        errorCode: ErrorCode.USER_INVALID_PROFILE_PICTURE,
                        description: "Upload failed",
                        metadata: {},
                    },
                ],
                extensions: {},
            },
            response: new Response(),
            data: undefined,
        });

        render(
            <SessionProvider user={userMock}>
                <ProfilePictureForm />
            </SessionProvider>,
        );

        const file = new File(["content"], "small.png", { type: "image/png" });
        const hiddenInput = screen.getByTestId("profilePictureFileInput");
        await user.upload(hiddenInput, file);

        expect(
            await screen.findByTestId("profilePictureError"),
        ).toHaveTextContent("Upload failed");
    });

    it("should call deleteProfilePicture when trash icon is clicked", async () => {
        const user = userEvent.setup();
        deleteProfilePictureMock.mockResolvedValue({
            error: undefined,
            response: new Response(),
            data: undefined,
        });

        render(
            <SessionProvider user={userMock}>
                <ProfilePictureForm />
            </SessionProvider>,
        );

        const trash = screen.getByTestId("clearProfilePicture");
        await user.click(trash);

        expect(deleteProfilePictureMock).toHaveBeenCalled();
        expect(screen.queryByTestId("profilePictureError")).toBeNull();

        const profilePictureImg: HTMLImageElement =
            screen.getByTestId("profilePicture");
        expect(profilePictureImg.src.endsWith("?1")).toBe(true);
    });

    it("should show error if deleteProfilePicture fails", async () => {
        const user = userEvent.setup();
        deleteProfilePictureMock.mockResolvedValue({
            error: { errors: [], extensions: {} },
            response: new Response(),
            data: undefined,
        });

        render(
            <SessionProvider user={userMock}>
                <ProfilePictureForm />
            </SessionProvider>,
        );

        const trash = screen.getByTestId("clearProfilePicture");
        await user.click(trash);

        expect(
            await screen.findByTestId("profilePictureError"),
        ).toHaveTextContent("Failed to clear profile picture");
    });
});
