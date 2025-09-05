import { useRouter } from "next/navigation";
import { useEffect } from "react";

export default function useInvalidateOnNavigate() {
    const router = useRouter();
    useEffect(() => {
        return () => router.refresh();
    }, [router]);
}
