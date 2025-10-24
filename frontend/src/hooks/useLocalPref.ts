import { useEffect, useState } from "react";

export default function useLocalPref<T>(
    localStorageName: string,
    defaultValue: T,
): [T, (newValue: T) => void] {
    const [value, setValue] = useState<T>(defaultValue);

    useEffect(() => {
        const storageValue = localStorage.getItem(localStorageName);
        if (!storageValue) return;

        setValue(JSON.parse(storageValue));
    }, [localStorageName]);

    function setNewValue(newValue: T) {
        setValue(newValue);
        localStorage.setItem(localStorageName, JSON.stringify(newValue));
    }

    return [value, setNewValue];
}
