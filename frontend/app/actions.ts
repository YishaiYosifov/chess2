"use server";

import { revalidateTag } from "next/cache";

export async function revalidateUser(username?: string): Promise<void> {
    if (!username) return;
    revalidateTag(`user-${username}`);
}
