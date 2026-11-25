> Status: moved from `docs/Framework/search-design.md` (2026-11-10); part of the [HybridSearch](README.md) design.



## Diagrams

### Upload Document

The branches inside a `split` run in parallel; `split` is used instead of `fork` because the PlantUML server crashes (`height=0.0`) on forks that contain groups.

```plantuml
@startuml
start

:upload content;

split
    group store original content
        :store content;
    end group
split again
    group convert if not text
        :detect content type;
        if (text) then (no)
            :convert document;
            :store converted content;
        endif
    end group
end split

split
    group index document content
        :chunk up content;
        while (has chunk)
            :get embedding;
            :store embedding;
        endwhile
    end group
split again
    group summarize text
        :call summarization service;
        :store summary;
        :index summary;
            :get embedding;
            :store embedding;
    end group
end split

stop
@enduml
```

## Search content

```plantuml
@startuml
start

:enter query;
:get embedding;
:query neighbors;
:re-rank and reduce documents;
:return results;

stop
@enduml
```

### Upload Graphic

```plantuml
@startuml
start

:upload content;

split
    group store original content
        :store content;
        :get image embedding;
        :store image embedding;
    end group
split again
    group classify content
        :query llava model to generate description;
        :get embedding for description;
        :store embedding;
    end group   
split again
    group OCR content
        :query image for OCR content;
        :chunk up content;
        while (has chunk)
            :get embedding;
            :store embedding;
        endwhile
    end group 
end split

stop
@enduml
```

## Notes

- [LLaVa](https://huggingface.co/docs/transformers/main/model_doc/llava) (Large Language and Vision assistant)
- [LLaMA](https://huggingface.co/docs/transformers/main/en/model_doc/llama)
- [Sentence Transformers](https://www.sbert.net/)
- [Image Similarity](https://huggingface.co/blog/image-similarity)
- [Sentence Similarity](https://huggingface.co/tasks/sentence-similarity)
- [Audio Classification](https://huggingface.co/tasks/audio-classification)
- [Hybrid Search](https://qdrant.tech/articles/hybrid-search/)