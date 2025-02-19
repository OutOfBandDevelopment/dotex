

## Diagrams

###  Upload Document

```plantuml
start 

:upload content;

fork
    group store original content
        :store content;
    end group
fork again
    group convert if not text
        :detect content type;
        if (text) then (no)
            :convert document;
            :store converted content;
        endif
    end group
end fork

fork
    group index document content
        :chunk up content;
        while (has chunk)
            :get embedding;
            :store embedding;
        endwhile
    end group
fork again
    group summarize text
        :call summarization service;
        :store summary;
        :index summary;
            :get embedding;
            :store embedding;
    end group
end fork

stop
```

## Search content

```plantuml 
start 

:enter query;
:get embedding;
:query neighbors;
:re-rank and reduce documents;
:return results;

stop
```

### Upload Graphic

```plantuml
start 

:upload content;

fork
    group store original content
        :store content;
        :get image embedding;
        :store image embedding;
    end group
fork again
    group classify content
        :query llava model to generate description;
        :get embedding for description;
        :store embedding;
    end group   
fork again
    group OCR content
        :query image for OCR content;
        :chunk up content;
        while (has chunk)
            :get embedding;
            :store embedding;
        endwhile
    end group 
end fork

stop
```

## Notes

- [LLaVa](https://huggingface.co/docs/transformers/main/model_doc/llava) (Large Language and Vision assistant)
- [LLaMA](https://huggingface.co/docs/transformers/main/en/model_doc/llama)
- [Sentence Transformers](https://www.sbert.net/)
- [Image Similarity](https://huggingface.co/blog/image-similarity)
- [Sentence Similarity](https://huggingface.co/tasks/sentence-similarity)
- [Audio Classification](https://huggingface.co/tasks/audio-classification)
- [Hybrid Search](https://qdrant.tech/articles/hybrid-search/)