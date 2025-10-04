"use client";

import { ClipboardIcon } from "@heroicons/react/24/outline";
import { useEffect, useState } from "react";
import Link from "next/link";
import QRCode from "qrcode";

import CountdownText from "@/components/CountdownText";
import Button from "@/components/ui/Button";
import Card from "@/components/ui/Card";
import InputField from "@/components/ui/InputField";
import TimeControlIcon from "@/features/lobby/Components/TimeControlIcon";
import { ChallengeRequest, PoolType } from "@/lib/apiClient";
import constants from "@/lib/constants";
import Image from "next/image";

const ChallengeSidebar = ({ challenge }: { challenge: ChallengeRequest }) => {
    const [isExpired, setIsExpired] = useState(false);
    const [currentUrl, setCurrentUrl] = useState<string>();
    const [qrCodeB64, setQrCodeB64] = useState<string>();

    const isRated = challenge.pool.poolType == PoolType.RATED;
    const expiresAt = new Date(challenge.expiresAt + "Z");

    async function copyChallengeLink() {
        if (currentUrl) navigator.clipboard.writeText(currentUrl);
    }

    useEffect(() => {
        if (typeof window === "undefined") return;

        setCurrentUrl(window.location.href);
        QRCode.toDataURL(window.location.href).then(setQrCodeB64);
    }, []);

    return (
        <Card className="h-full w-full min-w-xs items-center gap-10 p-5 lg:max-w-sm">
            <div className="flex flex-col items-center gap-1">
                <h1 className="text-4xl font-bold">Challenge</h1>

                <div className="flex items-center gap-2">
                    <TimeControlIcon
                        timeControl={challenge.timeControl}
                        className="h-8 w-8"
                    />
                    <p className="text-2xl font-semibold">
                        {challenge.pool.timeControl.baseSeconds / 60}+
                        {challenge.pool.timeControl.incrementSeconds}{" "}
                        {isRated ? "Rated" : "Casual"}
                    </p>
                </div>
            </div>

            <div className="flex w-full flex-col items-center gap-3">
                <p className="text-text/70 text-lg">
                    Invite someone to play via:
                </p>

                <InputField
                    defaultValue={currentUrl}
                    readOnly
                    icon={<ClipboardIcon onClick={copyChallengeLink} />}
                    className="flex-1"
                />
                {qrCodeB64 && (
                    <div className="bg-background flex w-full justify-center rounded-md border border-white/20 p-3">
                        <Image
                            src={qrCodeB64}
                            alt="challenge qr code"
                            width="150"
                            height="150"
                        />
                    </div>
                )}
            </div>

            <div className="mt-auto flex w-full flex-col items-center gap-3">
                {isExpired ? (
                    <>
                        <p className="text-error text-xl">Challenge Expired!</p>
                        <Link href={constants.PATHS.PLAY} className="w-full">
                            <Button className="w-full">New Opponent</Button>
                        </Link>
                    </>
                ) : (
                    <>
                        <p className="text-xl font-medium">
                            Waiting for opponent...
                        </p>
                        <Button className="w-full">Cancel</Button>
                        <CountdownText
                            getTimeUntil={() => expiresAt}
                            onDateReached={() => setIsExpired(true)}
                        >
                            {({ countdown }) => (
                                <p className="text-text/70 text-sm">
                                    Expires in {countdown}
                                </p>
                            )}
                        </CountdownText>
                    </>
                )}
            </div>
        </Card>
    );
};
export default ChallengeSidebar;
