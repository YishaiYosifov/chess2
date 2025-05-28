import { Color, PieceMap, PieceType } from "@/types/tempModels";

const USERNAME_EDIT_EVERY = 2419200;

const BOARD_WIDTH = 10;
const BOARD_HEIGHT = 10;
const BOARD_SIZE = BOARD_WIDTH * BOARD_HEIGHT;

const GENERIC_ERROR = "Something went wrong.";

export enum OAuthProvider {
    GOOGLE = "google",
    DISCORD = "discord",
}

const COOKIES = {
    REFRESH_TOKEN: "refreshToken",
    ACCESS_TOKEN: "accessToken",
    IS_AUTHED: "isAuthed",
    SIDEBAR_COLLAPSED: "sidebarCollapsed",
};

const PATHS = {
    LOGIN: "/login",
    LOGOUT: "/logout",
    REFRESH: "/refresh",
    OAUTH: `${process.env.NEXT_PUBLIC_API_URL}/api/oauth/signin/`,
};

const WEBSOCKET_PATHS = {
    MATCHMAKING: `${process.env.NEXT_PUBLIC_WS_URL}/api/ws/matchmaking`,
};

const HEADERS = {
    REFRESH_REDIRECT: "x-refresh-redirect",
};

const SETTING_PAGES = [
    { name: "Profile", url: "profile" },
    { name: "Live Game", url: "live-game" },
    { name: "Blocked", url: "blocked" },
    { name: "Security", url: "security" },
];

const TIME_CONTROLS = [
    { type: "Bullet", baseMinutes: 1, increment: 0 },
    { type: "Bullet", baseMinutes: 2, increment: 1 },
    { type: "Blitz", baseMinutes: 3, increment: 0 },
    { type: "Blitz", baseMinutes: 3, increment: 2 },
    { type: "Blitz", baseMinutes: 5, increment: 0, isMostPopular: true },
    { type: "Rapid", baseMinutes: 5, increment: 3 },
    { type: "Rapid", baseMinutes: 10, increment: 0 },
    { type: "Rapid", baseMinutes: 15, increment: 10 },
    { type: "Classical", baseMinutes: 30, increment: 0 },
];

// prettier-ignore
export const DEFAULT_CHESS_BOARD: PieceMap = new Map([
    ["0", { position: [0, 0], pieceType: PieceType.Rook, color: Color.Black }],
    ["1", { position: [1, 0], pieceType: PieceType.Horsie, color: Color.Black }],
    ["2", { position: [2, 0], pieceType: PieceType.Knook, color: Color.Black }],
    ["3", { position: [3, 0], pieceType: PieceType.Xook, color: Color.Black }],
    ["4", { position: [4, 0], pieceType: PieceType.Queen, color: Color.Black }],
    ["5", { position: [5, 0], pieceType: PieceType.King, color: Color.Black }],
    ["6", { position: [6, 0], pieceType: PieceType.Bishop, color: Color.Black }],
    ["7", { position: [7, 0], pieceType: PieceType.Antiqueen, color: Color.Black }],
    ["8", { position: [8, 0], pieceType: PieceType.Horsie, color: Color.Black }],
    ["9", { position: [9, 0], pieceType: PieceType.Rook, color: Color.Black }],
    ["10", { position: [0, 1], pieceType: PieceType.ChildPawn, color: Color.Black }],
    ["11", { position: [1, 1], pieceType: PieceType.ChildPawn, color: Color.Black }],
    ["12", { position: [2, 1], pieceType: PieceType.Pawn, color: Color.Black }],
    ["13", { position: [3, 1], pieceType: PieceType.ChildPawn, color: Color.Black }],
    ["14", { position: [4, 1], pieceType: PieceType.Pawn, color: Color.Black }],
    ["15", { position: [5, 1], pieceType: PieceType.Pawn, color: Color.Black }],
    ["16", { position: [6, 1], pieceType: PieceType.ChildPawn, color: Color.Black }],
    ["17", { position: [7, 1], pieceType: PieceType.Pawn, color: Color.Black }],
    ["18", { position: [8, 1], pieceType: PieceType.ChildPawn, color: Color.Black }],
    ["19", { position: [9, 1], pieceType: PieceType.ChildPawn, color: Color.Black }],
    ["20", { position: [0, 2], pieceType: PieceType.Archbishop, color: Color.Black }],
    ["21", { position: [9, 2], pieceType: PieceType.Archbishop, color: Color.Black }],

    ["22", { position: [0, 7], pieceType: PieceType.Archbishop, color: Color.White }],
    ["23", { position: [9, 7], pieceType: PieceType.Archbishop, color: Color.White }],
    ["24", { position: [0, 8], pieceType: PieceType.ChildPawn, color: Color.White }],
    ["25", { position: [1, 8], pieceType: PieceType.ChildPawn, color: Color.White }],
    ["26", { position: [2, 8], pieceType: PieceType.Pawn, color: Color.White }],
    ["27", { position: [3, 8], pieceType: PieceType.ChildPawn, color: Color.White }],
    ["28", { position: [4, 8], pieceType: PieceType.Pawn, color: Color.White }],
    ["29", { position: [5, 8], pieceType: PieceType.Pawn, color: Color.White }],
    ["30", { position: [6, 8], pieceType: PieceType.ChildPawn, color: Color.White }],
    ["31", { position: [7, 8], pieceType: PieceType.Pawn, color: Color.White }],
    ["32", { position: [8, 8], pieceType: PieceType.ChildPawn, color: Color.White }],
    ["33", { position: [9, 8], pieceType: PieceType.ChildPawn, color: Color.White }],
    ["34", { position: [0, 9], pieceType: PieceType.Rook, color: Color.White }],
    ["35", { position: [1, 9], pieceType: PieceType.Horsie, color: Color.White }],
    ["36", { position: [2, 9], pieceType: PieceType.Knook, color: Color.White }],
    ["37", { position: [3, 9], pieceType: PieceType.Xook, color: Color.White }],
    ["38", { position: [4, 9], pieceType: PieceType.Queen, color: Color.White }],
    ["39", { position: [5, 9], pieceType: PieceType.King, color: Color.White }],
    ["40", { position: [6, 9], pieceType: PieceType.Bishop, color: Color.White }],
    ["41", { position: [7, 9], pieceType: PieceType.Antiqueen, color: Color.White }],
    ["42", { position: [8, 9], pieceType: PieceType.Horsie, color: Color.White }],
    ["43", { position: [9, 9], pieceType: PieceType.Rook, color: Color.White }],
]);

const constants = {
    USERNAME_EDIT_EVERY,
    BOARD_WIDTH,
    BOARD_HEIGHT,
    BOARD_SIZE,
    GENERIC_ERROR,
    SETTING_PAGES,
    TIME_CONTROLS,
    COOKIES,
    PATHS,
    HEADERS,
    DEFAULT_CHESS_BOARD,
    WEBSOCKET_PATHS,
};
export default constants;
