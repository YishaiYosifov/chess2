import { render } from "@testing-library/react";
import { useStore } from "@/zustand/store";

import StoreInitializer from "../StoreInitializer";

vi.mock("@/zustand/store", () => ({
    useStore: {
        setState: vi.fn(),
    },
}));

describe("StoreInitializer", () => {
    const values = { isAuthed: true };
    const action = "TEST_INITIALIZE";

    it("should initialize the store with provided values and action", () => {
        render(<StoreInitializer values={values} action={action} />);
        expect(useStore.setState).toHaveBeenCalledWith(values, false, action);
    });

    it("should not initialize the store if already initialized", () => {
        const { rerender } = render(
            <StoreInitializer values={values} action={action} />
        );
        rerender(<StoreInitializer values={values} action={action} />);

        expect(useStore.setState).toHaveBeenCalledTimes(1);
    });
});
