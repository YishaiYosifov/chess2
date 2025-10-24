import ChessboardWithSidebar from "./ChessboardWithSidebar";
import StaticChessboard from "./StaticChessboard";

const StaticChessboardWithSidebar = ({ aside }: { aside: React.ReactNode }) => {
    return (
        <ChessboardWithSidebar
            chessboard={
                <StaticChessboard
                    breakpoints={[
                        {
                            maxScreenSize: 768,
                            paddingOffset: { width: 40, height: 110 },
                        },
                        {
                            maxScreenSize: 1024,
                            paddingOffset: { width: 200, height: 50 },
                        },
                    ]}
                    defaultOffset={{ width: 626, height: 40 }} // height gap-5 + p-5
                />
            }
            aside={aside}
        />
    );
};
export default StaticChessboardWithSidebar;
