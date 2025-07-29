import withSession from "@/features/auth/hocs/withSession";
import { notFound, redirect } from "next/navigation";
import { getGame, SessionUser } from "@/lib/apiClient";
import LiveChessboard from "@/features/liveGame/components/LiveChessboard";

export const metadata = { title: "Live Game - Chess 2" };

const GamePage = async ({
    params,
    user,
    accessToken,
}: {
    params: Promise<{ gameToken: string }>;
    user: SessionUser;
    accessToken: string;
}) => {
    const { gameToken } = await params;

    const { error, data: game } = await getGame({
        path: { gameToken },
        auth: () => accessToken,
    });

    if (error || !game) {
        console.warn(error);
        notFound();
    }

    // if the user is not participating in the game, they shouldn't be here
    // TODO: allow spectating
    if (
        user.userId != game.whitePlayer.userId &&
        user.userId != game.blackPlayer.userId
    )
        redirect("/");

    return <LiveChessboard gameToken={gameToken} gameState={game} />;
};

export default withSession(GamePage);
