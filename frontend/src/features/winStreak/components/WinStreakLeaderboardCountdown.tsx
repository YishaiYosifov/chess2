import CountdownText from "@/components/CountdownText";

const LEADERBOARD_LOCKS_IN = new Date(2025, 11, 14);

const WinStreakLeaderboardCountdown = () => {
    return (
        <CountdownText getTimeUntil={() => LEADERBOARD_LOCKS_IN}>
            {({ countdown }) => (
                <p
                    data-testid="winStreakLeaderboardCountdown"
                    className="text-text/70 text-lg"
                >
                    Leaderboard locks in {countdown}
                </p>
            )}
        </CountdownText>
    );
};
export default WinStreakLeaderboardCountdown;
