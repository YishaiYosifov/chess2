import { BsPlusSlashMinus, BsPlusSquare, BsDashSquare } from "react-icons/bs";
import Image from "next/image";
import Link from "next/link";

import styles from "./Game.module.scss";
import { Game } from "@/lib/types";

const GameRow = ({ username, game }: { username: string; game: Game }) => {
    const isDraw = game.black.score === game.white.score;
    const isWinner =
        (game.white.username === username && game.white.score === 1) ||
        (game.black.username === username && game.black.score === 1);

    let ResultsIcon;
    if (isDraw) ResultsIcon = BsPlusSlashMinus;
    else if (isWinner) ResultsIcon = BsPlusSquare;
    else ResultsIcon = BsDashSquare;

    const GameLink = () => (
        <Link href={`/game/${game.token}`} className={styles["game-link"]} />
    );

    const endedAt = new Date(game.createdAt);
    const formattedDate = endedAt.toLocaleDateString("en-us", {
        month: "short",
        day: "numeric",
        year: "2-digit",
    });

    return (
        <tr className={`${styles.game}`}>
            <td className="col-2">
                <GameLink />
                <Image
                    className="img-fluid mt-2 border border-3 rounded-circle"
                    src={`/assets/modes/${game.variant}.webp`}
                    width={60}
                    height={60}
                    alt="Mode"
                />
            </td>

            <td className="col-6">
                <div className={styles["users-column"]}>
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
                <GameLink />
            </td>

            <td className="col-2">
                <GameLink />
                <div className={styles["results-column"]}>
                    <div>
                        <span>{game.white.score}</span>
                        <span>-</span>
                        <span>{game.black.score}</span>
                    </div>
                    <ResultsIcon />
                </div>
            </td>

            <td className="col-2 text-wrap">
                <GameLink />

                <div className={styles["date-column"]}>
                    <span>{formattedDate}</span>
                </div>
            </td>
        </tr>
    );
};
export default GameRow;
