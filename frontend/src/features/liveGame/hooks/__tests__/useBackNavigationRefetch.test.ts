import { renderHook } from "@testing-library/react";
import { usePathname } from "next/navigation";
import { refetchGame } from "../../lib/gameStateProcessor";
import { StoreApi } from "zustand";
import {
    ChessboardStore,
    createChessboardStore,
} from "@/features/chessboard/stores/chessboardStore";
import createLiveChessStore, {
    LiveChessStore,
} from "../../stores/liveChessStore";
import { createFakeLiveChessStoreProps } from "@/lib/testUtils/fakers/liveChessStoreFaker";
import { GameResult } from "@/lib/apiClient";

vi.mock("next/navigation");
vi.mock("@/features/liveGame/lib/gameStateProcessor");

describe("useBackNavigationRefetch", () => {
    const usePathnameMock = vi.mocked(usePathname);
    const refetchGameMock = vi.mocked(refetchGame);

    let chessboardStore: StoreApi<ChessboardStore>;
    let liveChessStore: StoreApi<LiveChessStore>;
    let useBackNavigationRefetch: typeof import("../useBackNavigationRefetch").default;

    beforeEach(async () => {
        vi.resetModules();
        ({ default: useBackNavigationRefetch } = await import(
            "../useBackNavigationRefetch"
        ));

        chessboardStore = createChessboardStore();
        liveChessStore = createLiveChessStore(
            createFakeLiveChessStoreProps({ resultData: null }),
        );
    });

    function mountHookWithPath(path: string) {
        usePathnameMock.mockReturnValue(path);
        return renderHook(() =>
            useBackNavigationRefetch(chessboardStore, liveChessStore),
        );
    }

    it("should not call refetchGame on initial render", () => {
        mountHookWithPath("/game");

        expect(refetchGameMock).not.toHaveBeenCalled();
    });

    it("should not call refetchGame when navigating to a new page", () => {
        mountHookWithPath("/game").unmount();

        mountHookWithPath("/settings");

        expect(refetchGameMock).not.toHaveBeenCalled();
    });

    it("should call refetchGame when navigating back to the same page", () => {
        mountHookWithPath("/game").unmount();
        mountHookWithPath("/game");

        expect(refetchGameMock).toHaveBeenCalledTimes(1);
        expect(refetchGameMock).toHaveBeenCalledWith(
            liveChessStore,
            chessboardStore,
        );
    });

    it("should not call refetchGame twice on repeated remounts unless the path matches", () => {
        mountHookWithPath("/game").unmount();
        mountHookWithPath("/game").unmount();

        expect(refetchGameMock).toHaveBeenCalledTimes(1);

        mountHookWithPath("/something").unmount();

        expect(refetchGameMock).toHaveBeenCalledTimes(1);
    });

    it("should not call refetchGane if resultData is set", () => {
        mountHookWithPath("/game").unmount();
        liveChessStore.setState({
            resultData: {
                result: GameResult.WHITE_WIN,
                resultDescription: "desc",
            },
        });
        mountHookWithPath("/game").unmount();

        expect(refetchGameMock).not.toHaveBeenCalled();
    });
});
