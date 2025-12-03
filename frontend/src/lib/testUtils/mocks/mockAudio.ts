import { Mock } from "vitest";

interface AudioMock {
    play: Mock;
    pause: Mock;
    cloneNode: Mock;
    currentTime: number;
}

export function mockAudio(): {
    audioMock: AudioMock;
    audioConstructorMock: Mock;
} {
    const audioMock = {
        play: vi.fn(),
        pause: vi.fn(),
        cloneNode: vi.fn(() => audioMock),
        currentTime: 0,
    };

    const audioConstructorMock = vi.fn().mockImplementation(() => audioMock);
    vi.stubGlobal("Audio", audioConstructorMock);

    return { audioMock, audioConstructorMock };
}
