import { afterEach, describe, expect, it, vi } from 'vitest';
import { ApiError, http } from './http';

const respond = (status: number, body: unknown, contentType = 'application/problem+json') =>
  vi.stubGlobal('fetch', vi.fn(async () => new Response(typeof body === 'string' ? body : JSON.stringify(body), { status, headers: { 'Content-Type': contentType } })));

describe('http', () => {
  afterEach(() => vi.unstubAllGlobals());

  it('throws ApiError carrying the RFC 9457 problem details', async () => {
    respond(400, { type: 'https://tools.ietf.org/html/rfc9110#section-15.5.1', title: 'Bad Request', status: 400, detail: 'Name is required' });
    const error = (await http.getJson('/api/x').catch(e => e)) as ApiError;
    expect(error).toBeInstanceOf(ApiError);
    expect(error.status).toBe(400);
    expect(error.message).toBe('Name is required');
    expect(error.problem.title).toBe('Bad Request');
  });

  it('falls back to the status text when the body is not JSON', async () => {
    respond(502, 'upstream down', 'text/plain');
    const error = (await http.getText('/api/x').catch(e => e)) as ApiError;
    expect(error.status).toBe(502);
    expect(error.problem.status).toBe(502);
  });

  it('returns text bodies from postJson when the response is not JSON', async () => {
    respond(200, 'Mail accepted', 'text/plain');
    expect(await http.postJson('/api/Communications/public', {})).toBe('Mail accepted');
  });
});
