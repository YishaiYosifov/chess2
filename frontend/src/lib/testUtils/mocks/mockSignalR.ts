import * as signalR from "@microsoft/signalr";
import { mock } from "vitest-mock-extended";
import { Mock } from "vitest";

export function mockHubBuilder() {
    const mockHubBuilder = mock<signalR.HubConnectionBuilder>();
    mockHubBuilder.withUrl.mockReturnThis();
    mockHubBuilder.withAutomaticReconnect.mockReturnThis();
    mockHubBuilder.configureLogging.mockReturnThis();

    (signalR.HubConnectionBuilder as Mock).mockReturnValue(mockHubBuilder);

    return mockHubBuilder;
}
