import { notFound } from "next/navigation";

import { fetchGames, fetchProfile, fetchRatings } from "@/lib/utils/common";
import styles from "./user.module.scss";

import RatingCard from "@/components/pages/profile/RatingsCard";
import GamesTable from "@/components/pages/profile/GamesTable";
import Profile from "@/components/pages/profile/Profile";

export const metadata = {
    title: "Chess 2 - User",
};

const UserPage = async ({
    params: { username },
}: {
    params: { username: string };
}) => {
    // Find the date a month ago to fetch the ratings since
    const dateMonthAgo = new Date();
    dateMonthAgo.setMonth(dateMonthAgo.getMonth() - 1);

    const [profile, ratings, games] = await Promise.all([
        fetchProfile(username),
        fetchRatings(username, dateMonthAgo.toISOString().split("T")[0]),
        fetchGames(username),
    ]);
    if (!profile || !ratings || !games) notFound();

    return (
        <div className={styles.container}>
            <section className={styles["profile-info-section"]}>
                <Profile profile={profile} />
            </section>

            <section className={styles["games-section"]}>
                <GamesTable games={games} viewingProfile={profile} />

                {!games.length && (
                    <p className={`${styles["no-games-text"]} text-danger`}>
                        This user hasn't played any games yet!
                    </p>
                )}
            </section>

            <section className={styles["ratings-section"]}>
                <div className={styles["ratings-container"]}>
                    {Object.entries(ratings).map(([variant, data]) => (
                        <RatingCard
                            key={variant}
                            variant={variant}
                            ratingData={data}
                        />
                    ))}
                </div>
            </section>
        </div>
    );
};
export default UserPage;
