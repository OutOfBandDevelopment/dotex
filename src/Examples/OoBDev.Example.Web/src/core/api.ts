import type { paths } from './api-schema';
import { http } from './http';

type JsonBody<R> = R extends { content: { 'application/json': infer B } } ? B : unknown;
type Ok<Op> = Op extends { responses: { 200: infer R } } ? JsonBody<R> : unknown;

export type GetPath = { [P in keyof paths]: paths[P] extends { get: unknown } ? P : never }[keyof paths];
export type PostPath = { [P in keyof paths]: paths[P] extends { post: unknown } ? P : never }[keyof paths];

type GetOk<P extends GetPath> = paths[P] extends { get: infer Op } ? Ok<Op> : never;
type PostOk<P extends PostPath> = paths[P] extends { post: infer Op } ? Ok<Op> : never;
type PostBody<P extends PostPath> = paths[P] extends { post: { requestBody?: { content: { 'application/json': infer B } } } } ? B : unknown;

/**
 * Paths and response shapes come from the generated OpenAPI schema (`npm run api:generate`), so a renamed
 * endpoint or changed model fails the build instead of failing at run time. Errors are ApiError (RFC 9457).
 */
export const api = {
  get<P extends GetPath>(path: P): Promise<GetOk<P>> {
    return http.getJson<GetOk<P>>(path);
  },
  post<P extends PostPath>(path: P, body: PostBody<P>): Promise<PostOk<P>> {
    return http.postJson<PostOk<P>>(path, body);
  },
};
