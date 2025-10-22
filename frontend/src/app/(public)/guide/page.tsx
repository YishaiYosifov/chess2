import NewPiecesGuide from "@/features/guide/components/NewPiecesGuide";
import NewRulesGuide from "@/features/guide/components/NewRulesGuide";

export default function GuidePage() {
    return (
        <div className="mx-auto flex max-w-6xl flex-1 flex-col gap-10 p-10">
            <NewPiecesGuide />
            <NewRulesGuide />
        </div>
    );
}
