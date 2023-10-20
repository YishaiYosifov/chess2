import { BsPlusSlashMinus, BsPlusSquare, BsDashSquare } from "react-icons/bs";
import Image from "next/image";
import Link from "next/link";

import styles from "./GameRow.module.scss";

import type { Game, PublicProfile } from "@/lib/types";

const GameRow = ({
    game,
    viewingProfile,
}: {
    game: Game;
    viewingProfile: PublicProfile;
}) => {
    const isDraw = !game.winnerId;
    const isWinner = game.winnerId === viewingProfile.userId;

    const GameLink = () => (
        <Link href={`/game/${game.token}`} className={styles["game-link"]} />
    );

    // Format the game date
    const gameDate = new Date(game.createdAt);
    const formattedDate = gameDate.toLocaleDateString("en-us", {
        month: "short",
        day: "numeric",
        year: "2-digit",
    });

    // Find the results icon (whether the profile author is the winner or it's a draw)
    // and find the score for each player
    const ResultsIcon = isDraw
        ? BsPlusSlashMinus
        : isWinner
        ? BsPlusSquare
        : BsDashSquare;
    const getScore = (id: number): string =>
        isDraw ? "½" : game.winnerId === id ? "1" : "0";

    return (
        <tr className={styles.game}>
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
                        href={`/user/${game.white.username}`}
                        className="mt-2 limit-text"
                    >
                        {game.white.username}
                    </Link>
                    <Link
                        href={`/user/${game.black.username}`}
                        className="text-secondary limit-text"
                    >
                        {game.black.username}
                    </Link>
                </div>
            </td>

            <td>
                <GameLink />
                <div className={styles["results-column"]}>
                    <div>
                        <span>{getScore(game.white.userId)}</span>
                        <span>-</span>
                        <span>{getScore(game.black.userId)}</span>
                    </div>
                    <ResultsIcon />
                </div>
            </td>

            <td>
                <GameLink />

                <div className={styles["date-column"]}>
                    <span>{formattedDate}</span>
                </div>
            </td>
        </tr>
    );
};
export default GameRow;
