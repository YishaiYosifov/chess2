import { XMarkIcon } from "@heroicons/react/24/solid";
import { ScaleIcon } from "@heroicons/react/24/solid";

import GameControlButton from "./GameControlButton";
import useLiveChessStore from "../../hooks/useLiveChessStore";
import { useGameEmitter } from "../../hooks/useGameHub";

const DrawCard = () => {
    const gameToken = useLiveChessStore((x) => x.gameToken);
    const sendGameEvent = useGameEmitter(gameToken);

    return (
        <div
            className="animate-flash-once flex w-full items-center justify-center gap-3 rounded-md
                text-lg"
        >
            Accept Draw?
            <div className="flex">
                <GameControlButton
                    icon={XMarkIcon}
                    className="text-red-600"
                    onClick={() => sendGameEvent("DeclineDrawAsync", gameToken)}
                />
                <GameControlButton
                    icon={ScaleIcon}
                    className="text-green-600"
                    onClick={() => sendGameEvent("RequestDrawAsync", gameToken)}
                />
            </div>
        </div>
    );
};
export default DrawCard;
