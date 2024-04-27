import type { GameSettings } from "@/models/index";
import {
    ApiResponse,
    BaseAPI,
    TextApiResponse,
    VoidApiResponse,
} from "../runtime";

export class GameRequestsApi extends BaseAPI {
    /**
     * Cancel
     */
    async cancelRaw(initOverrides?: RequestInit): Promise<ApiResponse<void>> {
        const response = await this.request(
            {
                path: "/game-requests/cancel",
                method: "POST",
            },
            initOverrides
        );

        return new VoidApiResponse(response);
    }
    cancel = this.createFriendlyRoute(this.cancelRaw);

    /**
     * Joins the matchmaking pool with the specified game settings. If a game was not found, it will create a new game request.
     * Start Pool Game
     */
    async startPoolGameRaw(
        requestParameters: GameSettings,
        initOverrides?: RequestInit
    ): Promise<ApiResponse<string | void>> {
        const response = await this.request(
            {
                path: "/game-requests/pool/join",
                method: "POST",
                body: requestParameters,
            },
            initOverrides
        );

        if (response.status == 200) return new TextApiResponse(response);
        else return new VoidApiResponse(response);
    }
    startPoolGame = this.createFriendlyRoute(this.startPoolGameRaw);
}
