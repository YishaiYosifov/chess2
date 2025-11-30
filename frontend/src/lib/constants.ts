import {
    GameColor,
    PieceType,
    TimeControl,
    TimeControlSettings,
} from "./apiClient/definition/types.gen";
import { logicalPoint } from "../features/point/pointUtils";
import BoardPieces from "@/features/chessboard/lib/boardPieces";

const USERNAME_EDIT_EVERY_MS = 1000 * 60 * 60 * 24 * 7 * 2;

const BOARD_WIDTH = 10;
const BOARD_HEIGHT = 10;

const PROFILE_PICTURE_MAX_SIZE = 1024 * 1024 * 2;

export enum OAuthProvider {
    GOOGLE = "google",
    DISCORD = "discord",
}

const COOKIES = {
    REFRESH_TOKEN: "refreshToken",
    ACCESS_TOKEN: "accessToken",
    IS_LOGGED_IN: "isLoggedIn",
    AUTH_FAILURE: "authFailed",
    SIDEBAR_COLLAPSED: "sidebarCollapsed",
};

const PATHS = {
    SIGNIN: "/signin",
    LOGOUT: "/logout",
    REFRESH: "/refresh",
    GAME: "/game",
    PLAY: "/play",
    GUIDE: "/guide",
    QUESTS: "/quests",
    PROFILE: "/profile",
    CHALLENGE: "/challenge",
    WIN_STREAK: "/win-streak",
    SETTINGS_BASE: "/settings",
    SETTINGS_PROFILE: "/settings/profile",
    SETTINGS_SOCIAL: "/settings/social",
    OAUTH: `${process.env.NEXT_PUBLIC_API_URL}/api/oauth/signin/`,
    TOS: "/tos",
    PRIVACY: "/privacy",
    DONATE: "https://ko-fi.com/anarchychess",
    GITHUB: "https://github.com/slomg/anarchychess",
    DISCORD: "https://discord.gg/qnkddndecq",
    YOUTUBE: "https://youtube.com/@slomgdev",
};

const LOCALSTORAGE = {
    PREFERS_MATCHMAKING_POOL: "prefersMatchmakingPool",
    PREFERS_CHALLENGE_POOL: "prefersChallengePool",
    PREFERS_TIME_CONTROL_MINUTES_IDX: "prefersTimeControlMinutesIdx",
    PREFERS_TIME_CONTROL_INCREMENT_IDX: "prefersTimeControlIncrementIdx",
};

const SIGNALR_PATHS = {
    LOBBY: `${process.env.NEXT_PUBLIC_API_URL}/api/hub/lobby`,
    OPENSEEK: `${process.env.NEXT_PUBLIC_API_URL}/api/hub/openseek`,
    GAME: `${process.env.NEXT_PUBLIC_API_URL}/api/hub/game`,
    CHALLENGE: `${process.env.NEXT_PUBLIC_API_URL}/api/hub/challenge`,
};

const HEADERS = {
    REDIRECT_AFTER_AUTH: "Redirect-After-Auth",
};

const PAGINATION_PAGE_SIZE = {
    GAME_SUMMARY: 10,
    QUEST_LEADERBOARD: 10,
    WIN_STREAK_LEADERBOARD: 10,
    STARS: 5,
    BLOCKED: 5,
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
    { label: "Blitz", settings: { baseSeconds: 300, incrementSeconds: 3 } },
    { label: "Rapid", settings: { baseSeconds: 600, incrementSeconds: 0 } },
    { label: "Rapid", settings: { baseSeconds: 900, incrementSeconds: 10 } },
    {
        label: "Classical",
        settings: { baseSeconds: 1800, incrementSeconds: 0 },
    },
];

const CHALLENGE_MINUTES_OPTIONS = [
    0.25, 0.5, 1, 2, 3, 5, 7, 10, 15, 20, 25, 30, 45, 60, 90,
];
const DEFAULT_CHALLENGE_MINUTE_OPTION_IDX = 5;

const CHALLENGE_INCREMENT_SECONDS_OPTIONS = [
    0, 1, 2, 3, 4, 5, 10, 15, 20, 25, 30, 60,
];
const DEFAULT_CHALLENGE_INCREMENT_OPTION_IDX = 0;

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
    "rhnbqkbcar/pppdppdppp/10/10/9+/+9/10/10/PPPDPPDPPP/RHNBQKBCAR";

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
    c: PieceType.CHECKER,
};

