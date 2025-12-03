import useLobbyStore from "../../stores/lobbyStore";
import { EventHandlers } from "@/features/signalR/hooks/useSignalREvent";
import { act, render } from "@testing-library/react";
import LobbyHandler from "../LobbyHandler";
import { mockRouter } from "@/lib/testUtils/mocks/mockRouter";
import constants from "@/lib/constants";
import { usePathname } from "next/navigation";
import { PoolType } from "@/lib/apiClient";
import { PoolKeyStr } from "../../lib/types";
import {
    LobbyClientEvents,
    useLobbyEmitter,
    useLobbyEvent,
} from "../../hooks/useLobbyHub";
import { createFakeOngoingGame } from "@/lib/testUtils/fakers/ongoingGameFaker";

vi.mock("@/features/lobby/hooks/useLobbyHub");
vi.mock("next/navigation");

describe("LobbyHandler", () => {
    const lobbyHandlers: EventHandlers<LobbyClientEvents> = {};
    const sendLobbyEventMock = vi.fn();
    const usePathnameMock = vi.mocked(usePathname);

    beforeEach(() => {
        useLobbyStore.setState(useLobbyStore.getInitialState());

        vi.mocked(useLobbyEmitter).mockReturnValue(sendLobbyEventMock);
        vi.mocked(useLobbyEvent).mockImplementation((event, handler) => {
            lobbyHandlers[event] = handler;
        });
    });

    it("should call redirect when match is found", () => {
        const gameToken = "test game";
        const { push } = mockRouter();

        render(<LobbyHandler />);
        act(() => lobbyHandlers.MatchFoundAsync?.(gameToken));

        expect(push).toHaveBeenCalledWith(
            `${constants.PATHS.GAME}/${gameToken}`,
        );
    });

    it("should send cleanup events and clear seeks when pathname changes and seeks are present", () => {
        const seeks = new Set<PoolKeyStr>([`${PoolType.CASUAL}-15+0`]);
        useLobbyStore.setState({ seeks });

        usePathnameMock.mockReturnValue("/path1");
        const { rerender } = render(<LobbyHandler />);

        usePathnameMock.mockReturnValue("/path2");
        rerender(<LobbyHandler />);

        expect(sendLobbyEventMock).toHaveBeenCalledWith(
            "CleanupConnectionAsync",
        );
    });

    it("should clear requestedOpenSeek when pathname changes", () => {
        useLobbyStore.setState({ requestedOpenSeek: true });

        usePathnameMock.mockReturnValue("/path1");
        const { rerender } = render(<LobbyHandler />);

        usePathnameMock.mockReturnValue("/path2");
        rerender(<LobbyHandler />);

        expect(useLobbyStore.getState().requestedOpenSeek).toBe(false);
        expect(sendLobbyEventMock).toHaveBeenCalledWith(
            "CleanupConnectionAsync",
        );
    });

    it("should not send cleanup events if pathname has not changed", () => {
        const seeks = new Set<PoolKeyStr>([`${PoolType.RATED}-10+5`]);
        useLobbyStore.setState({ seeks });

        usePathnameMock.mockReturnValue("/path1");
        const { rerender } = render(<LobbyHandler />);

        usePathnameMock.mockReturnValue("/path1");
        rerender(<LobbyHandler />);

        expect(sendLobbyEventMock).not.toHaveBeenCalled();
    });

    it("should do nothing if pathname changes but no seeks are present", () => {
        useLobbyStore.setState({ seeks: new Set() });
        usePathnameMock.mockReturnValue("/new-path");

        usePathnameMock.mockReturnValue("/path1");
        const { rerender } = render(<LobbyHandler />);

        usePathnameMock.mockReturnValue("/path2");
        rerender(<LobbyHandler />);

        expect(sendLobbyEventMock).not.toHaveBeenCalled();
    });

    it("should add ongoing games when ReceiveOngoingGamesAsync is triggered", () => {
        const games = [createFakeOngoingGame(), createFakeOngoingGame()];

        render(<LobbyHandler />);
        act(() => lobbyHandlers.ReceiveOngoingGamesAsync?.(games));

        expect(
            Array.from(useLobbyStore.getState().ongoingGames.values()),
        ).toEqual(games);
    });

    it("should remove an ongoing game when OngoingGameEndedAsync is triggered", () => {
        const initial = new Map([
            ["game123", createFakeOngoingGame({ gameToken: "game123" })],
            ["game456", createFakeOngoingGame({ gameToken: "game456" })],
        ]);
        useLobbyStore.setState({ ongoingGames: initial });

        render(<LobbyHandler />);
        act(() => lobbyHandlers.OngoingGameEndedAsync?.("game456"));

        expect(useLobbyStore.getState().ongoingGames.has("game456")).toBe(
            false,
        );
        expect(useLobbyStore.getState().ongoingGames.has("game123")).toBe(true);
    });
});
