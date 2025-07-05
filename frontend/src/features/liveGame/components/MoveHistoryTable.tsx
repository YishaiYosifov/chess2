import Card from "@/components/ui/Card";

const MoveHistoryTable = () => {
    return (
        <Card className="block overflow-x-auto p-0">
            <table className="min-w-full overflow-hidden text-center text-sm text-white">
                <tbody>
                    <MoveRow index={0} moveWhite="e4" moveBlack="e5" />
                    <MoveRow index={1} moveWhite="e4" moveBlack="e5" />
                    <MoveRow index={2} moveWhite="e4" moveBlack="e5" />
                    <MoveRow index={3} moveWhite="e4" moveBlack="e5" />
                    <MoveRow index={4} moveWhite="e4" moveBlack="e5" />
                </tbody>
            </table>
        </Card>
    );
};
export default MoveHistoryTable;

const MoveRow = ({
    index,
    moveWhite,
    moveBlack,
}: {
    index: number;
    moveWhite?: string;
    moveBlack?: string;
}) => {
    const color = index % 2 === 0 ? "bg-white/5" : "";
    return (
        <tr className={color}>
            <td className="py-3">{moveWhite}</td>
            <td className="py-3">{moveBlack}</td>
        </tr>
    );
};
