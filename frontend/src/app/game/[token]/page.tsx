import { notFound, redirect } from "next/navigation";

import { parseFen } from "@/lib/utils/chessUtils";
import { ResponseError } from "@/apiClient";
import { liveGameApi } from "@/lib/apis";
import { Color, LiveGame, PrivateAuthedProfileOut } from "@/models";

import Chessboard from "@/components/game/Chessboard";
import withAuth from "@/hocs/withAuth";

export const metadata = { title: "Live Game - Chess 2" };

const GamePage = async ({
    params: { token },
    profile,
}: {
    params: { token: string };
    profile: PrivateAuthedProfileOut;
}) => {
    let game: LiveGame;
    try {
        game = await liveGameApi.getLiveGame(token);
    } catch (err) {
        if (!(err instanceof ResponseError)) throw err;

        if (err.response.status == 404) notFound();
        throw err;
    }

    // if the user is not participating in the game, they shouldn't be here
    // TODO: allow spectating
    if (
        profile.userId != game.playerWhite.user.userId &&
        profile.userId != game.playerBlack.user.userId
    )
        redirect("/");

    const playingAs =
        game.playerWhite.user.userId == profile.userId
            ? game.playerWhite
            : game.playerBlack;

    const playingSide =
        game.playerWhite.playerId == game.turnPlayerId
            ? Color.White
            : Color.Black;

    return (
        <Chessboard
            startingPieces={parseFen(game.fen)}
            playingAs={playingAs}
            playingSide={playingSide}
        />
    );
};
export default withAuth(GamePage);
