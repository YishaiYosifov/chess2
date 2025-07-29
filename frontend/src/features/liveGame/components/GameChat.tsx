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
import { useAuthedUser } from "@/features/auth/hooks/useSessionUser";

interface ChatMessage {
    sender: string;
    message: string;
}

const GameChat = () => {
    const [chatMessages, setChatMessages] = useState<ChatMessage[]>([]);
    const [message, setMessage] = useState("");
    const [isOnCooldown, setIsOnCooldown] = useState(false);
    const [isSending, setIsSending] = useState(false);
    const isGuest = useAuthedUser() === null;

    const chatRef = useRef<HTMLDivElement>(null);
    useAutoScroll(chatRef, [chatMessages]);

    const router = useRouter();
    const gameToken = useLiveChessStore((x) => x.gameToken);
    const sendGameEvent = useGameEmitter(gameToken);

    useGameEvent(gameToken, "ChatMessageDeliveredAsync", (cooldownLeftMs) => {
        setIsSending(false);

        if (!cooldownLeftMs) return;
        setIsOnCooldown(true);
        setTimeout(() => setIsOnCooldown(false), cooldownLeftMs);
    });

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

        if (isSending || !message.trim()) return;

        setIsSending(true);
        sendGameEvent("SendChatAsync", gameToken, message);
        setMessage("");
    }

    function getPlaceholderMessage(): string {
        if (isGuest) return "Sign Up to Chat!";
        if (isOnCooldown) return "Too fast, slow down...";
        return "Send a Message...";
    }

    return (
        <Card className="flex-col gap-3">
            <div className="h-full w-full overflow-auto" ref={chatRef}>
                {chatMessages.map((chatMessage, i) => (
                    <p key={i}>
                        <span
                            data-testid="gameChatUser"
                            className="cursor-pointer"
                            onClick={() =>
                                router.push(`/profile/${chatMessage.sender}`)
                            }
                        >
                            {chatMessage.sender}:
                        </span>{" "}
                        <span
                            data-testid="gameChatMessage"
                            className="text-gray-400"
                        >
                            {chatMessage.message}
                        </span>
                    </p>
                ))}
            </div>
            <form onSubmit={onChatSend}>
                <Input
                    data-testid="gameChatInput"
                    className="bg-white/5 text-white"
                    placeholder={getPlaceholderMessage()}
                    value={message}
                    onChange={(e) => setMessage(e.target.value)}
                    disabled={isGuest || isOnCooldown}
                />
            </form>
        </Card>
    );
};
export default GameChat;
