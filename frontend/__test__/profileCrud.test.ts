import { server } from "@/mocks/server";

import {
    fetchProfile,
    fetchRatings,
    fetchGames,
    fetchTodos,
} from "@/lib/cruds/profileCrud";

describe("fetchProfile crud function", () => {
    it("should return the correct number of todo items", async () => {
        const response = await fetch("http://127.0.0.1:3000/test");
        console.log(await response.text());
        //const todosArray = await fetchTodos();
        //expect(todosArray.length).toBe(4);
    });
});
