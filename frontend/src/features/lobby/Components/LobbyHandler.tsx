"use client";

import { useLobbyEvent } from "@/features/signalR/hooks/useSignalRHubs";
import constants from "@/lib/constants";
import { useRouter } from "next/navigation";

const LobbyHandler = () => {
    const router = useRouter();

    useLobbyEvent("MatchFoundAsync", (token) =>
        router.push(`${constants.PATHS.GAME}/${token}`),
    );
    return null;
};
export default LobbyHandler;