// prettier-ignore
const DEFAULT_CHESS_BOARD = BoardPieces.fromPieces(
    { position: logicalPoint({ x: 0, y: 0 }), type: PieceType.ROOK, color: GameColor.WHITE, id: "0", },
    { position: logicalPoint({ x: 1, y: 0 }), type: PieceType.HORSEY, color: GameColor.WHITE, id: "1", },
    { position: logicalPoint({ x: 2, y: 0 }), type: PieceType.KNOOK, color: GameColor.WHITE, id: "2", },
    { position: logicalPoint({ x: 3, y: 0 }), type: PieceType.BISHOP, color: GameColor.WHITE, id: "3", },
    { position: logicalPoint({ x: 4, y: 0 }), type: PieceType.QUEEN, color: GameColor.WHITE, id: "4", },
    { position: logicalPoint({ x: 5, y: 0 }), type: PieceType.KING, color: GameColor.WHITE, id: "5", },
    { position: logicalPoint({ x: 6, y: 0 }), type: PieceType.BISHOP, color: GameColor.WHITE, id: "6", },
    { position: logicalPoint({ x: 7, y: 0 }), type: PieceType.CHECKER, color: GameColor.WHITE, id: "7", },
    { position: logicalPoint({ x: 8, y: 0 }), type: PieceType.ANTIQUEEN, color: GameColor.WHITE, id: "8", },
    { position: logicalPoint({ x: 9, y: 0 }), type: PieceType.ROOK, color: GameColor.WHITE, id: "9", },

    { position: logicalPoint({ x: 0, y: 1 }), type: PieceType.PAWN, color: GameColor.WHITE, id: "10", },
    { position: logicalPoint({ x: 1, y: 1 }), type: PieceType.PAWN, color: GameColor.WHITE, id: "11", },
    { position: logicalPoint({ x: 2, y: 1 }), type: PieceType.PAWN, color: GameColor.WHITE, id: "12", },
    { position: logicalPoint({ x: 3, y: 1 }), type: PieceType.UNDERAGE_PAWN, color: GameColor.WHITE, id: "13", },
    { position: logicalPoint({ x: 4, y: 1 }), type: PieceType.PAWN, color: GameColor.WHITE, id: "14", },
    { position: logicalPoint({ x: 5, y: 1 }), type: PieceType.PAWN, color: GameColor.WHITE, id: "15", },
    { position: logicalPoint({ x: 6, y: 1 }), type: PieceType.UNDERAGE_PAWN, color: GameColor.WHITE, id: "16", },
    { position: logicalPoint({ x: 7, y: 1 }), type: PieceType.PAWN, color: GameColor.WHITE, id: "17", },
    { position: logicalPoint({ x: 8, y: 1 }), type: PieceType.PAWN, color: GameColor.WHITE, id: "18", },
    { position: logicalPoint({ x: 9, y: 1 }), type: PieceType.PAWN, color: GameColor.WHITE, id: "19", },

    { position: logicalPoint({ x: 0, y: 4 }), type: PieceType.TRAITOR_ROOK, color: null, id: "20", },
    { position: logicalPoint({ x: 9, y: 5 }), type: PieceType.TRAITOR_ROOK, color: null, id: "21", },

    { position: logicalPoint({ x: 0, y: 8 }), type: PieceType.PAWN, color: GameColor.BLACK, id: "22", },
    { position: logicalPoint({ x: 1, y: 8 }), type: PieceType.PAWN, color: GameColor.BLACK, id: "23", },
    { position: logicalPoint({ x: 2, y: 8 }), type: PieceType.PAWN, color: GameColor.BLACK, id: "24", },
    { position: logicalPoint({ x: 3, y: 8 }), type: PieceType.UNDERAGE_PAWN, color: GameColor.BLACK, id: "25", },
    { position: logicalPoint({ x: 4, y: 8 }), type: PieceType.PAWN, color: GameColor.BLACK, id: "26", },
    { position: logicalPoint({ x: 5, y: 8 }), type: PieceType.PAWN, color: GameColor.BLACK, id: "27", },
    { position: logicalPoint({ x: 6, y: 8 }), type: PieceType.UNDERAGE_PAWN, color: GameColor.BLACK, id: "28", },
    { position: logicalPoint({ x: 7, y: 8 }), type: PieceType.PAWN, color: GameColor.BLACK, id: "29", },
    { position: logicalPoint({ x: 8, y: 8 }), type: PieceType.PAWN, color: GameColor.BLACK, id: "30", },
    { position: logicalPoint({ x: 9, y: 8 }), type: PieceType.PAWN, color: GameColor.BLACK, id: "31", },

    { position: logicalPoint({ x: 0, y: 9 }), type: PieceType.ROOK, color: GameColor.BLACK, id: "32", },
    { position: logicalPoint({ x: 1, y: 9 }), type: PieceType.HORSEY, color: GameColor.BLACK, id: "33", },
    { position: logicalPoint({ x: 2, y: 9 }), type: PieceType.KNOOK, color: GameColor.BLACK, id: "34", },
    { position: logicalPoint({ x: 3, y: 9 }), type: PieceType.BISHOP, color: GameColor.BLACK, id: "35", },
    { position: logicalPoint({ x: 4, y: 9 }), type: PieceType.QUEEN, color: GameColor.BLACK, id: "36", },
    { position: logicalPoint({ x: 5, y: 9 }), type: PieceType.KING, color: GameColor.BLACK, id: "37", },
    { position: logicalPoint({ x: 6, y: 9 }), type: PieceType.BISHOP, color: GameColor.BLACK, id: "38", },
    { position: logicalPoint({ x: 7, y: 9 }), type: PieceType.CHECKER, color: GameColor.BLACK, id: "39", },
    { position: logicalPoint({ x: 8, y: 9 }), type: PieceType.ANTIQUEEN, color: GameColor.BLACK, id: "40", },
    { position: logicalPoint({ x: 9, y: 9 }), type: PieceType.ROOK, color: GameColor.BLACK, id: "41", },
);

const QUEST_WEEKDAY_NAMES: Record<number, string> = {
    0: "Savage Sunday",
    1: "Mundane Monday",
    2: "Tame Tuesday",
    3: "Wild Wednesday",
    4: "Thrilling Thursday",
    5: "Fiery Friday",
    6: "Strenuous Saturday",
};

const constants = {
    USERNAME_EDIT_EVERY_MS,
    BOARD_WIDTH,
    BOARD_HEIGHT,
    PROFILE_PICTURE_MAX_SIZE,
    STANDARD_TIME_CONTROLS,
    TIME_CONTROL_LABELS,
    DISPLAY_TIME_CONTROLS,
    CHALLENGE_MINUTES_OPTIONS,
    DEFAULT_CHALLENGE_MINUTE_OPTION_IDX,
    CHALLENGE_INCREMENT_SECONDS_OPTIONS,
    DEFAULT_CHALLENGE_INCREMENT_OPTION_IDX,
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
    QUEST_WEEKDAY_NAMES,
} as const;
export default constants;
