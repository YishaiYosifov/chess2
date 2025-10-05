import { render, screen } from "@testing-library/react";
import SessionProvider from "@/features/auth/contexts/sessionContext";
import { createFakeChallengeRequets } from "@/lib/testUtils/fakers/challengeRequestFaker";
import { createFakePrivateUser } from "@/lib/testUtils/fakers/userFaker";
import ChallengeDescription from "../ChallengeDescription";
import QRCode from "qrcode";
import userEvent from "@testing-library/user-event";
import flushMicrotasks from "@/lib/testUtils/flushMicrotasks";

vi.mock("qrcode");

describe("ChallengeDescription", () => {
    const userMock = createFakePrivateUser();
    const challengeMock = createFakeChallengeRequets();
    const qrCodeMock = vi.mocked(QRCode);
    const qrCodeText = "data:image/png;base64,fakeqr";
    const locationHref = "http://localhost:3000/challenge/12345";

    beforeEach(() => {
        // @ts-expect-error toDataURL could return void
        qrCodeMock.toDataURL.mockResolvedValue(qrCodeText);
        vi.stubGlobal("location", { href: locationHref });
    });

    it("should render DirectChallengeDescription when user is requester and recipient exists", () => {
        challengeMock.requester.userId = userMock.userId;
        challengeMock.recipient = createFakePrivateUser();

        render(
            <SessionProvider user={userMock}>
                <ChallengeDescription challenge={challengeMock} />
            </SessionProvider>,
        );

        expect(
            screen.getByTestId("directChallengeDescriptionTitle"),
        ).toHaveTextContent("Waiting For");
        expect(
            screen.getByTestId("directChallengeDescriptionUserName"),
        ).toHaveTextContent(challengeMock.recipient.userName);
    });

    it("should render OpenChallengeDescription input and QR code", async () => {
        challengeMock.requester.userId = userMock.userId;
        challengeMock.recipient = undefined;

        render(
            <SessionProvider user={userMock}>
                <ChallengeDescription challenge={challengeMock} />
            </SessionProvider>,
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
        challengeMock.requester.userId = userMock.userId;
        challengeMock.recipient = undefined;

        const writeTextMock = vi.fn();
        const user = userEvent.setup();
        Object.defineProperty(navigator, "clipboard", {
            value: {
                writeText: writeTextMock,
            },
        });
        render(
            <SessionProvider user={userMock}>
                <ChallengeDescription challenge={challengeMock} />
            </SessionProvider>,
        );

        const clipboardIcon = screen.getByTestId(
            "openChallengeDescriptionCopy",
        );
        await user.click(clipboardIcon);

        expect(writeTextMock).toHaveBeenCalledWith(location.href);
    });

    it("should render ChallengeRecipientDescription when user is not requester", () => {
        challengeMock.requester.userId = "different-user";

        render(
            <SessionProvider user={userMock}>
                <ChallengeDescription challenge={challengeMock} />
            </SessionProvider>,
        );

        expect(
            screen.getByTestId("challengeRecipientDescriptionTitle"),
        ).toHaveTextContent("Challenged By");
        expect(
            screen.getByTestId("challengeRecipientDescriptionUserName"),
        ).toHaveTextContent(challengeMock.requester.userName);
    });
});
