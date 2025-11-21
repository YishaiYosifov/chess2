export enum AudioType {
    MOVE = "/assets/sfx/move.webm",
    CAPTURE = "/assets/sfx/capture.webm",
    ILLEGAL_MOVE = "/assets/sfx/illegal.webm",

    KNOOKLEAR_FUSION = "/assets/sfx/explosion.webm",

    LOW_TIME = "/assets/sfx/low_time.webm",

    MATCH_FOUND = "/assets/sfx/game_start.webm",
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

        audio.currentTime = 0;
        await audio.play();
    }
}
