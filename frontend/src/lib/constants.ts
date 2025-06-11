import { PieceMap, PieceType } from "@/types/tempModels";
import { GameColor } from "./apiClient/definition/types.gen";

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
    GUEST: "/guest",
    GAME: "/game",
    OAUTH: `${process.env.NEXT_PUBLIC_API_URL}/api/oauth/signin/`,
};

const SIGNALR_PATHS = {
    MATCHMAKING: `${process.env.NEXT_PUBLIC_API_URL}/api/hub/matchmaking`,
    GAME: `${process.env.NEXT_PUBLIC_API_URL}/api/hub/game`,
};

const HEADERS = {
    REDIRECT_AFTER_AUTH: "x-redirect-after-auth",
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
    ["0", { position: { x: 0, y: 0 }, type: PieceType.ROOK, color: GameColor.WHITE }],
    ["1", { position: { x: 1, y: 0 }, type: PieceType.HORSEY, color: GameColor.WHITE }],
    // ["", { position: { x: 2, y: 0 }, type: PieceType.Knook, color: Color.White }],
    // ["", { position: { x: 3, y: 0 }, type: PieceType.Xook, color: Color.White }],
    ["2", { position: { x: 4, y: 0 }, type: PieceType.QUEEN, color: GameColor.WHITE }],
    ["3", { position: { x: 5, y: 0 }, type: PieceType.KING, color: GameColor.WHITE }],
    ["4", { position: { x: 6, y: 0 }, type: PieceType.BISHOP, color: GameColor.WHITE }],
    // ["", { position: { x: 7, y: 0 }, type: PieceType.Antiqueen, color: Color.White }],
    ["5", { position: { x: 8, y: 0 }, type: PieceType.HORSEY, color: GameColor.WHITE }],
    ["6", { position: { x: 9, y: 0 }, type: PieceType.ROOK, color: GameColor.WHITE }],
    ["7", { position: { x: 0, y: 1 }, type: PieceType.PAWN, color: GameColor.WHITE }],
    ["8", { position: { x: 1, y: 1 }, type: PieceType.PAWN, color: GameColor.WHITE }],
    // ["", { position: { x: 2, y: 1 }, type: PieceType.ChildPawn, color: Color.White }],
    ["9", { position: { x: 3, y: 1 }, type: PieceType.PAWN, color: GameColor.WHITE }],
    ["10", { position: { x: 4, y: 1 }, type: PieceType.PAWN, color: GameColor.WHITE }],
    ["11", { position: { x: 5, y: 1 }, type: PieceType.PAWN, color: GameColor.WHITE }],
    ["12", { position: { x: 6, y: 1 }, type: PieceType.PAWN, color: GameColor.WHITE }],
    // ["", { position: { x: 7, y: 1 }, type: PieceType.ChildPawn, color: Color.White }],
    ["13", { position: { x: 8, y: 1 }, type: PieceType.PAWN, color: GameColor.WHITE }],
    ["14", { position: { x: 9, y: 1 }, type: PieceType.PAWN, color: GameColor.WHITE }],
    // ["", { position: { x: 0, y: 2 }, type: PieceType.Archbishop, color: Color.White }],
    // ["", { position: { x: 9, y: 2 }, type: PieceType.Archbishop, color: Color.White }],

    // ["", { position: { x: 0, y: 7 }, type: PieceType.Archbishop, color: Color.Black }],
    // ["", { position: { x: 9, y: 7 }, type: PieceType.Archbishop, color: Color.Black }],
    ["15", { position: { x: 0, y: 8 }, type: PieceType.PAWN, color: GameColor.BLACK }],
    ["16", { position: { x: 1, y: 8 }, type: PieceType.PAWN, color: GameColor.BLACK }],
    // ["", { position: { x: 2, y: 8 }, type: PieceType.ChildPawn, color: Color.Black }],
    ["17", { position: { x: 3, y: 8}, type: PieceType.PAWN, color: GameColor.BLACK }],
    ["18", { position: { x: 4, y:8 }, type: PieceType.PAWN, color: GameColor.BLACK }],
    ["19", { position: { x: 5, y: 8 }, type: PieceType.PAWN, color: GameColor.BLACK }],
    ["20", { position: { x: 6, y: 8 }, type: PieceType.PAWN, color: GameColor.BLACK }],
    // ["", { position: { x: 7, y: 8 }, type: PieceType.ChildPawn, color: Color.Black }],
    ["21", { position: { x: 8, y: 8 }, type: PieceType.PAWN, color: GameColor.BLACK }],
    ["22", { position: { x: 9, y: 8 }, type: PieceType.PAWN, color: GameColor.BLACK }],
    ["23", { position: { x: 0, y: 9 }, type: PieceType.ROOK, color: GameColor.BLACK }],
    ["24", { position: { x: 1, y: 9 }, type: PieceType.HORSEY, color: GameColor.BLACK }],
    // ["", { position: { x: 2, y: 9 }, type: PieceType.Knook, color: Color.Black }],
    // ["", { position: { x: 3, y: 9 }, type: PieceType.Xook, color: Color.Black }],
    ["25", { position: { x: 4, y: 9 }, type: PieceType.QUEEN, color: GameColor.BLACK }],
    ["26", { position: { x: 5, y: 9 }, type: PieceType.KING, color: GameColor.BLACK }],
    ["27", { position: { x: 6, y: 9 }, type: PieceType.BISHOP, color: GameColor.BLACK }],
    // ["", { position: { x: 7, y: 9 }, type: PieceType.Antiqueen, color: Color.Black }],
    ["28", { position: { x: 8, y: 9 }, type: PieceType.HORSEY, color: GameColor.BLACK }],
    ["29", { position: { x: 9, y: 9 }, type: PieceType.ROOK, color: GameColor.BLACK }],
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
    SIGNALR_PATHS,
};
export default constants;
