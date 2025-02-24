CREATE TYPE [VECTOR]
EXTERNAL NAME [OoBDev.Data.Vectors].[OoBDev.Data.Vectors.SqlVector]

CREATE AGGREGATE [Centroid](@vector [Vector] )
    RETURNS [dbo].[Vector]
    EXTERNAL NAME [OoBDev.Data.Vectors].[OoBDev.Data.Vectors.CentroidAggregator];