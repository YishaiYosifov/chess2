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
        <Card className="h-full flex-col gap-5 overflow-auto">
            <h2 className="text-3xl">Open Challenges</h2>

            <div className="flex h-full flex-col gap-3 overflow-auto">
                <TransitionGroup duration={300}>
                    {Object.entries(openSeeks).map(([key, seek]) => {
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
                    })}
                </TransitionGroup>
            </div>
        </Card>
    );
};
export default OpenSeekDirectory;
