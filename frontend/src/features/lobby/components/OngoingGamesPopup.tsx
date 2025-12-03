import { forwardRef, ForwardRefRenderFunction } from "react";
import { shallow } from "zustand/shallow";

import Popup, { PopupRef } from "@/components/Popup";
import useLobbyStore from "../stores/lobbyStore";
import OngoingGameItem from "./OngoingGameItem";

const OngoingGamesPopup: ForwardRefRenderFunction<PopupRef, unknown> = (
    _,
    ref,
) => {
    const ongoingGames = useLobbyStore(
        (x) => [...x.ongoingGames.values()],
        shallow,
    );

    return (
        <Popup ref={ref} data-testid="ongoingGamesPopup">
            <h2 className="text-center text-3xl font-bold">Ongoing Games</h2>
            {ongoingGames.map((game) => (
                <OngoingGameItem key={game.gameToken} game={game} />
            ))}
        </Popup>
    );
};
export default forwardRef(OngoingGamesPopup);
