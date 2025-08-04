import { ChessboardStore } from "@/features/chessboard/stores/chessboardStore";
import { usePathname } from "next/navigation";
import { useEffect, useRef } from "react";
import { StoreApi } from "zustand";
import { LiveChessStore } from "../stores/liveChessStore";
import { refetchGame } from "../lib/gameStateProcessor";

export default function useBackNavigationRefetch(
    chessboardStore: StoreApi<ChessboardStore>,
    liveChessStore: StoreApi<LiveChessStore>,
) {
    const pathname = usePathname();
    const lastPathname = useRef<string>(null);

    useEffect(() => {
        if (pathname === lastPathname.current) {
            lastPathname.current = null;
            refetchGame(liveChessStore, chessboardStore);
        } else {
            lastPathname.current = pathname;
        }
    }, [pathname, chessboardStore, liveChessStore]);
}
