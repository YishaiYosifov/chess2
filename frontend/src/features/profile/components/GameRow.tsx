import {
    PlusCircleIcon,
    MinusCircleIcon,
    PauseCircleIcon,
} from "@heroicons/react/24/outline";

import Link from "next/link";

import clsx from "clsx";
import { GameResult, GameSummary, PublicUser } from "@/lib/apiClient";

const GameRow = ({
    game,
    profileViewpoint,
    index,
}: {
    game: GameSummary;
    profileViewpoint: PublicUser;
    index: number;
}) => {
    const winCondition =
        game.whitePlayer.userId == profileViewpoint.userId
            ? GameResult.WHITE_WIN
            : GameResult.BLACK_WIN;

    const isDraw = game.result === GameResult.DRAW;
    const isWinner = game.result === winCondition;

    const GameLink = () => (
        <Link
            data-testid="gameRowLink"
            className="absolute top-0 right-0 left-0"
            href={`/game/${game.gameToken}`}
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

    function getScore(winResult: GameResult): string {
        if (isDraw) return "½";

        return game.result == winResult ? "1" : "0";
    }

    const whiteUsername = game.whitePlayer.userName;
    const blackUsername = game.blackPlayer.userName;

    return (
        <tr
            data-testid={`gameRow-${game.gameToken}`}
            className={clsx(
                index % 2 == 0 ? "bg-gray-400/5" : "bg-gray-600/5",
                "whitespace-nowrap",
            )}
        >
            <td className="relative p-4">
                <GameLink />
                <div className="flex flex-col justify-between">
                    <Link
                        href={`/profile/${whiteUsername}`}
                        data-testid="gameRowWhiteUsername"
                    >
                        {whiteUsername}
                    </Link>
                    <Link
                        href={`/profile/${blackUsername}`}
                        className="text-white/50"
                        data-testid="gameRowBlackUsername"
                    >
                        {blackUsername}
                    </Link>
                </div>
            </td>

            <td className="relative p-4">
                <GameLink />
                <div className="flex items-center gap-3">
                    <div className="flex w-3 flex-col justify-between">
                        <span data-testid="gameRowScoreWhite">
                            {getScore(GameResult.WHITE_WIN)}
                        </span>

                        <span data-testid="gameRowScoreBlack">
                            {getScore(GameResult.BLACK_WIN)}
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
