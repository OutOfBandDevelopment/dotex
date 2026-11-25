-- A parent/child tree (organisation) with one cycle (A -> B -> C -> A) for PostgreSQL and SQLite.
CREATE TABLE node (id INTEGER PRIMARY KEY, parent_id INTEGER NULL, name TEXT NOT NULL);
INSERT INTO node (id, parent_id, name) VALUES (1, NULL, 'CEO'), (2, 1, 'VP'), (3, 2, 'Dev'), (4, 2, 'Ops'), (5, 3, 'Intern'), (6, 7, 'A'), (7, 8, 'B'), (8, 6, 'C');
