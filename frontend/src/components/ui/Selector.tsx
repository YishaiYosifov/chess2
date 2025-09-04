import { useMemo, useState } from "react";
import Button from "./Button";
import clsx from "clsx";

type Option<T> = {
    label: string;
    value: T;
};

interface SelectorProps<T> {
    className?: string;
    id?: string;
    options: Option<T>[];
    defaultValue?: T;
    onChange?: (option: T) => void;
}

const Selector = <T,>({
    options,
    defaultValue,
    onChange,
}: SelectorProps<T>) => {
    let initialIndex = useMemo(
        () =>
            defaultValue !== undefined
                ? options.findIndex((o) => o.value === defaultValue)
                : 0,
        [options, defaultValue],
    );
    if (initialIndex === -1) initialIndex = 0;

    const [selectedIndex, setSelectedIndex] = useState(initialIndex);

    const select = (index: number) => {
        setSelectedIndex(index);
        onChange?.(options[index].value);
    };

    return (
        <div className="flex w-full gap-3">
            {options.map((option, i) => (
                <Button
                    key={i}
                    className={clsx(
                        "w-full disabled:cursor-default",
                        i === selectedIndex && "border-secondary border-3",
                    )}
                    disabled={i === selectedIndex}
                    onClick={() => select(i)}
                >
                    {option.label}
                </Button>
            ))}
        </div>
    );
};

export default Selector;
