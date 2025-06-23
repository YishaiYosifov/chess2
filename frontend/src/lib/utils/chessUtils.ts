import { GameColor } from "../apiClient";

export function invertColor(color: GameColor): GameColor {
    return color === GameColor.WHITE ? GameColor.BLACK : GameColor.WHITE;
}
