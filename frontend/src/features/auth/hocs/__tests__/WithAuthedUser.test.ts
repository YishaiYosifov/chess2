import { createFakePrivateUser } from "@/lib/testUtils/fakers/userFaker";
import { fetchAuthedUserSession } from "../../lib/getLoggedIn";
import WithAuthedUser from "../WithAuthedUser";
import { PrivateUser } from "@/lib/apiClient";
import SessionProvider from "../../contexts/sessionContext";
import constants from "@/lib/constants";

vi.mock("next/navigation");
vi.mock("../../lib/getLoggedIn");

describe("WithAuthedUser", () => {
    let userMock: PrivateUser;
    let sessionMock: { user: PrivateUser; accessToken: string };

    const fetchAuthedUserSessionMock = vi.mocked(fetchAuthedUserSession);
    const childrenMock = vi.fn();

    beforeEach(() => {
        userMock = createFakePrivateUser();
        sessionMock = { user: userMock, accessToken: "test access token" };
    });

    it("should render children with session when user is authenticated", async () => {
        fetchAuthedUserSessionMock.mockResolvedValue(sessionMock);

        const ui = await WithAuthedUser({
            children: childrenMock,
        });
        expect(ui.type).toBe(SessionProvider);
        expect(ui.props).toEqual(
            expect.objectContaining({ fetchAttempted: true, user: userMock }),
        );
        expect(childrenMock).toHaveBeenCalledWith(sessionMock);
    });

    it("should redirect to logout when no session", async () => {
        fetchAuthedUserSessionMock.mockResolvedValue(null);

        const act = () =>
            WithAuthedUser({
                children: childrenMock,
            });

        await expect(act).rejects.toThrow(`redirect ${constants.PATHS.LOGOUT}`);
        expect(childrenMock).not.toHaveBeenCalled();
    });
});
