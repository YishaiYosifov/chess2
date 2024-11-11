import type { Config } from "tailwindcss";

export default {
    content: [
        "./src/components/**/*.{js,ts,jsx,tsx,mdx}",
        "./src/app/**/*.{js,ts,jsx,tsx,mdx}",
    ],
    theme: {
        extend: {
            colors: {
                text: "#e0e4f0",
                background: "#090c12",
                primary: "#9cadce",
                secondary: "#764e3a",
                accent: "#a1b364",
            },
        },
    },
    plugins: [],
} satisfies Config;
