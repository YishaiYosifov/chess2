import HomeIntroSection from "@/features/home/components/HomeIntroSection";
import HomeFooter from "@/features/home/components/HomeFooter";
import HomeHero from "@/features/home/components/HomeHero";

export const metadata = { title: "Home - Anarchy Chess" };

const HomePage = async () => {
    return (
        <div className="flex flex-1 flex-col">
            <HomeHero />
            <HomeIntroSection />
            <HomeFooter />
        </div>
    );
};
export default HomePage;
