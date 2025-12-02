"use client";

import { CSSTransition, TransitionGroup } from "react-transition-group";

import Card from "@/components/ui/Card";
import OpenSeekItem from "./OpenSeekItem";
import { useEffect, useRef, useState } from "react";
import { OpenSeek, SeekKeyStr } from "@/features/lobby/lib/types";
import { SeekKeyToStr } from "@/features/lobby/lib/matchmakingKeys";
import React from "react";
import constants from "@/lib/constants";
import {
    useOpenSeekEmitter,
    useOpenSeekEvent,
} from "@/features/lobby/hooks/useOpenSeekHub";

const OpenSeekDirectory = () => {
    const [openSeeks, setOpenSeeks] = useState<Record<SeekKeyStr, OpenSeek>>(
        {},
    );
    const openSeekRefs = useRef<
        Map<SeekKeyStr, React.RefObject<HTMLDivElement | null>>
    >(new Map());

    const noSeeksRef = useRef<HTMLParagraphElement | null>(null);
    const [showNoSeeksText, setShowNoSeeksText] = useState(true);
    useEffect(() => {
        if (Object.keys(openSeeks).length !== 0) {
            setShowNoSeeksText(false);
            return;
        }

        const timer = setTimeout(() => setShowNoSeeksText(true), 300);
        return () => clearTimeout(timer);
    }, [openSeeks]);

    const sendOpenSeekEvent = useOpenSeekEmitter();

    useEffect(() => {
        const interval = setInterval(
            () => sendOpenSeekEvent("SubscribeAsync"),
            constants.OPEN_SEEK_RESUBSCRIBE_INTERAVAL_MS,
        );
        sendOpenSeekEvent("SubscribeAsync");
        return () => clearInterval(interval);
    }, [sendOpenSeekEvent]);

    useOpenSeekEvent("NewOpenSeeksAsync", (newOpenSeeks) => {
        setOpenSeeks((prev) => {
            const updated = { ...prev };
            for (const seek of newOpenSeeks) {
                updated[
                    SeekKeyToStr({ userId: seek.userId, pool: seek.pool })
                ] = seek;
            }
            return updated;
        });
    });

    useOpenSeekEvent("OpenSeekEndedAsync", (userId, pool) => {
        const key = SeekKeyToStr({ userId, pool });
        openSeekRefs.current.delete(key);
        setOpenSeeks((prev) => {
            // eslint-disable-next-line @typescript-eslint/no-unused-vars
            const { [key]: _, ...rest } = prev;
            return rest;
        });
    });

    function getOpenSeekRef(
        key: SeekKeyStr,
    ): React.RefObject<HTMLDivElement | null> {
        let ref = openSeekRefs.current.get(key);
        if (ref) return ref;

        ref = React.createRef<HTMLDivElement>();
        openSeekRefs.current.set(key, ref);
        return ref;
    }

    return (
        <Card className="min-h-40 flex-1">
            <h2 className="text-center text-3xl">Open Challenges</h2>

            <div className="flex h-full max-h-96 flex-col gap-3 md:max-h-full">
                <CSSTransition
                    in={showNoSeeksText}
                    timeout={{ enter: 200, exit: 0 }}
                    classNames={{
                        enter: "opacity-0",
                        enterActive: "opacity-100 transition-all duration-200",
                    }}
                    nodeRef={noSeeksRef}
                    unmountOnExit
                >
                    <p
                        data-testid="noOpenChallengesText"
                        className="mt-4 text-center text-gray-500"
                        ref={noSeeksRef}
                    >
                        No open challenges, join a pool to appear here for
                        others
                    </p>
                </CSSTransition>

                <TransitionGroup duration={300}>
                    {Object.entries(openSeeks).map(([key, seek]) => {
                        const seekKey = key as SeekKeyStr;
                        const nodeRef = getOpenSeekRef(seekKey);

                        return (
                            <CSSTransition
                                key={key}
                                classNames={{
                                    enter: "opacity-0 -translate-x-10",
                                    enterActive:
                                        "opacity-100 translate-x-0 transition-all duration-300",
                                    exitActive:
                                        "opacity-0 -translate-x-10 transition-all duration-300",
                                }}
                                timeout={300}
                                nodeRef={nodeRef}
                            >
                                <div ref={nodeRef}>
                                    <OpenSeekItem seek={seek} />
                                </div>
                            </CSSTransition>
                        );
                    })}
                </TransitionGroup>
            </div>
        </Card>
    );
};
export default OpenSeekDirectory;
