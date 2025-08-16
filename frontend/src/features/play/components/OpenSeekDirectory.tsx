"use client";

import { CSSTransition, TransitionGroup } from "react-transition-group";

import Card from "@/components/ui/Card";
import OpenSeekItem from "./OpenSeek";
import {
    useOpenSeekEmitter,
    useOpenSeekEvent,
} from "@/features/signalR/hooks/useSignalRHubs";
import { useEffect, useRef, useState } from "react";
import { OpenSeek, SeekKeyStr } from "@/features/lobby/lib/types";
import { SeekKeyToStr } from "@/features/lobby/lib/matchmakingKeys";
import React from "react";

const OpenSeekDirectory = () => {
    const [openSeeks, setOpenSeeks] = useState<Record<SeekKeyStr, OpenSeek>>(
        {},
    );
    const nodeRefs = useRef<
        Map<string, React.RefObject<HTMLDivElement | null>>
    >(new Map());
    const test = useRef<HTMLParagraphElement | null>(null);

    const sendOpenSeekEvent = useOpenSeekEmitter();

    useEffect(
        () => void sendOpenSeekEvent("SubscribeAsync"),
        [sendOpenSeekEvent],
    );

    useOpenSeekEvent("NewOpenSeeksAsync", (newOpenSeeks) => {
        setOpenSeeks((prev) => {
            const updated = { ...prev };
            for (const seek of newOpenSeeks) {
                if (Object.keys(updated).length >= 10) break;
                updated[
                    SeekKeyToStr({ userId: seek.userId, pool: seek.pool })
                ] = seek;
            }
            return updated;
        });
    });

    useOpenSeekEvent("OpenSeekEndedAsync", (userId, pool) => {
        const key = SeekKeyToStr({ userId, pool });
        nodeRefs.current.delete(key);
        setOpenSeeks((prev) => {
            // eslint-disable-next-line @typescript-eslint/no-unused-vars
            const { [key]: _, ...rest } = prev;
            return rest;
        });
    });

    return (
        <Card className="h-full min-h-60 flex-col gap-2 overflow-auto">
            <h2 className="text-center text-3xl">Open Challenges</h2>

            <div className="flex h-full max-h-96 flex-col gap-3 overflow-auto md:max-h-full">
                <TransitionGroup duration={300}>
                    {Object.keys(openSeeks).length === 0 ? (
                        <CSSTransition
                            timeout={300}
                            classNames={{
                                enter: "opacity-0",
                                enterActive: "opacity-100 duration-300",
                                exitActive: "opacity-0 duration-300",
                            }}
                            nodeRef={test}
                        >
                            <p
                                className="mt-4 text-center text-gray-500"
                                ref={test}
                            >
                                No open challenges, join a pool to appear here
                                for others
                            </p>
                        </CSSTransition>
                    ) : (
                        Object.entries(openSeeks).map(([key, seek]) => {
                            if (!nodeRefs.current.has(key)) {
                                nodeRefs.current.set(
                                    key,
                                    React.createRef<HTMLDivElement | null>(),
                                );
                            }
                            const nodeRef = nodeRefs.current.get(key);

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
                        })
                    )}
                </TransitionGroup>
            </div>
        </Card>
    );
};
export default OpenSeekDirectory;
