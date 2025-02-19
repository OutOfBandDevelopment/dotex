# OoBDev - Hybrid Search Engine and Document Store

## Summary

Hybrid search is a means to combine multiple search providers into a single correlated result.  In this example this is combining Keyword/Lexical search with Associative/Semantic search.

This is will be an example file store that will integrate multiple search options.  It should be build 
around off the self components that can be swapped out based on needs

## Planned features

## Notes

* [x] Hybrid Search
  * [x] Semantic Search
    * [x] using `all-mpnet-base-v2` sentence transformer
    * [x] using `Qdrant`
    * [x] aggregate 
  * [x] Lexical Search 
    * [x] powered by `Open Search`
      * painful overkill for just full-text search... though it looks to have vector search features as well.
    * [x] aggregate 
  * [x] Reranking
  * [ ] Data Driven
* [ ] Document Management
  * [x] Document Generation
    * [x] enables document summarization
    * [x] powered by `Ollama`
    * [x] using ~~phi-2, llama-2:7b, llama-2:13b~~ `mistral:instruct`
    * [x] code name `muse`
  * [x] Persistance and Retrieval
    * [x] powered `Azurite`
  * [ ] File Conversion
    * [x] Markdown to HTML powered `Markdig`
    * [x] HTML to PDF powered `WkHtmlToPdf`
  * [ ] File Packing/Unpacking
    * [ ] Zip powered by --TBD--
* [ ] Authentication and Authorization
  * [ ] Claims based Authorization
    * [ ] powered `Keycloak`

## Details

### Hybrid Search

Hybrid search is a a feature to do a ranked combination search.  This mean mixing the results from 
multiple search providers to help provide more relevant results for the user.  

#### Semantic Search

Semantic search is a means to search through information based on conceptual context.  This uses a 
nearest neighbor vector search

#### Lexical Search

Lexical search provides search results based on a thesaurus and inverted indexes.  

#### Re-ranking

Re-ranking will adjust search scoring results based on previous user actions.  The idea would be to 
have a simple ANN that can monitor user interations to help improve ranking per user

### Document Management

#### Documentation Summarization

Based on the uploaded content type the system will perform a summary generation.  Some examples may include...

* Image to Text: using an AI model to evaluate an image and provide a meaningful description
* Text to Text: review the content of an existing body of text and generate a meaningful summary

## High Level Design

### Architecture

```plantuml

node "Authorization Services" as [AuthorizationServices] {
    [UpsertUser]
    [AuthorizeUser]
}

node "Document Services" as [DocumentServices] {
    [Upload]
    [Download]
    [Convert]
    [Package]
    [Unpackage]
}

node "Document Storage" as [DocumentStorage] {
    [Save]
    [Retrieve]
}

node "Hybrid Search" as [SearchManager] {
    [SearchProvider]
    [ResultsRank]
    [ResultsMerge]
}

node "Lexical Search" as [LexicalSearch] {
    [LexicalDocumentIndexer]
    [InvertedIndexStore]
    [LexicalSearchProvider]
}

node "Semantic Search" as [SemanticSearch] {
    [SemanticDocumentIndexer]
    [DocumentChunker]
    [EmbeddingProvider]
    [EmbeddingStorage]
    [SemanticSearchProvider]
}

node "Document Generation" as [DocumentGeneration] {
    [GenerationContextBuilder]
    [GenerateContent]
    [GenerationPromptProvider]
    [GenerationVersionPersistance]
}

node "Document Indexer" as [DocumentIndexingService] {
  [FileSelector]
  [ContentPersistence]
  [ContentVectorization]
  [ContentLexicalAnalyzer]
  [ContentSummarizer]
}

[DocumentServices] -down-> [DocumentStorage]

[DocumentStorage] -down-> [LexicalSearch]
[DocumentStorage] -down-> [SemanticSearch]

[SearchManager] -down-> [LexicalSearch]
[SearchManager] -down-> [SemanticSearch]


[AuthorizationServices] -[hidden]> [DocumentServices]
[AuthorizationServices] -[hidden]> [SearchManager]

[Upload] --[hidden]> [Download]
[Download] --[hidden]> [Convert]
[Convert] --[hidden]> [Package]
[Package] --[hidden]> [Unpackage]
[LexicalSearch] -[hidden]> [SemanticSearch]


[Save] --[hidden]> [Retrieve]

[SearchProvider] --[hidden]> [ResultsRank]
[ResultsRank] --[hidden]> [ResultsMerge]

[LexicalDocumentIndexer] --[hidden]> [InvertedIndexStore]
[InvertedIndexStore] --[hidden]> [LexicalSearchProvider]

[SemanticDocumentIndexer] --[hidden]> [DocumentChunker]
[DocumentChunker] --[hidden]> [EmbeddingProvider]
[EmbeddingProvider] --[hidden]> [EmbeddingStorage]
[EmbeddingStorage] --[hidden]> [SemanticSearchProvider]


[UpsertUser] --[hidden]> [AuthorizeUser]

[GenerationContextBuilder] --[hidden]> [GenerateContent]
[GenerateContent] --[hidden]> [GenerationPromptProvider]
[GenerationPromptProvider] --[hidden]> [GenerationVersionPersistance]

[FileSelector] --[hidden]> [ContentPersistence]
[ContentPersistence] --[hidden]> [ContentVectorization]
[ContentVectorization] --[hidden]> [ContentLexicalAnalyzer]
[ContentLexicalAnalyzer] --[hidden]> [ContentSummarizer]
```

