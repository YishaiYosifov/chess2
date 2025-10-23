import React from "react";

const ChessboardWithSidebar = ({
    chessboard,
    aside,
}: {
    chessboard: React.ReactNode;
    aside: React.ReactNode;
}) => {
    return (
        <main
            className="flex min-w-0 flex-1 flex-col items-center justify-center gap-5 p-5
                lg:max-h-screen lg:flex-row lg:items-start"
        >
            <section className="flex h-max flex-col gap-3">
                {chessboard}
            </section>
            {aside}
        </main>
    );
};
export default ChessboardWithSidebar;
