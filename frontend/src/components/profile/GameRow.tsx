import { BsPlusSlashMinus, BsPlus, BsDash } from "react-icons/bs";
import Image from "next/image";
import Link from "next/link";

import styles from "./GameRow.module.scss";

import type { AuthedProfileOut, FinishedGame } from "@/client";
import { Color } from "@/components/game/chess.types";
import { GameResult } from "@/client";

const GameRow = ({
    game,
    viewingProfile,
}: {
    game: FinishedGame;
    viewingProfile: AuthedProfileOut;
}) => {
    const color =
        game.userWhite?.userId == viewingProfile.userId
            ? Color.White
            : Color.Black;

    const isDraw = game.results === GameResult.Draw;
    const isWinner = color.valueOf() === game.results;

    const GameLink = () => (
        <Link
            data-testid="gameRowLink"
            href={`/game/${game.token}`}
            className={styles["game-link"]}
        />
    );

    // Format the game date
    const gameDate = new Date(game.createdAt);
    const formattedDate = gameDate.toLocaleDateString("en-us", {
        month: "short",
        day: "numeric",
        year: "2-digit",
    });

    // Find the results icon (whether the profile author is the winner or it's a draw)
    // and find the score for each color
    const ResultsIcon = isDraw ? BsPlusSlashMinus : isWinner ? BsPlus : BsDash;
    const getScore = (color: Color): string =>
        isDraw ? "½" : game.results == color.valueOf() ? "1" : "0";

    const usernameWhite = game.userWhite?.username ?? "DELETED";
    const usernameBlack = game.userBlack?.username ?? "DELETED";

    return (
        <tr className={styles.game} data-testid="gameRow">
            <td>
                <GameLink />
                <div className={styles["variant-icon"]}>
                    <Image
                        src={`/assets/modes/${game.variant}.webp`}
                        width={60}
                        height={60}
                        alt="Mode"
                    />
                </div>
            </td>

            <td>
                <GameLink />
                <div className={styles["players-column"]}>
                    <Link
                        href={`/user/${usernameWhite}`}
                        className="mt-2 limit-text"
                        data-testid="gameRowUsernameWhite"
                    >
                        {usernameWhite}
                    </Link>
                    <Link
                        href={`/user/${usernameBlack}`}
                        className="text-secondary limit-text"
                        data-testid="gameRowUsernameBlack"
                    >
                        {usernameBlack}
                    </Link>
                </div>
            </td>

            <td>
                <GameLink />
                <div className={styles["results-column"]}>
                    <div>
                        <span data-testid="gameRowScoreWhite">
                            {getScore(Color.White)}
                        </span>
                        <span>-</span>
                        <span data-testid="gameRowScoreBlack">
                            {getScore(Color.Black)}
                        </span>
                    </div>
                    <ResultsIcon />
                </div>
            </td>

            <td>
                <GameLink />

                <div className={styles["date-column"]}>
                    <span data-testid="gameRowDate">{formattedDate}</span>
                </div>
            </td>
        </tr>
    );
};
export default GameRow;
