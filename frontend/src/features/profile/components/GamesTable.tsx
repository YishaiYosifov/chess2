"use client";

import { twMerge } from "tailwind-merge";
import GameRow from "./GameRow";
import { FinishedGame } from "@/types/tempModels";
import { User } from "@/lib/apiClient";

const GamesTable = ({
    games,
    profileViewpoint,
    className,
}: {
    games: FinishedGame[];
    profileViewpoint: User;
    className?: string;
}) => {
    return (
        <table className={twMerge("h-max w-full table-auto", className)}>
            <colgroup>
                <col style={{ width: "50%" }} />
                <col style={{ width: "30%" }} />
                <col style={{ width: "20%" }} />
            </colgroup>
            <thead className="bg-card text-xl">
                <tr data-testid="gamesTableHeader">
                    <th scope="col" className="rounded-l-md p-4 text-start">
                        Players
                    </th>
                    <th scope="col" className="p-3 text-start">
                        Results
                    </th>
                    <th scope="col" className="rounded-r-md p-3 text-start">
                        Date
                    </th>
                </tr>
            </thead>
            <tbody className="text-xl">
                {games.length ? (
                    games.map((game, i) => (
                        <GameRow
                            key={game.token}
                            game={game}
                            profileViewpoint={profileViewpoint}
                            index={i}
                        />
                    ))
                ) : (
                    <tr className="bg-gray-400/5">
                        <td
                            colSpan={3}
                            className="p-5 text-center"
                            data-testid="noGamesRow"
                        >
                            This user hasn&#39;t played any games yet
                        </td>
                    </tr>
                )}
            </tbody>
        </table>
    );
};
export default GamesTable;
