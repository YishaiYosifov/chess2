import {
    PlusCircleIcon,
    MinusCircleIcon,
    PauseCircleIcon,
} from "@heroicons/react/24/outline";

import Link from "next/link";

import {
    type User,
    type FinishedGame,
    GameResult,
} from "@/lib/apiClient/models";

import { Color } from "@/lib/apiClient/models";
import clsx from "clsx";

const GameRow = ({
    game,
    profileViewpoint,
    index,
}: {
    game: FinishedGame;
    profileViewpoint: User;
    index: number;
}) => {
    const color =
        game.userWhite?.userId == profileViewpoint.userId
            ? Color.White
            : Color.Black;

    const isDraw = game.results === GameResult.Draw;
    const isWinner = color.valueOf() === game.results;

    const GameLink = () => (
        <Link
            data-testid="gameRowLink"
            className="absolute left-0 right-0 top-0"
            href={`/game/${game.token}`}
        />
    );

    // Format the game date
    const formattedDate = new Date(game.createdAt).toLocaleDateString("en-us", {
        month: "short",
        day: "numeric",
        year: "numeric",
    });

    // Find the results icon (whether the profile author is the winner or it's a draw)
    // and find the score for each color
    const ResultsIcon = isDraw ? (
        <PauseCircleIcon className="text-gray-500" />
    ) : isWinner ? (
        <PlusCircleIcon className="text-green-400" />
    ) : (
        <MinusCircleIcon className="text-red-400" />
    );

    function getScore(color: Color): string {
        if (isDraw) return "½";
        return game.results == color.valueOf() ? "1" : "0";
    }

    const usernameWhite = game.userWhite?.username ?? "DELETED";
    const usernameBlack = game.userBlack?.username ?? "DELETED";

    return (
        <tr
            data-testid="gameRow"
            className={clsx(
                index % 2 == 0 ? "bg-gray-400/5" : "bg-gray-600/5",
                "whitespace-nowrap",
            )}
        >
            <td className="relative p-4">
                <GameLink />
                <div className="flex flex-col justify-between">
                    <Link
                        href={`/profile/${usernameWhite}`}
                        data-testid="gameRowUsernameWhite"
                    >
                        {usernameWhite}
                    </Link>
                    <Link
                        href={`/profile/${usernameBlack}`}
                        className="text-white/50"
                        data-testid="gameRowUsernameBlack"
                    >
                        {usernameBlack}
                    </Link>
                </div>
            </td>

            <td className="relative p-4">
                <GameLink />
                <div className="flex items-center gap-3">
                    <div className="flex w-3 flex-col justify-between">
                        <span data-testid="gameRowScoreWhite">
                            {getScore(Color.White)}
                        </span>

                        <span data-testid="gameRowScoreBlack">
                            {getScore(Color.Black)}
                        </span>
                    </div>
                    <span className="size-7">{ResultsIcon}</span>
                </div>
            </td>

            <td className="relative p-4">
                <GameLink />

                <div>
                    <span data-testid="gameRowDate">{formattedDate}</span>
                </div>
            </td>
        </tr>
    );
};
export default GameRow;
