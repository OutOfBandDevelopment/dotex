
CREATE SCHEMA [vector]
GO
CREATE TYPE [vector].[VectorD]
EXTERNAL NAME [OoBDev.Data.Vectors].[OoBDev.Data.Vectors.SqlVector]
GO
CREATE TYPE [vector].[VectorF]
EXTERNAL NAME [OoBDev.Data.Vectors].[OoBDev.Data.Vectors.SqlVectorF]
GO 
CREATE AGGREGATE [vector].[Centroid](@vector [vector].[VectorD])
    RETURNS [vector].[VectorD]
    EXTERNAL NAME [OoBDev.Data.Vectors].[OoBDev.Data.Vectors.CentroidAggregator];
GO
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
CREATE FUNCTION [vector].[Element](@vector [vector].[VectorD], @index INT)
    RETURNS FLOAT
    EXTERNAL NAME [OoBDev.Data.Vectors].[OoBDev.Data.Vectors.VectorFunctions].[Element];
GO
CREATE FUNCTION [vector].[ElementF](@vector [vector].[VectorF], @index INT)
    RETURNS FLOAT(24)
    EXTERNAL NAME [OoBDev.Data.Vectors].[OoBDev.Data.Vectors.VectorFunctions].[ElementF];
GO
CREATE FUNCTION [vector].[Magnitude](@vector [vector].[VectorD])
    RETURNS FLOAT
    EXTERNAL NAME [OoBDev.Data.Vectors].[OoBDev.Data.Vectors.VectorFunctions].[Magnitude];
GO
CREATE FUNCTION [vector].[MagnitudeF](@vector [vector].[VectorF])
    RETURNS FLOAT(24)
    EXTERNAL NAME [OoBDev.Data.Vectors].[OoBDev.Data.Vectors.VectorFunctions].[MagnitudeF];
GO
CREATE FUNCTION [vector].[Length](@vector [vector].[VectorD])
    RETURNS INT
    EXTERNAL NAME [OoBDev.Data.Vectors].[OoBDev.Data.Vectors.VectorFunctions].[Length];
GO
CREATE FUNCTION [vector].[LengthF](@vector [vector].[VectorD])
    RETURNS INT
    EXTERNAL NAME [OoBDev.Data.Vectors].[OoBDev.Data.Vectors.VectorFunctions].[Length];
GO
CREATE FUNCTION [vector].[Distance](@metric NVARCHAR(MAX), @vector1 [vector].[VectorD], @vector2 [vector].[VectorD])
    RETURNS FLOAT
    EXTERNAL NAME [OoBDev.Data.Vectors].[OoBDev.Data.Vectors.VectorFunctions].[Distance];
GO
CREATE FUNCTION [vector].[DistanceF](@metric NVARCHAR(MAX), @vector1 [vector].[VectorF], @vector2 [vector].[VectorF])
    RETURNS FLOAT(24)
    EXTERNAL NAME [OoBDev.Data.Vectors].[OoBDev.Data.Vectors.VectorFunctions].[DistanceF];
GO
CREATE FUNCTION [vector].[Midpoint](@vector1 [vector].[VectorD], @vector2 [vector].[VectorD])
    RETURNS [vector].[VectorD]
    EXTERNAL NAME [OoBDev.Data.Vectors].[OoBDev.Data.Vectors.VectorFunctions].[Midpoint];
GO
CREATE FUNCTION [vector].[MidpointF](@vector1 [vector].[VectorF], @vector2 [vector].[VectorF])
    RETURNS [vector].[VectorF]
    EXTERNAL NAME [OoBDev.Data.Vectors].[OoBDev.Data.Vectors.VectorFunctions].[MidpointF];
GO
CREATE FUNCTION [vector].[Angle](@vector1 [vector].[VectorD], @vector2 [vector].[VectorD])
    RETURNS FLOAT
    EXTERNAL NAME [OoBDev.Data.Vectors].[OoBDev.Data.Vectors.VectorFunctions].[Angle];
GO
CREATE FUNCTION [vector].[AngleF](@vector1 [vector].[VectorF], @vector2 [vector].[VectorF])
    RETURNS FLOAT(24)
    EXTERNAL NAME [OoBDev.Data.Vectors].[OoBDev.Data.Vectors.VectorFunctions].[AngleF];
GO