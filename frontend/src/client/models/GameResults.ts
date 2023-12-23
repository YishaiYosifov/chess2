/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { GameResult } from './GameResult';
import type { SimpleUserOut } from './SimpleUserOut';
import type { Variant } from './Variant';

export type GameResults = {
    token: string;
    user_white: (SimpleUserOut | null);
    user_black: (SimpleUserOut | null);
    results: GameResult;
    variant: Variant;
    time_control: number;
    increment: number;
    created_at: string;
};
