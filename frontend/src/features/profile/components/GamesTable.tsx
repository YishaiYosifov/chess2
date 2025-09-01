"use client";

import Card from "@/components/ui/Card";
import GameRow from "./GameRow";
import { GameSummary, PublicUser } from "@/lib/apiClient";

const GamesTable = ({
    games,
    profileViewpoint,
}: {
    games: GameSummary[];
    profileViewpoint: PublicUser;
}) => {
    return (
        <Card className="overflow-x-auto p-0.5">
            <table className="h-min table-auto overflow-x-auto">
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
                                key={game.gameToken}
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
        </Card>
    );
};
export default GamesTable;
