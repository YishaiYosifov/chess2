export enum AudioType {
    MOVE = "/assets/sfx/move.webm",
    CAPTURE = "/assets/sfx/capture.webm",
    ILLEGAL_MOVE = "/assets/sfx/illegal.webm",
    PROMOTION = "/assets/sfx/promotion.webm",

    KNOOKLEAR_FUSION = "/assets/sfx/explosion.webm",
    CASTLE = "/assets/sfx/castle.webm",

    LOW_TIME = "/assets/sfx/low_time.webm",

    GAME_START = "/assets/sfx/game_start.webm",
    GAME_END = "/assets/sfx/game_end.webm",
}

export default class AudioPlayer {
    private static _cachedAudios: Map<AudioType, HTMLAudioElement> = new Map();

    static async playAudio(audioType: AudioType): Promise<void> {
        let audio = this._cachedAudios.get(audioType);
        if (!audio) {
            audio = new Audio(audioType);
            this._cachedAudios.set(audioType, audio);
        }

        const clone = audio.cloneNode() as HTMLAudioElement;
        clone.currentTime = 0;
        await clone.play();
    }
}
