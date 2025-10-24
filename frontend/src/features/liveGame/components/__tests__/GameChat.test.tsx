import userEvent from "@testing-library/user-event";
import { screen, render, act } from "@testing-library/react";

import GameChat from "../GameChat";
import LiveChessStoreContext from "../../contexts/liveChessContext";
import { StoreApi } from "zustand";
import createLiveChessStore, {
    LiveChessStore,
} from "../../stores/liveChessStore";
import { createFakeLiveChessStoreProps } from "@/lib/testUtils/fakers/liveChessStoreFaker";
import { mockRouter } from "@/lib/testUtils/mocks/mockRouter";
import { EventHandlers } from "@/features/signalR/hooks/useSignalREvent";
import SessionProvider from "@/features/auth/contexts/sessionContext";
import { GuestUser, PrivateUser } from "@/lib/apiClient";
import { createFakePrivateUser } from "@/lib/testUtils/fakers/userFaker";
import {
    GameClientEvents,
    useGameEmitter,
    useGameEvent,
} from "../../hooks/useGameHub";

vi.mock("@/features/liveGame/hooks/useGameHub");

describe("GameChat", () => {
    const sendGameEventMock = vi.fn();
    const useGameEventMock = vi.mocked(useGameEvent);

    const gameToken = "testGameToken";
    let store: StoreApi<LiveChessStore>;
    let userMock: PrivateUser;

    beforeEach(() => {
        Element.prototype.scrollTo = vi.fn();
        vi.mocked(useGameEmitter).mockReturnValue(sendGameEventMock);
        vi.useFakeTimers({ shouldAdvanceTime: true });

        store = createLiveChessStore(createFakeLiveChessStoreProps());
        store.setState({ gameToken });

        userMock = createFakePrivateUser();
    });

    it("should render the input", () => {
        render(
            <SessionProvider user={userMock}>
                <LiveChessStoreContext.Provider value={store}>
                    <GameChat initialShowChat={true} />
                </LiveChessStoreContext.Provider>
            </SessionProvider>,
        );

        const input = screen.getByTestId("gameChatInput");
        expect(input).toBeInTheDocument();
        expect(input).toHaveAttribute("placeholder", "Send a Message...");

        expect(screen.queryByTestId("gameChatUser")).not.toBeInTheDocument();
        expect(
            screen.queryByTestId("gameChatMessageContent"),
        ).not.toBeInTheDocument();
    });

    it("should disable input when a guest", () => {
        const guest: GuestUser = { userId: "test", type: "guest" };
        render(
            <SessionProvider user={guest}>
                <LiveChessStoreContext.Provider value={store}>
                    <GameChat initialShowChat={true} />
                </LiveChessStoreContext.Provider>
            </SessionProvider>,
        );

        const input = screen.getByTestId("gameChatInput");
        expect(input).toBeDisabled();
        expect(input).toHaveAttribute("placeholder", "Sign Up to Chat!");
    });

    it("should send a message and clear the input", async () => {
        const user = userEvent.setup();

        render(
            <SessionProvider user={userMock}>
                <LiveChessStoreContext.Provider value={store}>
                    <GameChat initialShowChat={true} />
                </LiveChessStoreContext.Provider>
            </SessionProvider>,
        );

        const input = screen.getByTestId("gameChatInput");
        await user.type(input, "test message");
        await user.keyboard("{Enter}");

        expect(sendGameEventMock).toHaveBeenCalledWith(
            "SendChatAsync",
            gameToken,
            "test message",
        );

        expect(input).toHaveValue("");
    });

    it("should not send empty or whitespace-only messages", async () => {
        const user = userEvent.setup();
        render(
            <SessionProvider user={userMock}>
                <LiveChessStoreContext.Provider value={store}>
                    <GameChat initialShowChat={true} />
                </LiveChessStoreContext.Provider>
            </SessionProvider>,
        );

        const input = screen.getByTestId("gameChatInput");
        await user.type(input, "   ");
        await user.keyboard("{Enter}");

        expect(sendGameEventMock).not.toHaveBeenCalled();
    });

    it("should disable input and show cooldown message when cooldown is active", async () => {
        const gameEventHandlers: EventHandlers<GameClientEvents> = {};
        useGameEventMock.mockImplementation((_token, event, handler) => {
            gameEventHandlers[event] = handler;
        });

        render(
            <SessionProvider user={userMock}>
                <LiveChessStoreContext.Provider value={store}>
                    <GameChat initialShowChat={true} />
                </LiveChessStoreContext.Provider>
            </SessionProvider>,
        );

        const cooldownLeftMs = 1000;
        act(() =>
            gameEventHandlers["ChatMessageDeliveredAsync"]?.(cooldownLeftMs),
        );

        const input = screen.getByTestId("gameChatInput");
        expect(input).toBeDisabled();
        expect(input).toHaveAttribute("placeholder", "Too fast, slow down...");

        await act(() => vi.advanceTimersByTimeAsync(cooldownLeftMs));

        expect(input).not.toBeDisabled();
        expect(input).toHaveAttribute("placeholder", "Send a Message...");
    });

    it("should add received chat messages to the list", async () => {
        const gameEventHandlers: EventHandlers<GameClientEvents> = {};
        useGameEventMock.mockImplementation((_token, event, handler) => {
            gameEventHandlers[event] = handler;
        });

        const user = userEvent.setup();
        const { push } = mockRouter();
        render(
            <SessionProvider user={userMock}>
                <LiveChessStoreContext.Provider value={store}>
                    <GameChat initialShowChat={true} />
                </LiveChessStoreContext.Provider>
            </SessionProvider>,
        );

        const userName = "user123";
        const message = "Test message";

        act(() => {
            gameEventHandlers["ChatMessageAsync"]?.(userName, message);
        });

        const messageUser = screen.getByTestId("gameChatUser");
        expect(messageUser).toHaveTextContent(userName);

        const messageValue = screen.getByTestId("gameChatMessageContent");
        expect(messageValue).toHaveTextContent(message);

        await user.click(messageUser);
        expect(push).toHaveBeenNthCalledWith(1, `/profile/${userName}`);
    });

    it("should show a button to reveal chat when collapsed", async () => {
        const gameEventHandlers: EventHandlers<GameClientEvents> = {};
        useGameEventMock.mockImplementation((_token, event, handler) => {
            gameEventHandlers[event] = handler;
        });

        render(
            <SessionProvider user={userMock}>
                <LiveChessStoreContext.Provider value={store}>
                    <GameChat initialShowChat={false} />
                </LiveChessStoreContext.Provider>
            </SessionProvider>,
        );

        act(() => {
            gameEventHandlers["ChatMessageAsync"]?.("user1", "message1");
        });

        const showButton = screen.getByTestId("showChatMessagesButton");
        expect(showButton).toBeInTheDocument();
        expect(showButton).toHaveTextContent(/Received 1 messages, show/i);

        act(() => {
            gameEventHandlers["ChatMessageAsync"]?.("user2", "message2");
        });
        expect(showButton).toHaveTextContent(/Received 2 messages, show/i);

        await userEvent.click(showButton);

        const messages = screen.getAllByTestId("gameChatMessage");
        expect(messages.map((x) => x.textContent)).toEqual([
            "user1: message1",
            "user2: message2",
        ]);
    });
});
