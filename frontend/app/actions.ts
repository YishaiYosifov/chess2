"use server";

import { revalidateTag } from "next/cache";

export async function revalidateUser(
    username: string | undefined
): Promise<void> {
    if (!username) return;
    revalidateTag(`user-${username}`);
}
