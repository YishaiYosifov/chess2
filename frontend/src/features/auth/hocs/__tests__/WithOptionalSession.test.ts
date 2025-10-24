import { SessionUser } from "@/lib/apiClient";
import { fetchUserSession } from "../../lib/getLoggedIn";
import WithOptionalSession from "../WithOptionalSession";
import SessionProvider from "../../contexts/sessionContext";
import { createFakeGuestUser } from "@/lib/testUtils/fakers/userFaker";

vi.mock("next/navigation");
vi.mock("../../lib/getLoggedIn");

describe("WithOptionalSession", () => {
    let userMock: SessionUser;
    let sessionMock: { user: SessionUser; accessToken: string };

    const fetchUserSessionMock = vi.mocked(fetchUserSession);
    const childrenMock = vi.fn();

    beforeEach(() => {
        userMock = createFakeGuestUser();
        sessionMock = { user: userMock, accessToken: "token-xyz" };
    });

    it("should render children with session data when a session exists", async () => {
        fetchUserSessionMock.mockResolvedValue(sessionMock);

        const ui = await WithOptionalSession({
            children: childrenMock,
        });

        expect(ui.type).toBe(SessionProvider);
        expect(ui.props).toEqual(
            expect.objectContaining({
                user: userMock,
                fetchAttempted: true,
            }),
        );
        expect(childrenMock).toHaveBeenCalledWith({
            user: userMock,
            accessToken: "token-xyz",
        });
    });

    it("should render children with null session data when no session exists", async () => {
        fetchUserSessionMock.mockResolvedValue(null);

        const ui = await WithOptionalSession({
            children: childrenMock,
        });

        expect(ui.type).toBe(SessionProvider);
        expect(ui.props).toEqual(
            expect.objectContaining({
                user: null,
                fetchAttempted: true,
            }),
        );

        expect(childrenMock).toHaveBeenCalledWith({
            user: null,
            accessToken: null,
        });
    });
});
