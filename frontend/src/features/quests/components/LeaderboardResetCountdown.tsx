import CountdownText from "@/components/CountdownText";

const LeaderboardResetCountdown = () => {
    return (
        <CountdownText
            getTimeUntil={() => {
                const now = new Date();
                return new Date(
                    Date.UTC(now.getUTCFullYear(), now.getUTCMonth() + 1),
                );
            }}
        >
            {({ countdown }) => (
                <p
                    data-testid="leaderboardResetCountdown"
                    className="text-text/70"
                >
                    Leaderboard resets in {countdown}
                </p>
            )}
        </CountdownText>
    );
};
export default LeaderboardResetCountdown;
