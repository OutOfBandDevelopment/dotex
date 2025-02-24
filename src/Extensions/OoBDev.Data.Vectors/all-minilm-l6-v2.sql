--EXEC sp_configure 'external scripts enabled', 1;
--RECONFIGURE;

DECLARE @text NVARCHAR(MAX) = 'Hello, world!';
DECLARE @result NVARCHAR(MAX);

EXEC sp_execute_external_script
    @language = N'Python',
    @script = N'
from sentence_transformers import SentenceTransformer
import numpy as np
import pandas as pd

# Load model
model = SentenceTransformer("all-MiniLM-L6-v2")

# Get embedding
embeddings = model.encode(input_text)
output_df = pd.DataFrame([",".join(map(str, embeddings))], columns=["Embedding"])
',
    @input_data_1 = N'SELECT @text AS input_text',
    @input_data_1_name = N'input_text',
    @output_data_1_name = N'output_df',
    @params = N'@text NVARCHAR(MAX)',
    @text = @text
WITH RESULT SETS ((Embedding NVARCHAR(MAX)));

