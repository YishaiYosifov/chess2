"use server";

import { revalidateTag } from "next/cache";

export async function revalidateUser(userId) {
    revalidateTag(`user-${userId}`);
}
