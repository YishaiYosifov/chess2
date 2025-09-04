import { useEffect, useMemo, useState } from "react";
import Button from "./Button";
import clsx from "clsx";

type Option<T> = {
    label: string;
    value: T;
};

interface SelectorProps<T> {
    id?: string;
    name?: string;
    options: Option<T>[];
    value?: T;
    onChange?: (e: { target: { name?: string; value: T } }) => void;
    onBlur?: React.FocusEventHandler<HTMLDivElement>;
}

const Selector = <T,>({
    id,
    name,
    options,
    value,
    onChange,
}: SelectorProps<T>) => {
    const initialIndex = useMemo(
        () =>
            value !== undefined
                ? options.findIndex((o) => o.value === value)
                : 0,
        [options, value],
    );

    const [selectedIndex, setSelectedIndex] = useState(
        initialIndex === -1 ? 0 : initialIndex,
    );

    useEffect(() => {
        if (value === undefined) return;

        const idx = options.findIndex((o) => o.value === value);
        if (idx !== -1 && idx !== selectedIndex) setSelectedIndex(idx);
    }, [value, options, selectedIndex]);

    const select = (index: number) => {
        setSelectedIndex(index);
        const selectedValue = options[index].value;

        onChange?.({
            target: {
                name,
                value: selectedValue,
            },
        });
    };

    return (
        <div id={id} className="flex w-full flex-wrap gap-3">
            {options.map((option, i) => (
                <Button
                    key={i}
                    className={clsx(
                        "flex-1 text-nowrap disabled:cursor-default",
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
