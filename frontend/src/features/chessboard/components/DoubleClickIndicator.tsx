import { CursorArrowRippleIcon } from "@heroicons/react/24/solid";
import { forwardRef, useImperativeHandle, useRef, useState } from "react";

export interface DoubleClickRef {
    trigger: () => Promise<void>;
    cancel: () => Promise<void>;
}

const DoubleClickIndicator: React.ForwardRefRenderFunction<
    DoubleClickRef,
    unknown
> = (_, ref) => {
    const [isTriggered, setIsTriggered] = useState(false);

    const cancelTimerRef = useRef<NodeJS.Timeout>(null);

    async function trigger() {
        await cancel();
        setIsTriggered(true);
        cancelTimerRef.current = setTimeout(cancel, 700);
    }

    async function cancel() {
        setIsTriggered(false);

        if (cancelTimerRef.current) clearTimeout(cancelTimerRef.current);
        await Promise.resolve();
    }

    useImperativeHandle(ref, () => ({
        trigger,
        cancel,
    }));

    if (!isTriggered) return null;

    return (
        <div className="absolute inset-0 flex items-center justify-center">
            <CursorArrowRippleIcon
                className="animate-double-tap mt-auto ml-auto h-6 w-6 text-black"
                data-testid="doubleClickIndicator"
            />
        </div>
    );
};
export default forwardRef(DoubleClickIndicator);
