"use client";

import type { FinishedGame, User } from "@/lib/apiClient/models";

import GameRow from "./GameRow";

const GamesTable = ({
    games,
    profileViewpoint,
}: {
    games: FinishedGame[];
    profileViewpoint: User;
}) => {
    return (
        <section className="w-full overflow-x-auto">
            <table className="w-full table-auto">
                <colgroup>
                    <col style={{ width: "50%" }} />
                    <col style={{ width: "30%" }} />
                    <col style={{ width: "20%" }} />
                </colgroup>
                <thead className="bg-card text-xl">
                    <tr data-testid="gamesTableHeader">
                        <th scope="col" className="rounded-l-md p-3 text-start">
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
                    {games.map((game, i) => (
                        <GameRow
                            key={game.token}
                            game={game}
                            profileViewpoint={profileViewpoint}
                            index={i}
                        />
                    ))}
                </tbody>
            </table>
        </section>
    );
};
export default GamesTable;
