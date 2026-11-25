-- The same social graph for PostgreSQL and SQLite, as two plain tables.
CREATE TABLE person (id INTEGER PRIMARY KEY, name TEXT NOT NULL);
CREATE TABLE knows (from_id INTEGER NOT NULL, to_id INTEGER NOT NULL);
INSERT INTO person (id, name) VALUES (1, 'Ann'), (2, 'Bob'), (3, 'Cat'), (4, 'Dan'), (5, 'Eve'), (6, 'Fay');
INSERT INTO knows (from_id, to_id) VALUES (1, 2), (2, 3), (3, 4), (1, 5), (5, 4), (2, 1), (3, 2), (4, 3), (5, 1), (4, 5);
