import { renderHook } from "@testing-library/react";

import useSignalRStore from "../signalRStore";
import { mockSignalRConnectionBuilder } from "@/lib/testUtils/mocks";
import { LogLevel } from "@microsoft/signalr";

vi.mock("@microsoft/signalr");

describe("signalRStore getOrJoinHub", () => {
    it("should create an hub if it doesn't exist", () => {
        const builderMock = mockSignalRConnectionBuilder();
        const url = "test-url";
        const mockResult = vi.fn();
        builderMock.build.mockReturnValue(mockResult);

        const {
            result: {
                current: { getOrJoinHub },
            },
        } = renderHook(() => useSignalRStore());
        const result = getOrJoinHub(url);

        expect(result).toBe(mockResult);

        expect(builderMock.withUrl).toHaveBeenCalledExactlyOnceWith(url);
        expect(builderMock.withAutomaticReconnect).toHaveBeenCalledOnce();
        expect(builderMock.configureLogging).toHaveBeenCalledExactlyOnceWith(
            LogLevel.Information,
        );

        expect(builderMock.build).toHaveBeenCalledOnce();
        expect(builderMock.build).toHaveBeenCalledAfter(builderMock.withUrl);
    });
});
