import { ArrowPathIcon, PlusIcon } from "@heroicons/react/24/solid";

import GameControlButton from "./GameControlButton";

const GameOverControls = () => (
    <>
        <GameControlButton icon={PlusIcon}>New Game</GameControlButton>
        <GameControlButton icon={ArrowPathIcon}>Rematch</GameControlButton>
    </>
);
export default GameOverControls;
