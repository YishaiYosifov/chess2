import type { Config } from "tailwindcss";

export default {
    content: [
        "./src/components/**/*.{js,ts,jsx,tsx,mdx}",
        "./src/app/**/*.{js,ts,jsx,tsx,mdx}",
    ],
    theme: {
        extend: {
            backgroundImage: {
                checkerboard:
                    "repeating-conic-gradient(#111111 0% 25%, transparent 0% 50%);",
            },
            colors: {
                text: "#EFEDF4",
                background: "#0F0C14",
                primary: "#B8ABCE",
                cta: "#A46496",
                link: "#60A5FA",
                error: "#F87171",
            },
        },
    },
    plugins: [],
} satisfies Config;
