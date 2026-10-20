import { auth } from './auth';

export interface ProblemDetails {
  type?: string;
  title?: string;
  status?: number;
  detail?: string;
}

export class ApiError extends Error {
  constructor(public readonly problem: ProblemDetails, public readonly status: number) {
    super(problem.detail ?? problem.title ?? `Request failed (${status})`);
  }
}

async function send(path: string, init: RequestInit = {}): Promise<Response> {
  const headers = new Headers(init.headers);
  if (auth.accessToken) headers.set('Authorization', `Bearer ${auth.accessToken}`);
  const response = await fetch(path, { ...init, headers });
  if (!response.ok) {
    let problem: ProblemDetails = { title: response.statusText, status: response.status };
    try { problem = await response.json(); } catch { /* not JSON */ }
    throw new ApiError(problem, response.status);
  }
  return response;
}

export const http = {
  async getJson<T>(path: string): Promise<T> {
    return (await send(path)).json() as Promise<T>;
  },
  async postJson<T>(path: string, body: unknown): Promise<T> {
    const r = await send(path, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(body),
    });
    const text = await r.text();
    try { return JSON.parse(text) as T; } catch { return text as unknown as T; }
  },
  /** Sends JSON with a verb other than POST (PUT, DELETE, PATCH); the body is optional. */
  async sendJson(method: 'PUT' | 'DELETE' | 'PATCH', path: string, body?: unknown): Promise<void> {
    await send(path, body === undefined ? { method } : { method, headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(body) });
  },
  async getText(path: string): Promise<string> {
    return (await send(path)).text();
  },
  async postFormJson<T>(path: string, form: FormData): Promise<T> {
    return (await send(path, { method: 'POST', body: form })).json() as Promise<T>;
  },
  async postForm(path: string, form: FormData): Promise<{ text: string; contentType: string }> {
    const r = await send(path, { method: 'POST', body: form });
    return { text: await r.text(), contentType: r.headers.get('Content-Type') ?? '' };
  },
  async postText(path: string, body: unknown): Promise<string> {
    const r = await send(path, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(body),
    });
    return r.text();
  },
  /** Reads a streamed response (the streamed AI endpoints) chunk by chunk. */
  async postStream(path: string, body: unknown, onChunk: (text: string) => void, signal?: AbortSignal): Promise<void> {
    const r = await send(path, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(body),
      signal,
    });
    const reader = r.body!.getReader();
    const decoder = new TextDecoder();
    for (;;) {
      const { done, value } = await reader.read();
      if (done) break;
      onChunk(decoder.decode(value, { stream: true }));
    }
  },
};
