import MoveHistoryTable from "./MoveHistoryTable";
import GameChat from "./GameChat";
import GameControls from "./GameControls";

const ChessboardSide = () => {
    return (
        <div
            className="grid h-full w-full min-w-xs grid-rows-[3fr_0.3fr_1fr] gap-5 overflow-auto
                lg:max-w-xs"
        >
            <MoveHistoryTable />
            <GameControls />
            <GameChat />
        </div>
    );
};
export default ChessboardSide;
