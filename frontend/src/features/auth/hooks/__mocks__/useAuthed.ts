import { createPrivateUser } from "@/lib/testUtils/fakers/userFaker";

export const useAuthedContext = () => ({
    user: createPrivateUser(),
});
