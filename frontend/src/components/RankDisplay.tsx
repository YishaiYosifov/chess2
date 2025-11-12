import ProgressBar from "./ui/ProgressBar";

const RankDisplay = ({
    rank,
    totalPlayers,
}: {
    rank: number;
    totalPlayers: number;
}) => {
    const percentile =
        ((totalPlayers - Math.min(totalPlayers, rank)) / totalPlayers) * 100;
    return (
        <div className="my-auto w-full sm:w-auto" data-testid="rankDisplay">
            <h2 className="text-xl font-bold">Your Rank</h2>
            <div className="flex items-center gap-3">
                <p
                    className="text-2xl font-extrabold text-amber-400"
                    data-testid="rankDisplayNumber"
                >
                    {totalPlayers > 0 ? `#${rank}` : "-"}
                </p>
                <ProgressBar percent={percentile} />
            </div>

            <p
                className="text-text/70 text-sm text-nowrap"
                data-testid="rankDisplayPercentile"
            >
                {totalPlayers > 0
                    ? `That's top ${percentile.toFixed(1)}%!`
                    : "No players yet!"}
            </p>
        </div>
    );
};
export default RankDisplay;
