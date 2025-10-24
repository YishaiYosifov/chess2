import { useEffect, useRef } from "react";

export default function useAutoScroll(
    ref: React.RefObject<HTMLElement | null>,
    deps: React.DependencyList,
) {
    const isAtBottomRef = useRef(true);
    const isAutoScrolling = useRef(false);

    useEffect(() => {
        const el = ref.current;
        if (!el) return;

        function handleScroll() {
            if (!el) return;

            const scrollDistance =
                el.scrollHeight - el.scrollTop - el.clientHeight;
            const isAtBottom = scrollDistance < 5;

            if (isAtBottom) {
                isAtBottomRef.current = true;
                isAutoScrolling.current = false;
            } else if (!isAutoScrolling.current) {
                isAtBottomRef.current = false;
            }
        }

        el.scrollTop = el.scrollHeight;
        el.addEventListener("scroll", handleScroll);

        return () => {
            el.removeEventListener("scroll", handleScroll);
        };
    }, [ref]);

    useEffect(() => {
        const el = ref.current;
        if (!el || !isAtBottomRef.current) return;

        isAutoScrolling.current = true;
        el.scrollTo({ top: el.scrollHeight, behavior: "smooth" });
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [...deps, ref]);
}
