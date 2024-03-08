import scssVariables from "@/styles/variables.module.scss";
import { Variant } from "@/lib/constants";
import styles from "./play.module.scss";

import PlayOptions from "@/components/play/PlayOptions";
import Chessboard from "@/components/Chessboard";

const PlayPage = () => {
    const breakpoint = parseInt(scssVariables.xl);
    return (
        <div className={styles.container}>
            <Chessboard
                variant={Variant.Anarchy}
                offsetBreakpoints={[
                    {
                        widthBreakpoint: breakpoint,
                        offset: { width: 10, height: 200 },
                    },
                    {
                        widthBreakpoint: breakpoint,
                        offset: { width: 626, height: 100 },
                    },
                ]}
                fixed
            />

            <PlayOptions />
        </div>
    );
};
export default PlayPage;
