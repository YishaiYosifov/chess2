import { MinimalProfile } from "@/lib/apiClient";
import clsx from "clsx";
import MinimalProfileView from "./MinimalProfileView";

const LeaderboardMinimalProfileView = ({
    profile,
    page,
    pageSize,
    index,
    children,
}: {
    profile: MinimalProfile;
    page: number;
    pageSize: number;
    index: number;
    children?: React.ReactNode;
}) => {
    const overallPosition = page * pageSize + index + 1;

    const podiumColors = ["bg-amber-400", "bg-slate-300", "bg-orange-400"];
    const podiumIcon = ["🥇", "🥈", "🥉"];
    const colorClass =
        overallPosition <= podiumColors.length
            ? podiumColors[overallPosition - 1]
            : "bg-text/70";
    const rankDisplay =
        overallPosition <= podiumIcon.length
            ? podiumIcon[overallPosition - 1]
            : `#${overallPosition}`;

    return (
        <>
            <span
                className={clsx(
                    "flex h-full items-center justify-center rounded-md p-2 text-black",
                    colorClass,
                )}
                data-testid={`leaderboardItem-${profile.userId}`}
            >
                {rankDisplay}
            </span>
            <MinimalProfileView profile={profile} index={index}>
                {children}
            </MinimalProfileView>
        </>
    );
};
export default LeaderboardMinimalProfileView;
