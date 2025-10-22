import NewPiecesGuide from "@/features/guide/components/NewPiecesGuide";
import NewRulesGuide from "@/features/guide/components/NewRulesGuide";

export default function GuidePage() {
    return (
        <div className="mx-auto flex max-w-6xl flex-1 flex-col gap-2 p-5">
            <h2 className="text-4xl">New Pieces</h2>
            <NewPiecesGuide />

            <h2 className="pt-5 text-4xl">New Rules</h2>
            <NewRulesGuide />
        </div>
    );
}
