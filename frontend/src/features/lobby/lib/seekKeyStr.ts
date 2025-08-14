import { SeekKeyStr, SeekKey } from "./types";

export default function SeekKeyToStr(seekKey: SeekKey): SeekKeyStr {
    return (`${seekKey.pool.poolType}-` +
        `${seekKey.pool.timeControl.baseSeconds}+${seekKey.pool.timeControl.incrementSeconds}:` +
        `${seekKey.userId}`) as SeekKeyStr;
}