### Document Indexer

```plantuml

control FileScanner
database ContentStore
control DocumentSplitter
boundary ContentEmbedding
database VectorStore
database LexicalStore
control DocumentSummarizer

activate FileScanner 
FileScanner -> FileScanner : List of files
  loop each file

    par content-thread
      FileScanner -> ContentStore : Upload Content
      activate ContentStore
      return content-id
    else vector-index-thread
      FileScanner -> DocumentSplitter : Split file into chunks
      activate DocumentSplitter
        DocumentSplitter -> ContentEmbedding : Calculate Embedding
        activate ContentEmbedding
        return embedding
        DocumentSplitter -> VectorStore : Store Embedding
        activate VectorStore
        return vector-id
      return vector-ids
    else lexical-index-thread
      FileScanner -> LexicalStore : Split file into chunks
      activate LexicalStore
      return content-id
    else document-summary-thread
      FileScanner -> DocumentSummarizer : Split file into chunks
      activate DocumentSummarizer
      return content-id
    end
  end

```

### Hybrid Search

```plantuml
actor User
boundary SearchController
control HybridProvider
boundary ContentEmbedding
database VectorStore
database LexicalStore

User -> SearchController : query
activate SearchController

  SearchController -> HybridProvider : query
  activate HybridProvider
    
      HybridProvider -> ContentEmbedding : get embedding
      activate ContentEmbedding
      return embedding

    par lexical-search-thread
      HybridProvider -> LexicalStore : query
      activate LexicalStore
      return results
    else semantic-search-thread
      HybridProvider -> VectorStore : query
      activate VectorStore
      return results
    end

    HybridProvider -> HybridProvider : re-rank, merge and limit
    activate HybridProvider
    return results

  return results
return results

```

### Document Summary

```plantuml
control DocumentSummarizer
control LargeLanguageModel

DocumentSummarizer -> DocumentSummarizer : Split Content
activate DocumentSummarizer 
  loop each section
    DocumentSummarizer -> LargeLanguageModel : Partial Summary
    activate LargeLanguageModel 
    return partial-summary  
  end

  DocumentSummarizer -> LargeLanguageModel : Full Summary (partial-summary)
  activate LargeLanguageModel 
  return summary  

return summary  
```

## Notes and References

* First Notes
    * Use ollama with and llm to generate summaries of documents in blob store
    * Add lexical search with open search
    * Add classification to vectors such as content, summary, file name to allow searching across all of select items
    * Generate interlaced search result using both semantic and lexical result
    * Add claims Based filtering to show authorization
    * Markdown to html conversion with caching
    * Conversion caching with caching
    * Plantuml to png with caching
    * Setup as a deployable cluster


