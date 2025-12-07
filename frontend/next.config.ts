import type { NextConfig } from "next";

const nextConfig: NextConfig = {
    images: {
        remotePatterns: [
            new URL("https://localhost:7266/**"),
            new URL("https://api.anarchychess.org/**"),
        ],
    },
    rewrites() {
        return [
            {
                source: "/api/:path*",
                destination: `${process.env.NEXT_PUBLIC_API_URL}/api/:path*`,
            },
        ];
    },
    devIndicators: false,
};

export default nextConfig;
