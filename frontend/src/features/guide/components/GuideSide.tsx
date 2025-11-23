import Link from "next/link";
import Card from "@/components/ui/Card";

const GuideSide = ({
    piecesGuideHref,
    rulesGuideHref,
}: {
    piecesGuideHref: string;
    rulesGuideHref: string;
}) => {
    return (
        <Card className="sticky top-5 hidden max-h-[calc(100vh-2.5rem)] max-w-30 flex-1 gap-0 p-0 lg:flex">
            <Link
                href={piecesGuideHref}
                className="hover:bg-primary flex flex-1 cursor-pointer items-center justify-center
                    rounded-t-md text-2xl font-semibold transition"
            >
                Pieces
            </Link>
            <hr className="text-secondary/50" />
            <Link
                href={rulesGuideHref}
                className="hover:bg-primary flex flex-1 cursor-pointer items-center justify-center
                    rounded-b-md text-2xl font-semibold transition"
            >
                Rules
            </Link>
        </Card>
    );
};
export default GuideSide;
