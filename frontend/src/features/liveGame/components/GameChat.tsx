import Card from "@/components/ui/Card";
import Input from "@/components/ui/Input";

const GameChat = () => {
    return (
        <Card className="flex-col gap-3">
            <div className="h-full w-full overflow-auto">
                <p>
                    user1: <span className="text-gray-400">hello</span>
                </p>
                <p>
                    user2: <span className="text-gray-400">hello</span>
                </p>
                <p>
                    user1: <span className="text-gray-400">hello</span>
                </p>
                <p>
                    user2: <span className="text-gray-400">hello</span>
                </p>
                <p>
                    user1: <span className="text-gray-400">hello</span>
                </p>
                <p>
                    user2: <span className="text-gray-400">hello</span>
                </p>
                <p>
                    user1: <span className="text-gray-400">hello</span>
                </p>
                <p>
                    user2: <span className="text-gray-400">hello</span>
                </p>
            </div>
            <Input
                className="bg-white/5 text-white"
                placeholder="Send a Message..."
            />
        </Card>
    );
};
export default GameChat;
