import type { NextConfig } from "next";

const nextConfig: NextConfig = {
    images: { remotePatterns: [new URL("https://localhost:7266/**")] },
    devIndicators: false,
};

export default nextConfig;
