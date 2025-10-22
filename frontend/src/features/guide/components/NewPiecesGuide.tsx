import knook1 from "@public/assets/examples/knook1.png";
import checker1 from "@public/assets/examples/checker1.png";
import checker2 from "@public/assets/examples/checker2.png";
import checker3 from "@public/assets/examples/checker3.png";
import underagepawn1 from "@public/assets/examples/underagepawn1.png";
import underagepawn2 from "@public/assets/examples/underagepawn2.png";
import traitorrook1 from "@public/assets/examples/traitorrook1.png";
import traitorrook2 from "@public/assets/examples/traitorrook2.png";
import traitorrook3 from "@public/assets/examples/traitorrook3.png";
import antiqueen1 from "@public/assets/examples/antiqueen1.png";

import Card from "@/components/ui/Card";
import GuideCard from "./GuideCard";

const NewPiecesGuide = () => {
    return (
        <Card className="gap-5 p-5">
            <h2 className="text-6xl">New Pieces</h2>

            <hr className="text-secondary/50" />

            <GuideCard
                title="Knook"
                points={[
                    "Moves like a knight.",
                    "Can also move like a rook for up to 2 squares in any direction.",
                ]}
                images={[knook1]}
            />

            <GuideCard
                title="Checker"
                points={[
                    "Moves up to two squares diagonally in any direction.",
                    "Can hop over any piece (friendly or enemy) and chain multiple hops like in checkers.",
                    "Captures are never forced, and hopping over friendly pieces doesn't capture them.",
                    "Reaching the back rank promotes it into a king, giving you an extra life.",
                ]}
                images={[checker1, checker2, checker3]}
            />

            <GuideCard
                title="Underage Pawn"
                points={[
                    "Moves like a normal pawn.",
                    "First move = 2 squares (unlike standard pawns which now move 3).",
                    "If seen by a bishop, that bishop MUST capture it. Even if it's your own bishop.",
                ]}
                images={[underagepawn1, underagepawn2]}
            />

            <GuideCard
                title="Traitor Rook (Neutral Piece)"
                points={[
                    "Moves like a regular rook.",
                    "Control by whichever side has more ADJACENT pieces (NOT overall pieces on the board, just adjacent ones).",
                    "If tied, both sides can move it but neither can capture with it.",
                ]}
                images={[traitorrook1, traitorrook2, traitorrook3]}
            />

            <GuideCard
                title="Antiqueen"
                points={[
                    "Moves anywhere the queen cannot within a 5x5 area.",
                    "Which, by definition means... it move like a knight.",
                ]}
                images={[antiqueen1]}
            />
        </Card>
    );
};
export default NewPiecesGuide;
