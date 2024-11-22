import Flag from "@/components/profile/Flag";

type Params = Promise<{ username: string }>;

export async function generateMetadata({ params }: { params: Params }) {
    const { username } = await params;
    return {
        title: `${username} - Chess 2 Profile - Chess 2`,
    };
}

const UserPage = async ({ params }: { params: Params }) => {
    const { username } = await params;

    return (
        <div>
            <Flag size={20} />
        </div>
    );
};
export default UserPage;
