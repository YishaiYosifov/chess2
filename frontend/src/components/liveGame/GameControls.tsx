import { ScaleIcon } from "@heroicons/react/24/outline";
import { FlagIcon } from "@heroicons/react/24/solid";

import Button from "../helpers/Button";
import Card from "../helpers/Card";

const GameControls = () => {
    return (
        <Card className="gap-5">
            <Button className="flex w-full justify-center gap-2">
                <FlagIcon className="size-6" /> Resign
            </Button>
            <Button className="flex w-full justify-center gap-2">
                <ScaleIcon className="size-6" /> Draw
            </Button>
        </Card>
    );
};
export default GameControls;
