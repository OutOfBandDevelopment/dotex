# OoBDev.Data.Vectors.DB.Tests

Integration tests that deploy the `OoBDev.Data.Vectors.DB` dacpac (with the SQL CLR vector types merged in) to the Docker SQL Server and run the vector functions inside the database.

- Category `Integration`; needs `SQLSERVER_CONNECTION_STRING` (see `containers/testing`).
- The tests enable CLR and turn off `clr strict security` for their run and restore both afterwards: use a disposable server only.
- Set `VECTORS_DB_DACPAC` to test a dacpac outside the default build output.
