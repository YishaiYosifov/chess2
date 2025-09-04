"use client";

import Button from "@/components/ui/Button";

const Error = ({ reset }: { reset: () => void }) => {
    return (
        <div className="flex h-full w-full flex-col items-center justify-center gap-5">
            <h1 className="text-3xl">Something went wrong!</h1>
            <Button onClick={reset}>Try again</Button>
        </div>
    );
};
export default Error;
