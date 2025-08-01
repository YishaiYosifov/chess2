import { PieceMap } from "@/types/tempModels";
import {
    GameColor,
    PieceType,
    TimeControl,
    TimeControlSettings,
} from "./apiClient/definition/types.gen";
import { logicalPoint } from "./utils/pointUtils";

const USERNAME_EDIT_EVERY = 2419200;

const BOARD_WIDTH = 10;
const BOARD_HEIGHT = 10;
const MIN_BOARD_SIZE_PX = 264;

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
    REDIRECT_AFTER_AUTH: "Redirect-After-Auth",
};

const SETTING_PAGES = [
    { name: "Profile", url: "profile" },
    { name: "Live Game", url: "live-game" },
    { name: "Blocked", url: "blocked" },
    { name: "Security", url: "security" },
];

const PAGINATION_PAGE_SIZE = {
    GAME_SUMMARY: 10,
};

interface TimeControlLabel {
    type: string;
    settings: TimeControlSettings;
    isMostPopular?: boolean;
}

const STANDARD_TIME_CONTROLS: TimeControlLabel[] = [
    { type: "Bullet", settings: { baseSeconds: 60, incrementSeconds: 0 } },
    { type: "Bullet", settings: { baseSeconds: 120, incrementSeconds: 1 } },
    { type: "Blitz", settings: { baseSeconds: 180, incrementSeconds: 0 } },
    { type: "Blitz", settings: { baseSeconds: 180, incrementSeconds: 2 } },
    {
        type: "Blitz",
        settings: { baseSeconds: 300, incrementSeconds: 0 },
        isMostPopular: true,
    },
    { type: "Rapid", settings: { baseSeconds: 300, incrementSeconds: 3 } },
    { type: "Rapid", settings: { baseSeconds: 600, incrementSeconds: 0 } },
    { type: "Rapid", settings: { baseSeconds: 900, incrementSeconds: 10 } },
    {
        type: "Classical",
        settings: { baseSeconds: 1800, incrementSeconds: 0 },
    },
];

const TIME_CONTROL_LABELS: Record<TimeControl, string> = {
    [TimeControl.BULLET]: "Bullet",
    [TimeControl.BLITZ]: "Blitz",
    [TimeControl.RAPID]: "Rapid",
    [TimeControl.CLASSICAL]: "Classical",
};

const DISPLAY_TIME_CONTROLS: TimeControl[] = [
    TimeControl.BULLET,
    TimeControl.BLITZ,
    TimeControl.RAPID,
    TimeControl.CLASSICAL,
];

const INITIAL_FEN =
    "rhn1qkb1hr/pppdppdppp/10/10/10/10/10/10/PPPDPPDPPP/RHN1QKB1HR";

const LETTER_TO_PIECE: Record<string, PieceType> = {
    k: PieceType.KING,
    q: PieceType.QUEEN,
    r: PieceType.ROOK,
    n: PieceType.KNOOK,
    b: PieceType.BISHOP,
    h: PieceType.HORSEY,
    p: PieceType.PAWN,
    d: PieceType.UNDERAGE_PAWN,
};

