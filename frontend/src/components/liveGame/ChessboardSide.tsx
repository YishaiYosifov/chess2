import MoveHistoryTable from "./MoveHistoryTable";
import GameChat from "./GameChat";
import GameControls from "./GameControls";

const ChessboardSide = () => {
    return (
        <aside
            className="grid h-full w-full min-w-xs grid-rows-[minmax(100px,3fr)_70px_200px] gap-3
                overflow-auto lg:max-w-xs"
        >
            <MoveHistoryTable />
            <GameControls />
            <GameChat />
        </aside>
    );
};
export default ChessboardSide;
