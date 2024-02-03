import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { Mock } from "vitest";

import { responseErrFactory } from "@/lib/utils/testUtils";
import { settingsApi } from "@/lib/apis";
import constants from "@/lib/constants";

import * as ProfilePicture from "@/components/ProfilePicture";
import ProfilePictureSettings from "../ProfilePictureSettings";
import { profileMock } from "@/mockUtils/profileMock";
import { revalidateUser } from "@/app/actions";

vi.mock("@/components/contexts/AuthContext");
vi.mock("@/app/actions");
vi.mock("@/lib/apis");

describe("ProfilePictureSettings", () => {
    const testErr = '{"detail": "testErr"}';
    const pfpMock = vi.spyOn(ProfilePicture, "default");

    /**
     * Render and submit a profile picture
     *
     * @returns the rendered component
     */
    async function uploadTestImage() {
        const user = userEvent.setup();
        const rendered = render(<ProfilePictureSettings />);

        const fileSelector = screen.getByTestId("pfpSettingsFileSelector");

        const file = new File(["test"], "test.png", { type: "image/png" });
        await user.upload(fileSelector, file);

        return rendered;
    }

    it("should correctly render the component", () => {
        render(<ProfilePictureSettings />);

        const fileSelector = screen.getByTestId("pfpSettingsFileSelector");
        expect(fileSelector.hidden).toBeTruthy();
        expect(screen.queryByTestId("pfpSettingsSubmit")).toBeInTheDocument();
        expect(screen.queryByTestId("profilePicture")).toBeInTheDocument();
    });

    it("should open the file selector on button click", async () => {
        const user = userEvent.setup();
        render(<ProfilePictureSettings />);

        const updateButton = screen.getByTestId("pfpSettingsSubmit");
        const onClickSpy = vi.spyOn(HTMLInputElement.prototype, "click");

        await user.click(updateButton);
        expect(onClickSpy).toHaveBeenCalledOnce();
    });

    it.each([
        [new Error(), constants.GENERIC_ERROR],
        [responseErrFactory(testErr, { status: 400 }), "testErr"],
        [responseErrFactory(testErr, { status: 413 }), "testErr"],
    ])("should handle errors correctly", async (err, status) => {
        const uploadPfpMock = settingsApi.uploadProfilePicture as Mock;
        uploadPfpMock.mockRejectedValue(err);

        await uploadTestImage();

        expect(screen.getByTestId("pfpSettingsStatus").textContent).toBe(
            status
        );
        expect(pfpMock).toHaveBeenLastCalledWith(
            expect.objectContaining({
                lastChanged: profileMock.pfpLastChanged,
            }),
            {}
        );
    });

    it("should update update the profile picture site-wise on success", async () => {
        const testDate = new Date(69420);
        vi.setSystemTime(testDate);

        const uploadPfpMock = settingsApi.uploadProfilePicture as Mock;
        uploadPfpMock.mockResolvedValue({});
        await uploadTestImage();

        expect(revalidateUser).toHaveBeenCalledOnce();
        expect(screen.getByTestId("pfpSettingsStatus").textContent).toBe("");
        expect(pfpMock).toHaveBeenLastCalledWith(
            expect.objectContaining({
                lastChanged: testDate,
            }),
            {}
        );
    });
});
