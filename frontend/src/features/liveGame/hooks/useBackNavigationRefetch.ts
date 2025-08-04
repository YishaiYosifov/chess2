import { ChessboardStore } from "@/features/chessboard/stores/chessboardStore";
import { usePathname } from "next/navigation";
import { useEffect } from "react";
import { StoreApi } from "zustand";
import { LiveChessStore } from "../stores/liveChessStore";
import { refetchGame } from "../lib/gameStateProcessor";

let lastPathname: string | null = null;

export default function useBackNavigationRefetch(
    chessboardStore: StoreApi<ChessboardStore>,
    liveChessStore: StoreApi<LiveChessStore>,
) {
    const pathname = usePathname();

    useEffect(() => {
        const resultData = liveChessStore.getState().resultData;
        if (pathname === lastPathname && !resultData) {
            refetchGame(liveChessStore, chessboardStore);
        } else {
            lastPathname = pathname;
        }
    }, [pathname, liveChessStore, chessboardStore]);
}
