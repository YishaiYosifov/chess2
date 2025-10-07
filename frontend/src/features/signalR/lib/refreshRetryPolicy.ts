import { ErrorCode } from "@/lib/apiClient";
import handleRefresh from "@/lib/apiClient/refresh";
import { IRetryPolicy, RetryContext } from "@microsoft/signalr";

export default class RefreshRetryPolicy implements IRetryPolicy {
    public constructor(
        private readonly _retryIntervals: number[],
        private readonly _postRetryInterval: number | null,
    ) {}

    public nextRetryDelayInMilliseconds(
        retryContext: RetryContext,
    ): number | null {
        if (
            retryContext.retryReason.message.includes(
                ErrorCode.AUTH_TOKEN_MISSING,
            )
        ) {
            handleRefresh();
        }

        return (
            this._retryIntervals.at(retryContext.previousRetryCount) ??
            this._postRetryInterval
        );
    }
}
