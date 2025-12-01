import HomeIntroSection from "@/features/home/components/HomeIntroSection";
import HomeFooter from "@/features/home/components/HomeFooter";
import HomeHero from "@/features/home/components/HomeHero";

export const metadata = { title: "Home - Anarchy Chess" };

const HomePage = async () => {
    return (
        <main className="grid flex-1 grid-cols-1 grid-rows-[min-content_min-content_1fr] flex-col">
            <HomeHero />
            <HomeIntroSection />
            <HomeFooter />
        </main>
    );
};
export default HomePage;
