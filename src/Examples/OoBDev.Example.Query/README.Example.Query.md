# OoBDev.Example.Query

A host sample for the metadata query engine (`OoBDev.Query.Metadata`) on PostgreSQL. One row class (`NoteRow`) declares what clients may ask; the host maps four routes:

| Route | Does |
|-------|------|
| `GET /describe` | kinds, facets, predicates, operators, spaces |
| `GET /openapi.json` | the OpenAPI document built from the same attributes |
| `POST /q` | a `QueryRequest` body |
| `GET /q?text=` | the text form, for example `cats near geo:51.5,-0.1 max 25` |

Set `Query:Postgres:ConnectionString` and a long `Query:Metadata:CursorKey`. A `QueryException` answers 400 with problem details. Add authentication in front (JWT bearer, see `docs/patterns-discovery/03-practices-and-conventions/09-authentication-practices.md`) and register an `IQueryAuthorizer` to scope rows per caller.
