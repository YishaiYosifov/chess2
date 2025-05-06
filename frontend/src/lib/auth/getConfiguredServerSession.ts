import { getServerSession } from "next-auth";
import authOptions from "./authOptions";
import { GET } from "@/app/api/auth/[...nextauth]/route";

const getConfiguredServerSession = () => getServerSession(GET);
export default getConfiguredServerSession;
