import { useRef, useState } from "react";

import Card from "@/components/ui/Card";
import Input from "@/components/ui/Input";
import { useLiveChessStore } from "../hooks/useLiveChessStore";
import {
    useGameEmitter,
    useGameEvent,
} from "@/features/signalR/hooks/useSignalRHubs";
import { useRouter } from "next/navigation";
import useAutoScroll from "@/hooks/useAutoScroll";

interface ChatMessage {
    sender: string;
    message: string;
}

const GameChat = () => {
    const [chatMessages, setChatMessages] = useState<ChatMessage[]>([]);
    const [message, setMessage] = useState("");

    const chatRef = useRef<HTMLDivElement>(null);
    useAutoScroll(chatRef, [chatMessages]);

    const router = useRouter();

    const gameToken = useLiveChessStore((x) => x.gameToken);
    const sendGameEvent = useGameEmitter(gameToken);

    useGameEvent(gameToken, "ChatMessageAsync", (sender, message) => {
        const chatMessage: ChatMessage = {
            sender,
            message,
        };
        setChatMessages((prev) => [...prev, chatMessage]);
    });
    // this event is fired for backend tests, but we don't really care about it
    // this is here to supress the missing event warning
    useGameEvent(gameToken, "ChatConnectedAsync", () => {});

    async function onChatSend(event: React.FormEvent) {
        event.preventDefault();

        if (!message || message.trim().length === 0) return;

        sendGameEvent("SendChatAsync", gameToken, message);
        setMessage("");
    }

    return (
        <Card className="flex-col gap-3">
            <div className="h-full w-full overflow-auto" ref={chatRef}>
                {chatMessages.map((chatMessage, i) => (
                    <p key={i}>
                        <span
                            className="cursor-pointer"
                            onClick={() =>
                                router.push(`/profile/${chatMessage.sender}`)
                            }
                        >
                            {chatMessage.sender}:
                        </span>{" "}
                        <span className="text-gray-400">
                            {chatMessage.message}
                        </span>
                    </p>
                ))}
            </div>
            <form onSubmit={onChatSend}>
                <Input
                    className="bg-white/5 text-white"
                    placeholder="Send a Message..."
                    value={message}
                    onChange={(e) => setMessage(e.target.value)}
                />
            </form>
        </Card>
    );
};
export default GameChat;
