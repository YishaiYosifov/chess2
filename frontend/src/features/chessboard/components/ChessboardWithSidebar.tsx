import React from "react";

const ChessboardWithSidebar = ({
    chessboard,
    aside,
}: {
    chessboard: React.ReactNode;
    aside: React.ReactNode;
}) => {
    return (
        <div
            className="flex w-full flex-col items-center justify-center gap-5 p-5 lg:flex-row
                lg:items-start"
        >
            <section className="flex h-max w-fit flex-col gap-3">
                {chessboard}
            </section>
            {aside}
        </div>
    );
};
export default ChessboardWithSidebar;
