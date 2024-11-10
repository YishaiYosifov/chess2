import { ApiResponse, BaseAPI, JSONApiResponse } from "../runtime";
import type { LiveGame } from "@/lib/models";

export class LiveGameApi extends BaseAPI {
    /**
     * Fetch everything neccasary to load a game
     * Get Live Game
     */
    async getLiveGameRaw(
        token: string,
        initOverrides?: RequestInit
    ): Promise<ApiResponse<LiveGame>> {
        const response = await this.request(
            {
                path: `/live-game/${encodeURIComponent(token)}/load`,
                method: "GET",
            },
            initOverrides
        );

        return new JSONApiResponse(response);
    }
    getLiveGame = this.createFriendlyRoute(this.getLiveGameRaw);
}
