import { ClipboardIcon } from "@heroicons/react/24/outline";
import { useEffect, useState } from "react";
import Image from "next/image";
import QRCode from "qrcode";

import InputField from "@/components/ui/InputField";
import clsx from "clsx";
import useChallengeStore from "@/features/challenges/hooks/useChallengeStore";
import ChallengeStatusText from "../ChallengeStatusText";

const OpenChallengeView = () => {
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
            <ChallengeStatusText
                activeText="Invite someone to play via:"
                activeClassName="text-text/70 text-lg"
                overClassName="text-error text-lg"
            />

            <InputField
                data-testid="openChallengeViewInput"
                defaultValue={currentUrl}
                readOnly
                disabled={isOver}
                icon={
                    <ClipboardIcon
                        onClick={copyChallengeLink}
                        data-testid="openChallengeViewCopy"
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
                        data-testid="openChallengeViewQRCode"
                        alt="challenge qr code"
                        width={150}
                        height={150}
                    />
                </div>
            )}
        </>
    );
};
export default OpenChallengeView;
