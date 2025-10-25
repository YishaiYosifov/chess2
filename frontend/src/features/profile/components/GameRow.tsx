import {
    PlusCircleIcon,
    MinusCircleIcon,
    PauseCircleIcon,
} from "@heroicons/react/24/outline";

import Link from "next/link";

import clsx from "clsx";
import { GameResult, GameSummary, PublicUser } from "@/lib/apiClient";
import ProfileTooltip from "./ProfileTooltip";
import { twMerge } from "tailwind-merge";

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
        game.whitePlayer.userId === profileViewpoint.userId
            ? GameResult.WHITE_WIN
            : GameResult.BLACK_WIN;

    const isDraw = game.result === GameResult.DRAW;
    const isWinner = game.result === winCondition;

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

        return game.result === winResult ? "1" : "0";
    }

    const GameLink = ({
        className,
        children,
    }: {
        className?: string;
        children?: React.ReactNode;
    }) => (
        <Link
            data-testid="gameRowLink"
            className={twMerge("flex flex-1 p-4", className)}
            href={`/game/${game.gameToken}`}
        >
            {children}
        </Link>
    );

    return (
        <tr
            data-testid={`gameRow-${game.gameToken}`}
            className={clsx(
                index % 2 === 0 ? "bg-gray-400/5" : "bg-gray-600/5",
                "whitespace-nowrap",
            )}
        >
            <td className="relative">
                <GameLink />
                <div className="absolute top-0 bottom-0 flex flex-col justify-center">
                    <ProfileTooltip
                        username={game.whitePlayer.userName}
                        userId={game.whitePlayer.userId}
                    >
                        <p data-testid="gameRowWhiteUsername">
                            {game.whitePlayer.userName}
                        </p>
                    </ProfileTooltip>
                    <ProfileTooltip
                        username={game.blackPlayer.userName}
                        userId={game.blackPlayer.userId}
                    >
                        <p
                            className="text-text/50"
                            data-testid="gameRowBlackUsername"
                        >
                            {game.blackPlayer.userName}
                        </p>
                    </ProfileTooltip>
                </div>
            </td>

            <td>
                <GameLink className="items-center gap-3">
                    <div className="flex w-3 flex-col justify-between">
                        <span data-testid="gameRowScoreWhite">
                            {getScore(GameResult.WHITE_WIN)}
                        </span>

                        <span data-testid="gameRowScoreBlack">
                            {getScore(GameResult.BLACK_WIN)}
                        </span>
                    </div>
                    <span className="size-7">{ResultsIcon}</span>
                </GameLink>
            </td>

            <td>
                <GameLink>
                    <span data-testid="gameRowDate">{formattedDate}</span>
                </GameLink>
            </td>
        </tr>
    );
};
export default GameRow;
