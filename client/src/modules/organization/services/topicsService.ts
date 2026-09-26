import { httpClient } from "../../../core/http/httpClient";
import type { CreateTopicRequest, RenameTopicRequest, TopicResponse } from "../types";

export async function createTopic(request: CreateTopicRequest): Promise<TopicResponse> {
  const { data } = await httpClient.post<TopicResponse>("/api/topics", request);
  return data;
}

export async function listTopics(): Promise<TopicResponse[]> {
  const { data } = await httpClient.get<TopicResponse[]>("/api/topics");
  return data;
}

export async function renameTopic(id: string, request: RenameTopicRequest): Promise<TopicResponse> {
  const { data } = await httpClient.put<TopicResponse>(`/api/topics/${id}`, request);
  return data;
}

export async function deleteTopic(id: string): Promise<void> {
  await httpClient.delete(`/api/topics/${id}`);
}
