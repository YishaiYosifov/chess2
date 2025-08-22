import { PieceMap } from "@/features/chessboard/lib/types";
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

export enum OAuthProvider {
    GOOGLE = "google",
    DISCORD = "discord",
}

const COOKIES = {
    REFRESH_TOKEN: "refreshToken",
    ACCESS_TOKEN: "accessToken",
    IS_LOGGED_IN: "isLoggedIn",
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

const LOCALSTORAGE = {
    PREFERS_RATED_POOL: "prefersRatedPool",
};

const SIGNALR_PATHS = {
    LOBBY: `${process.env.NEXT_PUBLIC_API_URL}/api/hub/lobby`,
    OPENSEEK: `${process.env.NEXT_PUBLIC_API_URL}/api/hub/openseek`,
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
    label: string;
    settings: TimeControlSettings;
    isMostPopular?: boolean;
}

const STANDARD_TIME_CONTROLS: TimeControlLabel[] = [
    { label: "Bullet", settings: { baseSeconds: 60, incrementSeconds: 0 } },
    { label: "Bullet", settings: { baseSeconds: 120, incrementSeconds: 1 } },
    { label: "Blitz", settings: { baseSeconds: 180, incrementSeconds: 0 } },
    { label: "Blitz", settings: { baseSeconds: 180, incrementSeconds: 2 } },
    {
        label: "Blitz",
        settings: { baseSeconds: 300, incrementSeconds: 0 },
        isMostPopular: true,
    },
    { label: "Rapid", settings: { baseSeconds: 300, incrementSeconds: 3 } },
    { label: "Rapid", settings: { baseSeconds: 600, incrementSeconds: 0 } },
    { label: "Rapid", settings: { baseSeconds: 900, incrementSeconds: 10 } },
    {
        label: "Classical",
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

const SEEK_RESUBSCRIBE_INTERAVAL_MS = 1000 * 60 * 4;
const OPEN_SEEK_RESUBSCRIBE_INTERAVAL_MS = 1000 * 60 * 4;

const ALLOW_ABORTION_UNTIL_MOVE = 2;

const INITIAL_FEN =
    "rhn1qkbahr/pppdppdppp/10/10/10/10/10/10/PPPDPPDPPP/RHN1QKBAHR";

const LETTER_TO_PIECE: Record<string, PieceType> = {
    k: PieceType.KING,
    q: PieceType.QUEEN,
    r: PieceType.ROOK,
    n: PieceType.KNOOK,
    b: PieceType.BISHOP,
    h: PieceType.HORSEY,
    p: PieceType.PAWN,
    d: PieceType.UNDERAGE_PAWN,
    a: PieceType.ANTIQUEEN,
    "+": PieceType.TRAITOR_ROOK,
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
    ["6", { position: logicalPoint({ x: 7, y: 0 }), type: PieceType.ANTIQUEEN, color: GameColor.WHITE }],
    ["7", { position: logicalPoint({ x: 8, y: 0 }), type: PieceType.HORSEY, color: GameColor.WHITE }],
    ["8", { position: logicalPoint({ x: 9, y: 0 }), type: PieceType.ROOK, color: GameColor.WHITE }],

    ["9", { position: logicalPoint({ x: 0, y: 1 }), type: PieceType.PAWN, color: GameColor.WHITE }],
    ["10", { position: logicalPoint({ x: 1, y: 1 }), type: PieceType.PAWN, color: GameColor.WHITE }],
    ["11", { position: logicalPoint({ x: 2, y: 1 }), type: PieceType.PAWN, color: GameColor.WHITE }],
    ["12", { position: logicalPoint({ x: 3, y: 1 }), type: PieceType.UNDERAGE_PAWN, color: GameColor.WHITE }],
    ["13", { position: logicalPoint({ x: 4, y: 1 }), type: PieceType.PAWN, color: GameColor.WHITE }],
    ["14", { position: logicalPoint({ x: 5, y: 1 }), type: PieceType.PAWN, color: GameColor.WHITE }],
    ["15", { position: logicalPoint({ x: 6, y: 1 }), type: PieceType.UNDERAGE_PAWN, color: GameColor.WHITE }],
    ["16", { position: logicalPoint({ x: 7, y: 1 }), type: PieceType.PAWN, color: GameColor.WHITE }],
    ["17", { position: logicalPoint({ x: 8, y: 1 }), type: PieceType.PAWN, color: GameColor.WHITE }],
    ["18", { position: logicalPoint({ x: 9, y: 1 }), type: PieceType.PAWN, color: GameColor.WHITE }],
    // ["", { position: logicalPoint({ x: 0, y: 2 }), type: PieceType.Archbishop, color: Color.White }],
    // ["", { position: logicalPoint({ x: 9, y: 2 }), type: PieceType.Archbishop, color: Color.White }],

    ["19", { position: logicalPoint({ x: 0, y: 4 }), type: PieceType.TRAITOR_ROOK, color: null }],
    ["20", { position: logicalPoint({ x: 9, y: 5 }), type: PieceType.TRAITOR_ROOK, color: null }],

    // ["", { position: logicalPoint({ x: 0, y: 7 }), type: PieceType.Archbishop, color: Color.Black }],
    // ["", { position: logicalPoint({ x: 9, y: 7 }), type: PieceType.Archbishop, color: Color.Black }],
    ["21", { position: logicalPoint({ x: 0, y: 8 }), type: PieceType.PAWN, color: GameColor.BLACK }],
    ["22", { position: logicalPoint({ x: 1, y: 8 }), type: PieceType.PAWN, color: GameColor.BLACK }],
    ["23", { position: logicalPoint({ x: 2, y: 8 }), type: PieceType.PAWN, color: GameColor.BLACK }],
    ["24", { position: logicalPoint({ x: 3, y: 8 }), type: PieceType.UNDERAGE_PAWN, color: GameColor.BLACK }],
    ["25", { position: logicalPoint({ x: 4, y: 8 }), type: PieceType.PAWN, color: GameColor.BLACK }],
    ["26", { position: logicalPoint({ x: 5, y: 8 }), type: PieceType.PAWN, color: GameColor.BLACK }],
    ["27", { position: logicalPoint({ x: 6, y: 8 }), type: PieceType.UNDERAGE_PAWN, color: GameColor.BLACK }],
    ["28", { position: logicalPoint({ x: 7, y: 8 }), type: PieceType.PAWN, color: GameColor.BLACK }],
    ["29", { position: logicalPoint({ x: 8, y: 8 }), type: PieceType.PAWN, color: GameColor.BLACK }],
    ["30", { position: logicalPoint({ x: 9, y: 8 }), type: PieceType.PAWN, color: GameColor.BLACK }],

    ["31", { position: logicalPoint({ x: 0, y: 9 }), type: PieceType.ROOK, color: GameColor.BLACK }],
    ["32", { position: logicalPoint({ x: 1, y: 9 }), type: PieceType.HORSEY, color: GameColor.BLACK }],
    ["33", { position: logicalPoint({ x: 2, y: 9 }), type: PieceType.KNOOK, color: GameColor.BLACK }],
    // ["", { position: logicalPoint({ x: 3, y: 9 }), type: PieceType.Xook, color: Color.Black }],
    ["34", { position: logicalPoint({ x: 4, y: 9 }), type: PieceType.QUEEN, color: GameColor.BLACK }],
    ["35", { position: logicalPoint({ x: 5, y: 9 }), type: PieceType.KING, color: GameColor.BLACK }],
    ["36", { position: logicalPoint({ x: 6, y: 9 }), type: PieceType.BISHOP, color: GameColor.BLACK }],
    ["37", { position: logicalPoint({ x: 7, y: 9 }), type: PieceType.ANTIQUEEN, color: GameColor.BLACK }],
    ["38", { position: logicalPoint({ x: 8, y: 9 }), type: PieceType.HORSEY, color: GameColor.BLACK }],
    ["39", { position: logicalPoint({ x: 9, y: 9 }), type: PieceType.ROOK, color: GameColor.BLACK }],
]);

const constants = {
    USERNAME_EDIT_EVERY,
    BOARD_WIDTH,
    BOARD_HEIGHT,
    SETTING_PAGES,
    STANDARD_TIME_CONTROLS,
    TIME_CONTROL_LABELS,
    DISPLAY_TIME_CONTROLS,
    SEEK_RESUBSCRIBE_INTERAVAL_MS,
    OPEN_SEEK_RESUBSCRIBE_INTERAVAL_MS,
    COOKIES,
    PATHS,
    HEADERS,
    LOCALSTORAGE,
    ALLOW_ABORTION_UNTIL_MOVE,
    INITIAL_FEN,
    LETTER_TO_PIECE,
    DEFAULT_CHESS_BOARD,
    SIGNALR_PATHS,
    PAGINATION_PAGE_SIZE,
} as const;
export default constants;