* [Pretrained Models - (sbert)](https://www.sbert.net/docs/pretrained_models.html)
* [Ollama](https://ollama.com/)
* Microsoft [phi-2](https://ollama.com/library/phi)
* [Open Search](https://opensearch.org/)
  * [OpenSearch - github](https://github.com/opensearch-project/OpenSearch)
* Sentence Transformers [all-mpnet-base-v2](https://huggingface.co/sentence-transformers/all-mpnet-base-v2)
* [Image to Text](https://huggingface.co/tasks/image-to-text)
* [Markdig](https://github.com/xoofx/markdig)
* [PlantUML](https://plantuml.com/)
* [Powering Bloop semantic code search](https://qdrant.tech/blog/case-study-bloop/)

* SQL Search
  * [Install SQL Server Full-Text Search on Linux](https://learn.microsoft.com/en-us/sql/linux/sql-server-linux-setup-full-text-search)
  * [semantickeyphrasetable (Transact-SQL)](https://learn.microsoft.com/en-us/sql/relational-databases/system-functions/semantickeyphrasetable-transact-sql)  * 
  * [Full-Text Index and Semantic Search in SQL Server ](https://www.kodyaz.com/sql-server-2014/fulltext-index-semantic-search-in-sql-server-2014.aspx)

* Others
  * [Similarity search](https://qdrant.tech/documentation/concepts/search/)
  * [RAG is Dead. Long Live RAG!](https://qdrant.tech/articles/rag-is-dead/)
  * [On Hybrid Search](https://qdrant.tech/articles/hybrid-search/)
  * [Semantic Search for Beginners](https://qdrant.tech/documentation/tutorials/search-beginners/)

* AI Examples
  * [Using Sentence Embeddings to Automate Customer Support](https://blog.floydhub.com/automate-customer-support-part-one/)
  * [Large Language Models vs Searchable Knowledge Bases: The Pros and Cons](https://www.servicexrg.com/blog/llm-vs-kb/)
  * [Text Summarization of Large Documents using LangChain](https://colab.research.google.com/github/GoogleCloudPlatform/generative-ai/blob/main/language/use-cases/document-summarization/summarization_large_documents_langchain.ipynb)
  * [rag-with-phi-2-and-langchain](https://github.com/rasyosef/rag-with-phi-2-and-langchain)

* doc services
  * conversion
    * (https://www.msweet.org/htmldoc/)
    * [wkhtmltopdf](https://wkhtmltopdf.org/)
  * versions
    * [Data Version Control](https://dvc.org/)
    * [A Modern Approach to Versioning Large Files for Machine learning and more](https://blog.infuseai.io/a-modern-approach-to-versioning-large-datasets-for-machine-learning-fca2f541dd85)
    * [Blob versioning - Azure Storage Services](https://learn.microsoft.com/en-us/azure/storage/blobs/versioning-overview)
  * webdav
    * [Awesome-WebDAV](https://github.com/fstanis/awesome-webdav)
    * [WebDAV Projects](http://webdav.org/projects/)
    * [sabre/dav](https://sabre.io/)
    * 
* Hybrid Search
  * [Relevance scoring in hybrid search using Reciprocal Rank Fusion (RRF)](https://learn.microsoft.com/en-us/azure/search/hybrid-search-ranking)
  * [Azure AI Search: Outperforming vector search with hybrid retrieval and ranking capabilities](https://techcommunity.microsoft.com/t5/ai-azure-ai-services-blog/azure-ai-search-outperforming-vector-search-with-hybrid/ba-p/3929167)
  * [Learning to Rank for Amazon OpenSearch Service](https://docs.aws.amazon.com/opensearch-service/latest/developerguide/learning-to-rank.html)
  * [MongoDB as a Graph Database](https://www.mongodb.com/databases/mongodb-graph-database)
  * [How to store n-dimensional vector in Microsoft SQL Server?](https://stackoverflow.com/questions/76167117/how-to-store-n-dimensional-vector-in-microsoft-sql-server)
  * [The Rise of Vector Databases](https://www.sqlservercentral.com/editorials/the-rise-of-vector-databases)
  * [StackEdit](https://stackedit.io/)
  * [POSTGRES FOR SEARCH AND ANALYTICS](https://www.paradedb.com/)

* RAG
  * [Retrieval Augmented Generation with Huggingface Transformers and Ray](https://medium.com/distributed-computing-with-ray/retrieval-augmented-generation-with-huggingface-transformers-and-ray-b09b56161b1e)
  * [Advanced Retrieval-Augmented Generation: From Theory to LlamaIndex Implementation](https://towardsdatascience.com/advanced-retrieval-augmented-generation-from-theory-to-llamaindex-implementation-4de1464a9930)
  * [RAG-ing Success: Guide to choose the right components for your RAG solution on AWS](https://medium.com/@pandey.vikesh/rag-ing-success-guide-to-choose-the-right-components-for-your-rag-solution-on-aws-223b9d4c7280)
  * [Should you Prompt, RAG, Tune, or Train? A Guide to Choose the Right Generative AI Approach](https://medium.com/@pandey.vikesh/should-you-prompt-rag-tune-or-train-a-guide-to-choose-the-right-generative-ai-approach-5e264043bd7d)
  * [Long context window models vs. RAG](https://medium.com/@jm_51428/long-context-window-models-vs-rag-a73c35a763f2)
  * [How to measure and prevent LLM hallucinations](https://www.promptfoo.dev/docs/guides/prevent-llm-hallucations/)
  * [The Curious Case of LLM Hallucination: Causes and Tips to Reduce its Risks](https://www.simform.com/blog/llm-hallucinations/)
  * [Hallucination is Inevitable: An Innate Limitation of Large Language Models](https://arxiv.org/pdf/2401.11817v1.pdf)
