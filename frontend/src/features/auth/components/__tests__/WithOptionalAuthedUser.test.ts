import { PrivateUser } from "@/lib/apiClient";
import { fetchAuthedUserSession } from "../../lib/getLoggedIn";
import { createFakePrivateUser } from "@/lib/testUtils/fakers/userFaker";
import WithOptionalAuthedUser from "../WithOptionalAuthedUser";
import SessionProvider from "../../contexts/sessionContext";

vi.mock("../../lib/getLoggedIn");

describe("WithOptionalAuthedUser", () => {
    let userMock: PrivateUser;
    let sessionMock: { user: PrivateUser; accessToken: string };
    const fetchAuthedUserSessionMock = vi.mocked(fetchAuthedUserSession);
    const childrenMock = vi.fn();

    beforeEach(() => {
        userMock = createFakePrivateUser();
    });

    it("should render children with user when session exists", async () => {
        sessionMock = { user: userMock, accessToken: "token-abc" };
        fetchAuthedUserSessionMock.mockResolvedValue(sessionMock);

        const ui = await WithOptionalAuthedUser({
            children: childrenMock,
        });

        expect(ui.type).toBe(SessionProvider);
        expect(ui.props).toEqual(
            expect.objectContaining({ fetchAttempted: true, user: userMock }),
        );
        expect(childrenMock).toHaveBeenCalledWith(sessionMock);
    });

    it("should render children with null user when no session", async () => {
        fetchAuthedUserSessionMock.mockResolvedValue(null);

        const ui = await WithOptionalAuthedUser({
            children: childrenMock,
        });

        expect(ui.type).toBe(SessionProvider);
        expect(ui.props).toEqual(
            expect.objectContaining({ fetchAttempted: true, user: null }),
        );
        expect(childrenMock).toHaveBeenCalledWith({
            user: null,
            accessToken: null,
        });
    });
});
