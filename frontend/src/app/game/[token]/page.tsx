import { liveGameApi } from "@/lib/apis";

const page = async ({ params: { token } }: { params: { token: string } }) => {
    console.log(await liveGameApi.getLiveGame({ token }));
    return;
};
export default page;
