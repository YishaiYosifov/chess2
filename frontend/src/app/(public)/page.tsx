import HomeIntroSection from "@/features/home/components/HomeIntroSection";
import HomeFooter from "@/features/home/components/HomeFooter";
import HomeHero from "@/features/home/components/HomeHero";

export const metadata = { title: "Home - Chess 2" };

const HomePage = async () => {
    return (
        <div className="flex flex-1 flex-col gap-10">
            <HomeHero />
            <HomeIntroSection />
            <HomeFooter />
        </div>
    );
};
export default HomePage;
