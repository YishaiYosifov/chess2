import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { StoreApi } from "zustand";
import QRCode from "qrcode";

import {
    ChallengeStore,
    createChallengeStore,
} from "@/features/challenges/stores/challengeStore";

import { createFakeChallengeRequest } from "@/lib/testUtils/fakers/challengeRequestFaker";
import ChallengeStoreContext from "@/features/challenges/contexts/challengeContext";
import flushMicrotasks from "@/lib/testUtils/flushMicrotasks";
import OpenChallengeView from "../OpenChallengeView";
import { ChallengeRequest } from "@/lib/apiClient";

vi.mock("qrcode");

describe("OpenChallengeView", () => {
    const qrCodeMock = vi.mocked(QRCode);
    const qrCodeText = "data:image/png;base64,fakeqr";
    const locationHref = "http://localhost:3000/challenge/12345";
    let challengeMock: ChallengeRequest;
    let challengeStore: StoreApi<ChallengeStore>;

    beforeEach(() => {
        // @ts-expect-error toDataURL could return void
        qrCodeMock.toDataURL.mockResolvedValue(qrCodeText);
        vi.stubGlobal("location", { href: locationHref });

        challengeMock = createFakeChallengeRequest();
        challengeStore = createChallengeStore({ challenge: challengeMock });
    });

    it("should render input and QR code", async () => {
        render(
            <ChallengeStoreContext.Provider value={challengeStore}>
                <OpenChallengeView />
            </ChallengeStoreContext.Provider>,
        );
        await flushMicrotasks();

        expect(screen.getByTestId("challengeStatusText")).toHaveTextContent(
            "Invite someone to play via:",
        );
        expect(screen.getByTestId("openChallengeViewInput")).toHaveValue(
            locationHref,
        );
        expect(screen.getByTestId("openChallengeViewQRCode")).toHaveAttribute(
            "src",
            qrCodeText,
        );
    });

    it("should copy challenge URL to clipboard when clipboard icon is clicked", async () => {
        const writeTextMock = vi.fn();
        const user = userEvent.setup();
        Object.defineProperty(navigator, "clipboard", {
            value: {
                writeText: writeTextMock,
            },
        });
        render(
            <ChallengeStoreContext.Provider value={challengeStore}>
                <OpenChallengeView />
            </ChallengeStoreContext.Provider>,
        );

        const clipboardIcon = screen.getByTestId("openChallengeViewCopy");
        await user.click(clipboardIcon);

        expect(writeTextMock).toHaveBeenCalledWith(location.href);
    });

    it("should update correctly when challenge is over", async () => {
        challengeStore.setState({ isCancelled: true });
        render(
            <ChallengeStoreContext.Provider value={challengeStore}>
                <OpenChallengeView />
            </ChallengeStoreContext.Provider>,
        );
        await flushMicrotasks();

        expect(screen.getByTestId("challengeStatusText")).toHaveClass(
            "text-error",
        );
        expect(screen.getByTestId("openChallengeViewInput")).toBeDisabled();
    });
});
