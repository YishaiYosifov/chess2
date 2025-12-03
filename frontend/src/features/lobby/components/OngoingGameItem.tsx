import { useRouter } from "next/navigation";
import { OngoingGame } from "../lib/types";
import constants from "@/lib/constants";
import TimeControlIcon from "./TimeControlIcon";
import { PoolType } from "@/lib/apiClient";

const OngoingGameItem = ({ game }: { game: OngoingGame }) => {
    const router = useRouter();

    const redirectToGame = () =>
        router.push(`${constants.PATHS.GAME}/${game.gameToken}`);

    return (
        <div
            className="hover:bg-primary flex cursor-pointer rounded-md p-3"
            onClick={redirectToGame}
            data-testid={`ongoingGameItem-${game.gameToken}`}
        >
            <div className="flex w-20 flex-col items-center">
                <TimeControlIcon
                    className="h-10 w-10"
                    timeControl={game.pool.timeControl.type}
                />
                <span className="text-2xl" data-testid="ongoingGameTimeControl">
                    {game.pool.timeControl.baseSeconds / 60}+
                    {game.pool.timeControl.incrementSeconds}
                </span>
            </div>

            <div className="flex flex-col justify-center">
                <p
                    className="truncate text-xl"
                    data-testid="ongoingGameUsername"
                >
                    {game.opponent.userName}
                </p>
                <p
                    className="text-text/50 text-sm"
                    data-testid="ongoingGamePoolType"
                >
                    {game.pool.poolType === PoolType.RATED ? `rated` : "casual"}
                </p>
            </div>
        </div>
    );
};
export default OngoingGameItem;
