/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { GameSettings } from '../models/GameSettings';

import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';

export class GameRequestsService {

    /**
     * Start Pool Game
     * Searches for a game request that fits the criterias.
 * If a game was not found, it will create a new game request with an unspecified recipient.
     * @returns any Game started
     * @throws ApiError
     */
    public static startPoolGame({
requestBody,
}: {
requestBody: GameSettings,
}): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/game-requests/pool/join',
            body: requestBody,
            mediaType: 'application/json',
            errors: {
                401: `Could not verify credentials`,
                409: `You already have an active game`,
                422: `Validation Error`,
            },
        });
    }

}
