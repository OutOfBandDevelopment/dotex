
CREATE SCHEMA [vector]
GO

CREATE TYPE [vector].[VectorD]
EXTERNAL NAME [OoBDev.Data.Vectors].[OoBDev.Data.Vectors.SqlVector]

CREATE TYPE [vector].[VectorF]
EXTERNAL NAME [OoBDev.Data.Vectors].[OoBDev.Data.Vectors.SqlVectorF]
GO 

CREATE AGGREGATE [vector].[Centroid](@vector [vector].[VectorD])
    RETURNS [vector].[VectorD]
    EXTERNAL NAME [OoBDev.Data.Vectors].[OoBDev.Data.Vectors.CentroidAggregator];

CREATE AGGREGATE [vector].[CentroidF](@vector [vector].[VectorF])
    RETURNS [vector].[VectorF]
    EXTERNAL NAME [OoBDev.Data.Vectors].[OoBDev.Data.Vectors.CentroidFAggregator];
GO		
CREATE FUNCTION [vector].[Random](@length INT, @seed INT)
    RETURNS [vector].[VectorD]
    EXTERNAL NAME [OoBDev.Data.Vectors].[OoBDev.Data.Vectors.VectorFunctions].[Random];
GO
CREATE FUNCTION [vector].[RandomF](@length INT, @seed INT)
    RETURNS [vector].[VectorF]
    EXTERNAL NAME [OoBDev.Data.Vectors].[OoBDev.Data.Vectors.VectorFunctions].[RandomF];
GO
CREATE FUNCTION [vector].[Uniform](@length INT, @min float, @max float, @seed INT)
    RETURNS [vector].[VectorD]
    EXTERNAL NAME [OoBDev.Data.Vectors].[OoBDev.Data.Vectors.VectorFunctions].[Uniform];
GO
CREATE FUNCTION [vector].[UniformF](@length INT, @min float, @max float, @seed INT)
    RETURNS [vector].[VectorF]
    EXTERNAL NAME [OoBDev.Data.Vectors].[OoBDev.Data.Vectors.VectorFunctions].[UniformF];
GO
