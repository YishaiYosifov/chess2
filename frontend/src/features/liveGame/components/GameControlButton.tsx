import { XMarkIcon } from "@heroicons/react/24/solid";
import { useRef, useState } from "react";
import clsx from "clsx";

const GameControlButton = ({
    icon,
    needsConfirmation,
    onClick,
    children,
    ...props
}: React.ButtonHTMLAttributes<HTMLButtonElement> & {
    icon: React.ElementType;
    needsConfirmation?: boolean;
}) => {
    const Component = icon;

    const [isConfirming, setIsConfirming] = useState(false);
    const timeoutRef = useRef<NodeJS.Timeout>(null);

    function confirmClick(
        event: React.MouseEvent<HTMLButtonElement, MouseEvent>,
    ) {
        if (!needsConfirmation || isConfirming) {
            setIsConfirming(false);
            onClick?.(event);
            return;
        }

        setIsConfirming(true);

        if (timeoutRef.current) clearTimeout(timeoutRef.current);
        timeoutRef.current = setTimeout(() => setIsConfirming(false), 3000);
    }

    return (
        <div className="flex h-full w-full gap-1">
            <button
                data-testid="gameControlButton"
                className={clsx(
                    `text-md lg:text-md flex w-full cursor-pointer items-center justify-center gap-1
                    rounded-md p-2 text-nowrap`,
                    isConfirming
                        ? "border-b-4 border-orange-800 bg-orange-600 p-1 hover:brightness-75"
                        : `enabled:hover:bg-secondary enabled:hover:text-neutral-900
                            disabled:cursor-not-allowed disabled:brightness-75`,
                )}
                onClick={confirmClick}
                {...props}
            >
                <Component className="h-10 w-10" />
                {children}
            </button>

            {isConfirming && (
                <button
                    data-testid="gameControlCancelButton"
                    type="button"
                    onClick={() => setIsConfirming(false)}
                    className="hover:text-secondary cursor-pointer p-1"
                    title="Cancel"
                    aria-label="Cancel confirmation"
                >
                    <XMarkIcon className="h-6 w-6" />
                </button>
            )}
        </div>
    );
};
export default GameControlButton;
