namespace OoBDev.Data.Vectors.Postgres;

/// <summary>
/// The statements of the store. Only validated identifiers are written into the text; every value is a parameter.
/// </summary>
internal sealed class PgVectorSql(PgVectorOptions options)
{
    private string Type => options.UseHalfVector ? "halfvec" : "vector";

    public string Table => $"\"{options.Schema}\".\"{options.Table}\"";

    public string Operator => options.Metric == VectorMetric.Cosine ? "<=>" : "<->";

    public string OperatorClass => (options.UseHalfVector, options.Metric) switch
    {
        (false, VectorMetric.Cosine) => "vector_cosine_ops",
        (false, _) => "vector_l2_ops",
        (true, VectorMetric.Cosine) => "halfvec_cosine_ops",
        _ => "halfvec_l2_ops",
    };

    public string IndexName => $"\"{options.Table}_embedding_hnsw\"";

    public string Extension => "CREATE EXTENSION IF NOT EXISTS vector";

    public string CreateTable =>
        $"CREATE SCHEMA IF NOT EXISTS \"{options.Schema}\"; " +
        $"CREATE TABLE IF NOT EXISTS {Table} (id uuid PRIMARY KEY, item_id text NOT NULL, vector_index int NOT NULL, embedding {Type}({options.Dimensions}) NOT NULL, metadata jsonb NOT NULL DEFAULT '{{}}'::jsonb); " +
        $"CREATE INDEX IF NOT EXISTS \"{options.Table}_item_idx\" ON {Table} (item_id)";

    public string CreateHnswIndex => $"CREATE INDEX IF NOT EXISTS {IndexName} ON {Table} USING hnsw (embedding {OperatorClass})";

    public string Insert => $"INSERT INTO {Table} (id, item_id, vector_index, embedding, metadata) VALUES (@id, @item, @idx, @embedding, @metadata)";

    public string List => $"SELECT item_id, metadata FROM {Table} ORDER BY item_id, vector_index";

    public string Neighbours =>
        $"SELECT item_id, vector_index, metadata, embedding {Operator} @query AS distance FROM {Table} ORDER BY distance, id LIMIT @limit";

    public string NeighboursBestPerItem =>
        $"SELECT item_id, vector_index, metadata, distance FROM (SELECT item_id, vector_index, metadata, embedding {Operator} @query AS distance, " +
        $"row_number() OVER (PARTITION BY item_id ORDER BY embedding {Operator} @query, vector_index) AS rn FROM {Table}) t WHERE rn = 1 ORDER BY distance, item_id LIMIT @limit";

    public string NeighboursBestPerGroup =>
        $"SELECT item_id, vector_index, metadata, distance FROM (SELECT item_id, vector_index, metadata, embedding {Operator} @query AS distance, " +
        $"row_number() OVER (PARTITION BY metadata ->> @group ORDER BY embedding {Operator} @query, item_id) AS rn FROM {Table}) t WHERE rn = 1 ORDER BY distance, item_id LIMIT @limit";

    public double Score(double distance) => options.Metric == VectorMetric.Cosine ? 1 - distance : 1 / (1 + distance);
}
