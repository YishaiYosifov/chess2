export const USERNAME_EDIT_EVERY = 2419200;
export const EMAIL_EDIT_EVERY = 86400;

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

export enum Variant {
    Anarchy = "anarchy",
    FogOfWar = "fog of war",
    Chss = "chss",
}
