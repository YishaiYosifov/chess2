import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import QRCode from "qrcode";

import OpenChallengeDescription from "../OpenChallengeDescription";
import flushMicrotasks from "@/lib/testUtils/flushMicrotasks";
import { ChallengeRequest } from "@/lib/apiClient";
import { createFakeChallengeRequets } from "@/lib/testUtils/fakers/challengeRequestFaker";
import { StoreApi } from "zustand";
import {
    ChallengeStore,
    createChallengeStore,
} from "@/features/challenges/stores/challengeStore";
import ChallengeStoreContext from "@/features/challenges/contexts/challengeContext";

vi.mock("qrcode");

describe("OpenChallengeDescription", () => {
    const qrCodeMock = vi.mocked(QRCode);
    const qrCodeText = "data:image/png;base64,fakeqr";
    const locationHref = "http://localhost:3000/challenge/12345";
    let challengeMock: ChallengeRequest;
    let challengeStore: StoreApi<ChallengeStore>;

    beforeEach(() => {
        // @ts-expect-error toDataURL could return void
        qrCodeMock.toDataURL.mockResolvedValue(qrCodeText);
        vi.stubGlobal("location", { href: locationHref });

        challengeMock = createFakeChallengeRequets();
        challengeStore = createChallengeStore({ challenge: challengeMock });
    });

    it("should render OpenChallengeDescription input and QR code", async () => {
        render(
            <ChallengeStoreContext.Provider value={challengeStore}>
                <OpenChallengeDescription />
            </ChallengeStoreContext.Provider>,
        );
        await flushMicrotasks();

        expect(screen.getByTestId("openChallengeDescriptionInput")).toHaveValue(
            locationHref,
        );
        expect(
            screen.getByTestId("openChallengeDescriptionQRCode"),
        ).toHaveAttribute("src", qrCodeText);
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
                <OpenChallengeDescription />
            </ChallengeStoreContext.Provider>,
        );

        const clipboardIcon = screen.getByTestId(
            "openChallengeDescriptionCopy",
        );
        await user.click(clipboardIcon);

        expect(writeTextMock).toHaveBeenCalledWith(location.href);
    });

    it("should show 'Challenge Cancelled' when isCancelled is true", async () => {
        challengeStore.setState({ isCancelled: true });
        render(
            <ChallengeStoreContext.Provider value={challengeStore}>
                <OpenChallengeDescription />
            </ChallengeStoreContext.Provider>,
        );
        await flushMicrotasks();

        const title = screen.getByTestId("openChallengeDescriptionTitle");
        expect(title).toHaveTextContent("Challenge Cancelled");
        expect(
            screen.getByTestId("openChallengeDescriptionInput"),
        ).toBeDisabled();
    });

    it("should show 'Challenge Expired' when hasExpired is true", async () => {
        challengeStore.setState({ hasExpired: true });
        render(
            <ChallengeStoreContext.Provider value={challengeStore}>
                <OpenChallengeDescription />
            </ChallengeStoreContext.Provider>,
        );
        await flushMicrotasks();

        const title = screen.getByTestId("openChallengeDescriptionTitle");
        expect(title).toHaveTextContent("Challenge Expired");
        expect(
            screen.getByTestId("openChallengeDescriptionInput"),
        ).toBeDisabled();
    });
});
