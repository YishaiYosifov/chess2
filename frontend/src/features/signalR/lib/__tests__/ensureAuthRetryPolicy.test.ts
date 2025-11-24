import { RetryContext } from "@microsoft/signalr";
import EnsureAuthRetryPolicy from "../ensureAuthRetryPolicy";
import { ErrorCode } from "@/lib/apiClient";
import ensureAuth from "@/features/auth/lib/ensureAuth";

vi.mock("@/features/auth/lib/ensureAuth");

describe("EnsureAuthRetryPolicy", () => {
    const retryIntervals = [100, 200, 300];
    const postRetryInterval = 500;

    const ensureAuthMock = vi.mocked(ensureAuth);

    let policy: EnsureAuthRetryPolicy;

    beforeEach(() => {
        policy = new EnsureAuthRetryPolicy(retryIntervals, postRetryInterval);
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

    it("should call ensureAuth when retryReason contains missing token", () => {
        const ctx: Partial<RetryContext> = {
            previousRetryCount: 0,
            retryReason: new Error(
                `{"errorCode":"${ErrorCode.AUTH_TOKEN_MISSING}"}`,
            ),
        };
        policy.nextRetryDelayInMilliseconds(ctx as RetryContext);
        expect(ensureAuthMock).toHaveBeenCalledTimes(1);
    });

    it("should not call ensureAuth for other errors", () => {
        const ctx: Partial<RetryContext> = {
            previousRetryCount: 0,
            retryReason: new Error(),
        };
        policy.nextRetryDelayInMilliseconds(ctx as RetryContext);
        expect(ensureAuthMock).not.toHaveBeenCalled();
    });

    it("should return null if postRetryInterval is null and retries exhausted", () => {
        policy = new EnsureAuthRetryPolicy(retryIntervals, null);
        const ctx: Partial<RetryContext> = {
            previousRetryCount: 10,
            retryReason: new Error(),
        };
        const delay = policy.nextRetryDelayInMilliseconds(ctx as RetryContext);
        expect(delay).toBeNull();
    });
});
