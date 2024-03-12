import {
    Color,
    type ChessBoard,
    PieceType,
} from "@/components/game/chess.types";

export const USERNAME_EDIT_EVERY = 2419200;

export const BOARD_WIDTH = 10;
export const BOARD_HEIGHT = 10;
export const BOARD_SIZE = BOARD_WIDTH * BOARD_HEIGHT;

export const ACCESS_TOKEN_EXPIRES_SECONDS = 1800;
export const REFRESH_TOKEN_EXPIRES_SECONDS = 216000;

export const ACCESS_TOKEN = "access_token";
export const REFRESH_TOKEN = "refresh_token";

export const LAST_LOGIN_LOCAL_STORAGE = "lastLogin";
export const GENERIC_ERROR = "Something went wrong.";

export const SETTING_PAGES = [
    { name: "Profile", url: "profile" },
    { name: "Live Game", url: "live-game" },
    { name: "Blocked", url: "blocked" },
    { name: "Security", url: "security" },
];

export const TIME_CONTROLS = [
    { type: "bullet", timeControl: 60, increment: 0 },
    { type: "bullet", timeControl: 60, increment: 1 },
    { type: "bullet", timeControl: 120, increment: 1 },
    { type: "blitz", timeControl: 180, increment: 0 },
    { type: "blitz", timeControl: 180, increment: 2 },
    { type: "blits", timeControl: 300, increment: 0 },
    { type: "rapid", timeControl: 600, increment: 0 },
    { type: "rapid", timeControl: 900, increment: 10 },
    { type: "rapid", timeControl: 1800, increment: 0 },
];

export const defaultChessBoard: ChessBoard = [
    [[0, 0], { pieceType: PieceType.Rook, color: Color.Black }],
    [[1, 0], { pieceType: PieceType.Horsie, color: Color.Black }],
    [[2, 0], { pieceType: PieceType.Knook, color: Color.Black }],
    [[3, 0], { pieceType: PieceType.Xook, color: Color.Black }],
    [[4, 0], { pieceType: PieceType.King, color: Color.Black }],
    [[5, 0], { pieceType: PieceType.Queen, color: Color.Black }],
    [[6, 0], { pieceType: PieceType.Bishop, color: Color.Black }],
    [[7, 0], { pieceType: PieceType.Antiqueen, color: Color.Black }],
    [[8, 0], { pieceType: PieceType.Horsie, color: Color.Black }],
    [[9, 0], { pieceType: PieceType.Rook, color: Color.Black }],
    [[0, 1], { pieceType: PieceType.ChildPawn, color: Color.Black }],
    [[1, 1], { pieceType: PieceType.ChildPawn, color: Color.Black }],
    [[2, 1], { pieceType: PieceType.Pawn, color: Color.Black }],
    [[3, 1], { pieceType: PieceType.ChildPawn, color: Color.Black }],
    [[4, 1], { pieceType: PieceType.Pawn, color: Color.Black }],
    [[5, 1], { pieceType: PieceType.Pawn, color: Color.Black }],
    [[6, 1], { pieceType: PieceType.ChildPawn, color: Color.Black }],
    [[7, 1], { pieceType: PieceType.Pawn, color: Color.Black }],
    [[8, 1], { pieceType: PieceType.ChildPawn, color: Color.Black }],
    [[9, 1], { pieceType: PieceType.ChildPawn, color: Color.Black }],
    [[0, 2], { pieceType: PieceType.Archbishop, color: Color.Black }],
    [[9, 2], { pieceType: PieceType.Archbishop, color: Color.Black }],

    [[0, 9], { pieceType: PieceType.Rook, color: Color.White }],
    [[1, 9], { pieceType: PieceType.Horsie, color: Color.White }],
    [[2, 9], { pieceType: PieceType.Knook, color: Color.White }],
    [[3, 9], { pieceType: PieceType.Xook, color: Color.White }],
    [[4, 9], { pieceType: PieceType.King, color: Color.White }],
    [[5, 9], { pieceType: PieceType.Queen, color: Color.White }],
    [[6, 9], { pieceType: PieceType.Bishop, color: Color.White }],
    [[7, 9], { pieceType: PieceType.Antiqueen, color: Color.White }],
    [[8, 9], { pieceType: PieceType.Horsie, color: Color.White }],
    [[9, 9], { pieceType: PieceType.Rook, color: Color.White }],
    [[0, 8], { pieceType: PieceType.ChildPawn, color: Color.White }],
    [[1, 8], { pieceType: PieceType.ChildPawn, color: Color.White }],
    [[2, 8], { pieceType: PieceType.Pawn, color: Color.White }],
    [[3, 8], { pieceType: PieceType.ChildPawn, color: Color.White }],
    [[4, 8], { pieceType: PieceType.Pawn, color: Color.White }],
    [[5, 8], { pieceType: PieceType.Pawn, color: Color.White }],
    [[6, 8], { pieceType: PieceType.ChildPawn, color: Color.White }],
    [[7, 8], { pieceType: PieceType.Pawn, color: Color.White }],
    [[8, 8], { pieceType: PieceType.ChildPawn, color: Color.White }],
    [[9, 8], { pieceType: PieceType.ChildPawn, color: Color.White }],
    [[0, 7], { pieceType: PieceType.Archbishop, color: Color.White }],
    [[9, 7], { pieceType: PieceType.Archbishop, color: Color.White }],
];

const exported = {
    USERNAME_EDIT_EVERY,
    BOARD_WIDTH,
    BOARD_HEIGHT,
    BOARD_SIZE,
    ACCESS_TOKEN_EXPIRES_SECONDS,
    REFRESH_TOKEN_EXPIRES_SECONDS,
    ACCESS_TOKEN,
    REFRESH_TOKEN,
    LAST_LOGIN_LOCAL_STORAGE,
    GENERIC_ERROR,
    SETTING_PAGES,
};
export default exported;
