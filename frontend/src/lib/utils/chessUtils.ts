import { Point, Color } from "@/components/game/chess.types";

export function position2Offset(
    position: Point,
    viewingFrom: Color,
    boardWidth: number,
    boardHeight: number
): [offsetX: number, offsetY: number] {
    const boardSize = boardWidth * boardHeight;
    let [x, y] = position;

    // flip the board if we are viewing from the black prespective
    if (viewingFrom == Color.Black) {
        x = boardWidth - x - 1;
        y = boardHeight - y - 1;
    }

    const physicalX = x * boardWidth * boardHeight;
    const physicalY = y * boardSize;
    return [physicalX, physicalY];
}