// prettier-ignore
const DEFAULT_CHESS_BOARD: PieceMap = new Map([
    ["0", { position: logicalPoint({ x: 0, y: 0 }), type: PieceType.ROOK, color: GameColor.WHITE }],
    ["1", { position: logicalPoint({ x: 1, y: 0 }), type: PieceType.HORSEY, color: GameColor.WHITE }],
    ["2", { position: logicalPoint({ x: 2, y: 0 }), type: PieceType.KNOOK, color: GameColor.WHITE }],
    // ["", { position: logicalPoint({ x: 3, y: 0 }), type: PieceType.Xook, color: Color.White }],
    ["3", { position: logicalPoint({ x: 4, y: 0 }), type: PieceType.QUEEN, color: GameColor.WHITE }],
    ["4", { position: logicalPoint({ x: 5, y: 0 }), type: PieceType.KING, color: GameColor.WHITE }],
    ["5", { position: logicalPoint({ x: 6, y: 0 }), type: PieceType.BISHOP, color: GameColor.WHITE }],
    // ["", { position: logicalPoint({ x: 7, y: 0 }), type: PieceType.Antiqueen, color: Color.White }],
    ["6", { position: logicalPoint({ x: 8, y: 0 }), type: PieceType.HORSEY, color: GameColor.WHITE }],
    ["7", { position: logicalPoint({ x: 9, y: 0 }), type: PieceType.ROOK, color: GameColor.WHITE }],

    ["8", { position: logicalPoint({ x: 0, y: 1 }), type: PieceType.PAWN, color: GameColor.WHITE }],
    ["9", { position: logicalPoint({ x: 1, y: 1 }), type: PieceType.PAWN, color: GameColor.WHITE }],
    ["10", { position: logicalPoint({ x: 2, y: 1 }), type: PieceType.PAWN, color: GameColor.WHITE }],
    ["11", { position: logicalPoint({ x: 3, y: 1 }), type: PieceType.UNDERAGE_PAWN, color: GameColor.WHITE }],
    ["12", { position: logicalPoint({ x: 4, y: 1 }), type: PieceType.PAWN, color: GameColor.WHITE }],
    ["13", { position: logicalPoint({ x: 5, y: 1 }), type: PieceType.PAWN, color: GameColor.WHITE }],
    ["14", { position: logicalPoint({ x: 6, y: 1 }), type: PieceType.UNDERAGE_PAWN, color: GameColor.WHITE }],
    ["15", { position: logicalPoint({ x: 7, y: 1 }), type: PieceType.PAWN, color: GameColor.WHITE }],
    ["16", { position: logicalPoint({ x: 8, y: 1 }), type: PieceType.PAWN, color: GameColor.WHITE }],
    ["17", { position: logicalPoint({ x: 9, y: 1 }), type: PieceType.PAWN, color: GameColor.WHITE }],
    // ["", { position: logicalPoint({ x: 0, y: 2 }), type: PieceType.Archbishop, color: Color.White }],
    // ["", { position: logicalPoint({ x: 9, y: 2 }), type: PieceType.Archbishop, color: Color.White }],

    // ["", { position: logicalPoint({ x: 0, y: 7 }), type: PieceType.Archbishop, color: Color.Black }],
    // ["", { position: logicalPoint({ x: 9, y: 7 }), type: PieceType.Archbishop, color: Color.Black }],
    ["18", { position: logicalPoint({ x: 0, y: 8 }), type: PieceType.PAWN, color: GameColor.BLACK }],
    ["19", { position: logicalPoint({ x: 1, y: 8 }), type: PieceType.PAWN, color: GameColor.BLACK }],
    ["20", { position: logicalPoint({ x: 2, y: 8 }), type: PieceType.PAWN, color: GameColor.BLACK }],
    ["21", { position: logicalPoint({ x: 3, y: 8 }), type: PieceType.UNDERAGE_PAWN, color: GameColor.BLACK }],
    ["22", { position: logicalPoint({ x: 4, y: 8 }), type: PieceType.PAWN, color: GameColor.BLACK }],
    ["23", { position: logicalPoint({ x: 5, y: 8 }), type: PieceType.PAWN, color: GameColor.BLACK }],
    ["24", { position: logicalPoint({ x: 6, y: 8 }), type: PieceType.UNDERAGE_PAWN, color: GameColor.BLACK }],
    ["25", { position: logicalPoint({ x: 7, y: 8 }), type: PieceType.PAWN, color: GameColor.BLACK }],
    ["26", { position: logicalPoint({ x: 8, y: 8 }), type: PieceType.PAWN, color: GameColor.BLACK }],
    ["27", { position: logicalPoint({ x: 9, y: 8 }), type: PieceType.PAWN, color: GameColor.BLACK }],

    ["28", { position: logicalPoint({ x: 0, y: 9 }), type: PieceType.ROOK, color: GameColor.BLACK }],
    ["29", { position: logicalPoint({ x: 1, y: 9 }), type: PieceType.HORSEY, color: GameColor.BLACK }],
    ["30", { position: logicalPoint({ x: 2, y: 9 }), type: PieceType.KNOOK, color: GameColor.BLACK }],
    // ["", { position: logicalPoint({ x: 3, y: 9 }), type: PieceType.Xook, color: Color.Black }],
    ["31", { position: logicalPoint({ x: 4, y: 9 }), type: PieceType.QUEEN, color: GameColor.BLACK }],
    ["32", { position: logicalPoint({ x: 5, y: 9 }), type: PieceType.KING, color: GameColor.BLACK }],
    ["33", { position: logicalPoint({ x: 6, y: 9 }), type: PieceType.BISHOP, color: GameColor.BLACK }],
    // ["", { position: logicalPoint({ x: 7, y: 9 }), type: PieceType.Antiqueen, color: Color.Black }],
    ["34", { position: logicalPoint({ x: 8, y: 9 }), type: PieceType.HORSEY, color: GameColor.BLACK }],
    ["35", { position: logicalPoint({ x: 9, y: 9 }), type: PieceType.ROOK, color: GameColor.BLACK }],
]);

const constants = {
    USERNAME_EDIT_EVERY,
    BOARD_WIDTH,
    BOARD_HEIGHT,
    MIN_BOARD_SIZE_PX,
    SETTING_PAGES,
    STANDARD_TIME_CONTROLS,
    TIME_CONTROL_LABELS,
    DISPLAY_TIME_CONTROLS,
    COOKIES,
    PATHS,
    HEADERS,
    INITIAL_FEN,
    LETTER_TO_PIECE,
    DEFAULT_CHESS_BOARD,
    SIGNALR_PATHS,
    PAGINATION_PAGE_SIZE,
} as const;
export default constants;
