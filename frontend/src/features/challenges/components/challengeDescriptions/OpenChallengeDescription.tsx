import { ClipboardIcon } from "@heroicons/react/24/outline";
import { useEffect, useState } from "react";
import Image from "next/image";
import QRCode from "qrcode";

import InputField from "@/components/ui/InputField";
import useChallengeStore from "../../hooks/useChallengeStore";
import clsx from "clsx";

const OpenChallengeDescription = () => {
    const [currentUrl, setCurrentUrl] = useState<string>();
    const [qrCodeB64, setQrCodeB64] = useState<string>();

    const { isCancelled, isExpired } = useChallengeStore((x) => ({
        isCancelled: x.isCancelled,
        isExpired: x.isExpired,
    }));
    const isOver = isCancelled || isExpired;

    async function copyChallengeLink() {
        if (currentUrl && !isOver)
            await navigator.clipboard.writeText(currentUrl);
    }

    useEffect(() => {
        if (typeof window === "undefined") return;

        setCurrentUrl(window.location.href);
        QRCode.toDataURL(window.location.href).then(setQrCodeB64);
    }, []);

    return (
        <>
            {isOver ? (
                <p
                    data-testid="openChallengeDescriptionTitle"
                    className="text-error text-lg"
                >
                    Challenge {isCancelled ? "Cancelled" : "Expired"}
                </p>
            ) : (
                <p
                    data-testid="openChallengeDescriptionTitle"
                    className={"text-text/70 text-lg"}
                >
                    Invite someone to play via:
                </p>
            )}

            <InputField
                data-testid="openChallengeDescriptionInput"
                defaultValue={currentUrl}
                readOnly
                disabled={isOver}
                icon={
                    <ClipboardIcon
                        onClick={copyChallengeLink}
                        data-testid="openChallengeDescriptionCopy"
                        className={clsx(
                            isOver && "cursor-not-allowed brightness-50",
                        )}
                    />
                }
                className="flex-1"
            />
            {qrCodeB64 && (
                <div
                    className={clsx(
                        "bg-background flex w-full justify-center rounded-md border border-white/20 p-3",
                        isOver && "cursor-not-allowed opacity-50",
                    )}
                >
                    <Image
                        src={qrCodeB64}
                        data-testid="openChallengeDescriptionQRCode"
                        alt="challenge qr code"
                        width={150}
                        height={150}
                    />
                </div>
            )}
        </>
    );
};
export default OpenChallengeDescription;
