import { useMemo, useState } from "react";
import Button from "./Button";

type SelectorProps<T extends Record<string, unknown>> = {
    options: T;
    defaultValue?: T[keyof T];
    onSelect?: (option: T[keyof T]) => void;
    id?: string;
    className?: string;
};

function Selector<T extends Record<string, unknown>>({
    options,
    defaultValue,
    onSelect,
    id,
}: SelectorProps<T>) {
    const initialKey = useMemo(
        () =>
            defaultValue
                ? Object.keys(options).find((k) => options[k] === defaultValue)
                : undefined,
        [defaultValue, options],
    );

    const [selectedKey, setSelectedKey] = useState<keyof T | undefined>(
        initialKey,
    );

    function select(key: keyof T) {
        setSelectedKey(key);
        onSelect?.(options[key]);
    }

    return (
        <div className="flex w-full gap-3" id={id}>
            {Object.keys(options).map((key) => (
                <Button
                    key={key}
                    className="w-full"
                    disabled={key === selectedKey}
                    onClick={() => select(key as keyof T)}
                >
                    {key}
                </Button>
            ))}
        </div>
    );
}

export default Selector;
