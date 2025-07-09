import { createFakePrivateUser } from "@/lib/testUtils/fakers/userFaker";

export const useAuthedContext = () => ({
    user: createFakePrivateUser(),
});
