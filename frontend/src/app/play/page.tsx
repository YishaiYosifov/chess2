import scssVariables from "@/lib/variables.module.scss";
import { Variant } from "@/lib/constants";
import styles from "./play.module.scss";

import PlayOptions from "@/components/play/PlayOptions";
import Chessboard from "@/components/Chessboard";

const PlayPage = () => {
    const breakpoint = parseInt(scssVariables.lg);
    return (
        <div className={styles.container}>
            <Chessboard
                variant={Variant.Anarchy}
                offsetBreakpoints={[
                    {
                        breakpoint,
                        offset: { width: 10, height: 200 },
                    },
                    {
                        breakpoint,
                        offset: { width: 500, height: 100 },
                    },
                ]}
                fixed
            />

            <PlayOptions />
        </div>
    );
};
export default PlayPage;
