import { renderHook } from "@testing-library/react";
import { mock } from "vitest-mock-extended";
import { act } from "react";

import useSignalRStore from "../signalRStore";
import {
    mockHubBuilder,
    mockHubConnection,
} from "@/lib/testUtils/mocks/mockSignalR";
import {
    HubConnection,
    HubConnectionBuilder,
    HubConnectionState,
    LogLevel,
} from "@microsoft/signalr";
import flushMicrotasks from "@/lib/testUtils/flushMicrotasks";
import EnsureAuthRetryPolicy from "../../lib/ensureAuthRetryPolicy";
import ensureAuth from "@/features/auth/lib/ensureAuth";
import { ErrorCode } from "@/lib/apiClient";

vi.mock("@microsoft/signalr");
vi.mock("@/features/auth/lib/ensureAuth");

describe("signalRStore", () => {
    const hubBuilderMock = vi.mocked(HubConnectionBuilder);
    const ensureAuthMock = vi.mocked(ensureAuth);
    const url = "test-url";

    beforeEach(() => {
        useSignalRStore.setState(useSignalRStore.getInitialState());
        vi.useFakeTimers();
    });

    function renderSignalRStore() {
        return renderHook(() => useSignalRStore()).result.current;
    }

    describe("joinHub", () => {
        it("should create a hub if it doesn't exist", async () => {
            const { mockConnection } = mockHubConnection();
            const hubBuilderInstanceMock = mockHubBuilder(mockConnection);

            const { joinHub } = renderSignalRStore();

            act(() => joinHub(url));
            await flushMicrotasks();

            const stored = useSignalRStore.getState().hubs.get(url);
            expect(stored?.connection).toBe(mockConnection);

            expect(
                hubBuilderInstanceMock.withUrl,
            ).toHaveBeenCalledExactlyOnceWith(url);
            expect(
                hubBuilderInstanceMock.withAutomaticReconnect,
            ).toHaveBeenCalledExactlyOnceWith(
                new EnsureAuthRetryPolicy([1000, 2000, 5000], 20000),
            );
            expect(
                hubBuilderInstanceMock.configureLogging,
            ).toHaveBeenCalledExactlyOnceWith(LogLevel.Information);

            expect(hubBuilderInstanceMock.build).toHaveBeenCalledOnce();
            expect(hubBuilderMock).toHaveBeenCalledOnce();
        });

        it("should return the existing hub if it exists", async () => {
            const { mockConnection } = mockHubConnection();
            const hubBuilderInstanceMock = mockHubBuilder(mockConnection);

            const { joinHub } = renderSignalRStore();

            act(() => joinHub(url));
            await flushMicrotasks();
            expect(useSignalRStore.getState().hubs.get(url)?.connection).toBe(
                mockConnection,
            );

            const otherHub = mock<HubConnection>();
            hubBuilderInstanceMock.build.mockReturnValue(otherHub);

            act(() => joinHub(url));
            expect(useSignalRStore.getState().hubs.get(url)?.connection).toBe(
                mockConnection,
            );
        });

        it("should update state to Connected on start", async () => {
            const { mockConnection } = mockHubConnection();
            mockHubBuilder(mockConnection);

            const { joinHub } = renderSignalRStore();
            act(() => joinHub(url));
            await flushMicrotasks();

            const hub = useSignalRStore.getState().hubs.get(url);
            expect(hub?.state).toBe(HubConnectionState.Connected);
        });

        it("should update state to Disconnected on onclose", async () => {
            const { mockConnection, handlers } = mockHubConnection(
                HubConnectionState.Connected,
            );
            mockHubBuilder(mockConnection);

            const { joinHub } = renderSignalRStore();
            act(() => joinHub(url));
            await flushMicrotasks();

            act(() => handlers.onCloseHandler?.());
            const hub = useSignalRStore.getState().hubs.get(url);
            expect(hub?.state).toBe(HubConnectionState.Disconnected);
        });

        it("should update state to Connected on onreconnected", async () => {
            const { mockConnection, handlers } = mockHubConnection(
                HubConnectionState.Disconnected,
            );
            mockHubBuilder(mockConnection);

            const { joinHub } = renderSignalRStore();
            act(() => joinHub(url));
            await flushMicrotasks();

            act(() => handlers.onReconnectedHandler?.());
            const hub = useSignalRStore.getState().hubs.get(url);
            expect(hub?.state).toBe(HubConnectionState.Connected);
        });

        it("should call ensureAuth if missing auth tokens", async () => {
            const { mockConnection } = mockHubConnection();
            mockHubBuilder(mockConnection);

            mockConnection.start
                .mockRejectedValueOnce(
                    new Error(
                        `{"errorCode":"${ErrorCode.AUTH_TOKEN_MISSING}"}`,
                    ),
                )
                .mockResolvedValueOnce(undefined);

            const { joinHub } = renderSignalRStore();
            act(() => joinHub(url));

            await flushMicrotasks();
            expect(mockConnection.start).toHaveBeenCalledTimes(1);
            expect(ensureAuthMock).toHaveBeenCalledOnce();

            vi.advanceTimersByTime(2000);
            await flushMicrotasks();

            expect(mockConnection.start).toHaveBeenCalledTimes(2);
        });

        it("should keep retrying indefinitely on persistent failure", async () => {
            const { mockConnection } = mockHubConnection();
            mockHubBuilder(mockConnection);

            mockConnection.start.mockRejectedValue(
                new Error("Server unavailable"),
            );

            const { joinHub } = renderSignalRStore();
            act(() => joinHub(url));

            for (let i = 0; i < 3; i++) {
                await flushMicrotasks();
                vi.advanceTimersByTime(2000);
            }

            expect(mockConnection.start).toHaveBeenCalledTimes(4);
        });
    });

    describe("dereferenceHub", () => {
        it("should decrement referenceCount if more than one reference exists", async () => {
            const { mockConnection } = mockHubConnection();
            mockHubBuilder(mockConnection);

            const { joinHub, dereferenceHub } = renderSignalRStore();

            act(() => joinHub(url));
            await flushMicrotasks();

            act(() => joinHub(url));
            const hubBefore = useSignalRStore.getState().hubs.get(url);
            expect(hubBefore?.referenceCount).toBe(2);

            act(() => dereferenceHub(url));
            const hubAfter = useSignalRStore.getState().hubs.get(url);
            expect(hubAfter?.referenceCount).toBe(1);
        });

        it("should stop and delete the hub if referenceCount is 1 and state is Connected", async () => {
            const { mockConnection } = mockHubConnection();
            mockHubBuilder(mockConnection);

            const { joinHub, dereferenceHub } = renderSignalRStore();

            act(() => joinHub(url));
            await flushMicrotasks();

            act(() => dereferenceHub(url));
            const hub = useSignalRStore.getState().hubs.get(url);
            expect(hub).toBeUndefined();
            expect(mockConnection.stop).toHaveBeenCalledOnce();
        });

        it("should not delete the hub if state is not Connected", async () => {
            const url = "test-url";
            const { mockConnection, handlers } = mockHubConnection();
            mockHubBuilder(mockConnection);

            const { joinHub, dereferenceHub } = renderSignalRStore();

            act(() => joinHub(url));
            await flushMicrotasks();
            act(() => handlers.onCloseHandler?.());

            act(() => dereferenceHub(url));
            const hub = useSignalRStore.getState().hubs.get(url);
            expect(hub).toBeDefined();
            expect(mockConnection.stop).not.toHaveBeenCalled();
            expect(hub?.referenceCount).toBe(0);
        });
    });
});
