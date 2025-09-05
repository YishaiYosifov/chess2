import { createFakeGuestUser } from "@/lib/testUtils/fakers/userFaker";
import { GuestUser, SessionUser } from "@/lib/apiClient";
import { fetchUserSession } from "../../lib/getLoggedIn";
import SessionProvider from "../../contexts/sessionContext";
import WithSession from "../WithSession";
import constants from "@/lib/constants";

vi.mock("next/navigation");
vi.mock("../../lib/getLoggedIn");

describe("WithSession", () => {
    let userMock: GuestUser;
    let sessionMock: { user: SessionUser; accessToken: string };

    const fetchUserSessionMock = vi.mocked(fetchUserSession);
    const childrenMock = vi.fn();

    beforeEach(() => {
        userMock = createFakeGuestUser();
        sessionMock = { user: userMock, accessToken: "token-abc" };
    });

    it("should render children with session when user is authenticated", async () => {
        fetchUserSessionMock.mockResolvedValue(sessionMock);

        const ui = await WithSession({
            children: childrenMock,
        });

        expect(ui.type).toBe(SessionProvider);
        expect(ui.props).toEqual(
            expect.objectContaining({ fetchAttempted: true, user: userMock }),
        );
        expect(childrenMock).toHaveBeenCalledWith(sessionMock);
    });

    it("should redirect to logout when no session", async () => {
        fetchUserSessionMock.mockResolvedValue(null);

        const act = () =>
            WithSession({
                children: childrenMock,
            });

        await expect(act).rejects.toThrow(`redirect ${constants.PATHS.LOGOUT}`);
        expect(childrenMock).not.toHaveBeenCalled();
    });
});
