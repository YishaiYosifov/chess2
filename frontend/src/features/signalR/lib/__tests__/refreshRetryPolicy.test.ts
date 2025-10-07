import { RetryContext } from "@microsoft/signalr";
import RefreshRetryPolicy from "../refreshRetryPolicy";
import { ErrorCode, refresh } from "@/lib/apiClient";

vi.mock("@/lib/apiClient/definition");

describe("RefreshRetryPolicy", () => {
    const retryIntervals = [100, 200, 300];
    const postRetryInterval = 500;

    const refreshMock = vi.mocked(refresh);

    let policy: RefreshRetryPolicy;

    beforeEach(() => {
        policy = new RefreshRetryPolicy(retryIntervals, postRetryInterval);
        refreshMock.mockResolvedValue({
            response: new Response(),
            data: undefined,
        });
    });

    it("should return the correct retry interval based on previousRetryCount", () => {
        const ctx: Partial<RetryContext> = {
            previousRetryCount: 1,
            retryReason: new Error(),
        };
        const delay = policy.nextRetryDelayInMilliseconds(ctx as RetryContext);
        expect(delay).toBe(200);
    });

    it("should return postRetryInterval when previousRetryCount exceeds retryIntervals length", () => {
        const ctx: Partial<RetryContext> = {
            previousRetryCount: 5,
            retryReason: new Error(),
        };
        const delay = policy.nextRetryDelayInMilliseconds(ctx as RetryContext);
        expect(delay).toBe(postRetryInterval);
    });

    it("should call handleRefresh when retryReason contains AUTH_TOKEN_MISSING", () => {
        const ctx: Partial<RetryContext> = {
            previousRetryCount: 0,
            retryReason: new Error(
                `{"errorCode":"${ErrorCode.AUTH_TOKEN_MISSING}"}`,
            ),
        };
        policy.nextRetryDelayInMilliseconds(ctx as RetryContext);
        expect(refreshMock).toHaveBeenCalledTimes(1);
    });

    it("should not call handleRefresh for other errors", () => {
        const ctx: Partial<RetryContext> = {
            previousRetryCount: 0,
            retryReason: new Error(),
        };
        policy.nextRetryDelayInMilliseconds(ctx as RetryContext);
        expect(refreshMock).not.toHaveBeenCalled();
    });

    it("should return null if postRetryInterval is null and retries exhausted", () => {
        policy = new RefreshRetryPolicy(retryIntervals, null);
        const ctx: Partial<RetryContext> = {
            previousRetryCount: 10,
            retryReason: new Error(),
        };
        const delay = policy.nextRetryDelayInMilliseconds(ctx as RetryContext);
        expect(delay).toBeNull();
    });
});
