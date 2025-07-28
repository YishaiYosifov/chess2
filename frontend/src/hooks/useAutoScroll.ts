import { useEffect } from "react";

export default function useAutoScroll(
    ref: React.RefObject<HTMLElement | null>,
    deps: React.DependencyList,
) {
    useEffect(() => {
        const el = ref.current;
        if (!el) return;

        const scrollDistance = el.scrollHeight - el.scrollTop - el.clientHeight;
        if (scrollDistance < 50)
            el.scrollTo({ top: el.scrollHeight, behavior: "smooth" });
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, deps);
}
