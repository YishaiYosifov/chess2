import classes from "./GamesTable.module.scss";
import GameRow from "./GameRow";

const GamesTable = ({ games, username }) => {
    return (
        <>
            <div
                className={`col-11 col-lg-10 col-xl-8 table-responsive ${classes["games-table"]}`}
            >
                <table className={`table text-light`}>
                    <thead>
                        <tr className="text-white-50 text-start">
                            <th scope="col">Mode</th>
                            <th scope="col">Players</th>
                            <th scope="col">Results</th>
                            <th scope="col">Date</th>
                        </tr>
                    </thead>
                    <tbody>
                        {!games.length && (
                            <tr className="col-11 text-start text-secondary">
                                <td>
                                    <div className={classes["empty-td"]}></div>
                                </td>
                                <td>
                                    <div className={classes["empty-td"]}></div>
                                </td>
                                <td>
                                    <div className={classes["empty-td"]}></div>
                                </td>
                                <td>
                                    <div className={classes["empty-td"]}></div>
                                </td>
                            </tr>
                        )}
                        {games.length
                            ? games.map((game) => (
                                  <GameRow game={game} username={username} />
                              ))
                            : ""}
                    </tbody>
                </table>
            </div>

            {!games.length && (
                <h4 className="text-danger text-center">
                    This user hasn't played any games yet!
                </h4>
            )}
        </>
    );
};
export default GamesTable;
