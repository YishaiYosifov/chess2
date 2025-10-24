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
        <Card className="hidden max-w-30 flex-1 gap-0 p-0 lg:flex">
            <div className="sticky top-0 flex h-screen flex-col">
                <Link
                    href={piecesGuideHref}
                    className="hover:bg-primary flex flex-1 cursor-pointer items-center justify-center text-2xl
                        font-semibold transition"
                >
                    Pieces
                </Link>
                <hr className="text-secondary/50" />
                <Link
                    href={rulesGuideHref}
                    className="hover:bg-primary flex flex-1 cursor-pointer items-center justify-center text-2xl
                        font-semibold transition"
                >
                    Rules
                </Link>
            </div>
        </Card>
    );
};
export default GuideSide;
