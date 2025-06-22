import MoveHistoryTable from "./MoveHistoryTable";
import Card from "../helpers/Card";
import GameChat from "./GameChat";

const ChessboardSide = () => {
    return (
        <div className="grid h-full w-full min-w-xs grid-rows-[3fr_1fr] gap-5 overflow-auto lg:max-w-xs">
            <MoveHistoryTable />
            <GameChat />
        </div>
    );
};
export default ChessboardSide;
