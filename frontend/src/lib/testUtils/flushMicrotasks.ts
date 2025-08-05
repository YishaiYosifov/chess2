import { act } from "react";

const flushMicrotasks = (): Promise<void> => act(() => Promise.resolve());
export default flushMicrotasks;
