import Button from "@/components/ui/Button";
import Link from "next/link";

const notFound = () => {
    return (
        <div className="flex h-full w-full flex-col items-center justify-center gap-5">
            <h1 className="text-3xl">Not Found!</h1>
            <Button>
                <Link href="/">Go back to Home</Link>
            </Button>
        </div>
    );
};
export default notFound;
