"use client";

import {
    forwardRef,
    ForwardRefRenderFunction,
    ReactNode,
    useImperativeHandle,
    useState,
} from "react";
import { twMerge } from "tailwind-merge";

export interface PopupRef {
    open(): void;
}

const Popup: ForwardRefRenderFunction<
    PopupRef,
    {
        className?: string;
        children?: ReactNode;
        "data-testid"?: string;
    }
> = ({ className, children, "data-testid": testId }, ref) => {
    const [isOpen, setIsOpen] = useState(false);

    const openPopup = () => setIsOpen(true);
    const closePopup = () => setIsOpen(false);

    useImperativeHandle(ref, () => ({
        open: openPopup,
    }));

    if (!isOpen) return null;
    return (
        <div
            className="fixed inset-0 z-50 flex min-h-screen items-center justify-center bg-black/60 p-4"
            onClick={closePopup}
            data-testid="popupBackground"
        >
            <div
                className={twMerge(
                    `bg-card shadow-x4 relative flex h-min max-h-full w-full max-w-md flex-col gap-3
                    overflow-auto rounded-2xl p-8`,
                    className,
                )}
                onClick={(e) => e.stopPropagation()}
                data-testid={testId ?? "popup"}
            >
                <button
                    onClick={closePopup}
                    aria-label="Close popup"
                    className="hover:text-text/80 absolute top-2 right-4 cursor-pointer text-4xl"
                    data-testid="closePopup"
                >
                    ×
                </button>

                {children}
            </div>
        </div>
    );
};
export default forwardRef(Popup);
