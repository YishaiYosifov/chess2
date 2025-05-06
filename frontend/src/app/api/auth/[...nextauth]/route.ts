import { NextRequest } from "next/server";
import NextAuth from "next-auth";

import authOptions from "@/lib/auth/authOptions";

interface RouteHandlerContext {
    params: Promise<{ nextauth: string[] }>;
}

async function handler(request: NextRequest, context: RouteHandlerContext) {
    return NextAuth(request, context, authOptions);
}

export { handler as GET, handler as POST };
