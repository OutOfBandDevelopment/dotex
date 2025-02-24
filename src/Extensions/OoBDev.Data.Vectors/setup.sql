
CREATE TYPE [Vector]
EXTERNAL NAME [OoBDev.Data.Vectors].[OoBDev.Data.Vectors.SqlVector]

CREATE TYPE [VectorF]
EXTERNAL NAME [OoBDev.Data.Vectors].[OoBDev.Data.Vectors.SqlVectorF]

CREATE AGGREGATE [Centroid](@vector [Vector])
    RETURNS [dbo].[Vector]
    EXTERNAL NAME [OoBDev.Data.Vectors].[OoBDev.Data.Vectors.CentroidAggregator];

CREATE AGGREGATE [CentroidF](@vector [VectorF])
    RETURNS [dbo].[VectorF]
    EXTERNAL NAME [OoBDev.Data.Vectors].[OoBDev.Data.Vectors.CentroidFAggregator];