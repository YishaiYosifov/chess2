import { useRef, useState } from "react";
import { useRouter } from "next/navigation";

import { useAuthedUser } from "@/features/auth/hooks/useSessionUser";
import { useGameEmitter, useGameEvent } from "../hooks/useGameHub";
import useLiveChessStore from "../hooks/useLiveChessStore";
import InputField from "@/components/ui/InputField";
import useAutoScroll from "@/hooks/useAutoScroll";
import Button from "@/components/ui/Button";
import Card from "@/components/ui/Card";

interface ChatMessage {
    sender: string;
    message: string;
}

const GameChat = ({ initialShowChat }: { initialShowChat: boolean }) => {
    const [chatMessages, setChatMessages] = useState<ChatMessage[]>([]);
    const [currentMessage, setCurrentMessage] = useState("");

    const [showChat, setShowChat] = useState(initialShowChat);

    const [isOnCooldown, setIsOnCooldown] = useState(false);
    const [isSending, setIsSending] = useState(false);
    const isGuest = useAuthedUser() === null;
    const canType = !isGuest && !isOnCooldown;

    const chatRef = useRef<HTMLDivElement>(null);
    useAutoScroll(chatRef, [chatMessages]);

    const gameToken = useLiveChessStore((x) => x.gameToken);
    const sendGameEvent = useGameEmitter(gameToken);
    const router = useRouter();

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
        if (isSending || !currentMessage.trim()) return;

        setIsSending(true);
        sendGameEvent("SendChatAsync", gameToken, currentMessage);
        setCurrentMessage("");
        setShowChat(true);
    }

    function getPlaceholderMessage(): string {
        if (isGuest) return "Sign Up to Chat!";
        if (isOnCooldown) return "Too fast, slow down...";
        return "Send a Message...";
    }

    return (
        <Card className="relative overflow-y-auto">
            <div
                className="h-full w-full overflow-auto break-words"
                ref={chatRef}
            >
                {!showChat && chatMessages.length > 0 && (
                    <Button
                        onClick={() => setShowChat(true)}
                        className="w-full"
                        data-testid="showChatMessagesButton"
                    >
                        Received {chatMessages.length} messages, show?
                    </Button>
                )}

                {showChat &&
                    chatMessages.length > 0 &&
                    chatMessages.map((message, i) => (
                        <p key={i} data-testid="gameChatMessage">
                            <span
                                data-testid="gameChatUser"
                                className="cursor-pointer"
                                onClick={() =>
                                    router.push(`/profile/${message.sender}`)
                                }
                            >
                                {message.sender}:
                            </span>{" "}
                            <span
                                data-testid="gameChatMessageContent"
                                className="text-gray-400"
                            >
                                {message.message}
                            </span>
                        </p>
                    ))}
            </div>
            <form onSubmit={onChatSend}>
                <InputField
                    data-testid="gameChatInput"
                    className="text-text bg-white/5"
                    value={currentMessage}
                    placeholder={getPlaceholderMessage()}
                    onChange={(e) => setCurrentMessage(e.target.value)}
                    disabled={!canType}
                    maxLength={200}
                />
            </form>
        </Card>
    );
};
export default GameChat;
