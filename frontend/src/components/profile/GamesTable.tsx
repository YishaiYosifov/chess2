"use client";

import { Table } from "react-bootstrap";

import type { FinishedGame, PublicUserOut } from "@/client";
import styles from "./GamesTable.module.scss";

import GameRow from "./GameRow";

const GamesTable = ({
    games,
    viewingProfile,
}: {
    games: FinishedGame[];
    viewingProfile: PublicUserOut;
}) => {
    const MAX_EMPTY_ROWS = 3;

    return (
        <Table responsive className={styles["games-table"]}>
            <colgroup>
                <col style={{ width: "15%" }} />
                <col style={{ width: "45%" }} />
                <col style={{ width: "20%" }} />
                <col style={{ width: "13%" }} />
            </colgroup>
            <thead>
                <tr
                    className="text-white-50 text-start"
                    data-testid="gamesTableHeader"
                >
                    <th scope="col">Mode</th>
                    <th scope="col">Players</th>
                    <th scope="col">Results</th>
                    <th scope="col">Date</th>
                </tr>
            </thead>
            <tbody>
                {games.map((game) => (
                    <GameRow
                        key={game.token}
                        game={game}
                        viewingProfile={viewingProfile}
                    />
                ))}

                {games.length < MAX_EMPTY_ROWS &&
                    [...Array(MAX_EMPTY_ROWS - games.length).keys()].map(
                        (i) => <EmptyRow key={i} />
                    )}
            </tbody>
        </Table>
    );
};
export default GamesTable;

const EmptyRow = () => (
    <tr data-testid="emptyGamesTableRow">
        <td>
            <div className={styles["empty-td"]} />
        </td>
        <td>
            <div className={styles["empty-td"]} />
        </td>
        <td>
            <div className={styles["empty-td"]} />
        </td>
        <td>
            <div className={styles["empty-td"]} />
        </td>
    </tr>
);
