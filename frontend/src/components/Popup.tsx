import { ReactNode } from "react";
import { twMerge } from "tailwind-merge";

const Popup = ({
    closePopup,
    className,
    children,
}: {
    closePopup: () => void;
    className?: string;
    children: ReactNode;
}) => {
    return (
        <div
            className="fixed inset-0 z-50 flex min-h-screen items-center justify-center bg-black/60 p-4"
            onClick={closePopup}
            data-testid="popupBackground"
        >
            <div
                className={twMerge(
                    `bg-background shadow-x4 relative flex h-min max-h-full w-full max-w-md flex-col
                    gap-3 overflow-auto rounded-2xl p-8`,
                    className,
                )}
                onClick={(e) => e.stopPropagation()}
                data-testid="popup"
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
export default Popup;
