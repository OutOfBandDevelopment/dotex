# Document Center - Document Generation Design

## Summary

The intention of document generation would be to abstractly convert document from simple text/markup to formatted rendered documents (HTML to PDF).  

Document content would be created could be created with the existing template/data enhancement processes then sent into a pipeline process to convert that content into the designed output format.

## Related Documents

* [Document Conversion](DocumentConversion.md)
* [Document Storage](DocumentStorage.md)
* [Communication Center](..\OoBDev.Communications.Contracts\readme.md)

## Proposed Designs

### Option 1 

```plantuml
@startuml

actor Agent
boundary DocumentCenter
control TextGeneration
control DataEnhancement
entity DocumentReference
entity BlobContainer
control TransformationEngine
control CommunicationCenter
actor AgentCallback

Agent -> DocumentCenter : Request(DocType, payload)
DocumentCenter -> DocumentReference : CreateRequestReference
DocumentCenter <- DocumentReference : {CorrelationId}
Agent <- DocumentCenter : {CorrelationId}
DocumentCenter -> DataEnhancement : Enhance(DocType, payload) 
DocumentCenter <- DataEnhancement : {EnhancedData}
DocumentCenter -> TextGeneration : GenerateContent(CorrelationId, DocType, EnhancedData)
DocumentCenter <- TextGeneration : {GeneratedContent}
DocumentCenter -> BlobContainer : Store(GeneratedContent)
DocumentCenter <- BlobContainer : {GeneratedContentRef}
DocumentCenter -> DocumentReference : StoreReference(CorrelationId, GeneratedContentRef, MimeType)
DocumentCenter -> TransformationEngine : Transform(CorrelationId, GeneratedContentRef)
TransformationEngine --> BlobContainer : PossibleCallback? {HTTP}
DocumentCenter <- TransformationEngine : {TransformedContent}
DocumentCenter -> BlobContainer : Store(TransformedContent)
DocumentCenter <- BlobContainer : {TransformedContentRef}
DocumentCenter -> DocumentReference : StoreReference(CorrelationId, TransformedContentRef, MimeType)
DocumentCenter --> CommunicationCenter : Send(agent.Id, "DocumentCreated", {CorrelationId, TransformedContentRef, MimeType})
CommunicationCenter --> AgentCallback
@enduml
```

### Option 2

```plantuml
@startuml

actor Agent
boundary DocumentCenter
control TextGeneration
control DataEnhancement
entity DocumentReference
entity BlobContainer
control TransformationEngine
control CommunicationCenter
actor AgentCallback

Agent -> DocumentCenter : Request(DocType, payload)
DocumentCenter -> DocumentReference : CreateRequestReference
DocumentCenter <- DocumentReference : {CorrelationId}
Agent <- DocumentCenter : {CorrelationId}
DocumentCenter -> DataEnhancement : Enhance(DocType, payload) 
DocumentCenter <- DataEnhancement : {EnhancedData}
DocumentCenter -> TextGeneration : GenerateContent(CorrelationId, DocType, EnhancedData)
DocumentCenter <- TextGeneration : {GeneratedContent}
DocumentCenter -> BlobContainer : Store(GeneratedContent)
DocumentCenter <- BlobContainer : {GeneratedContentRef}
BlobContainer -> DocumentReference : StoreReference(CorrelationId, GeneratedContentRef, MimeType)
DocumentReference --> CommunicationCenter : Send(agent.Id, "DocumentCreated", {CorrelationId, TransformedContentRef, MimeType})
CommunicationCenter --> AgentCallback
DocumentCenter -> TransformationEngine : Transform(CorrelationId, GeneratedContentRef)
TransformationEngine --> BlobContainer : PossibleCallback? {HTTP}
BlobContainer <-- TransformationEngine : Store(CorrelationId,TransformedContent, MimeType)
BlobContainer -> DocumentReference : StoreReference(CorrelationId, TransformedContentRef, MimeType)
DocumentReference --> CommunicationCenter : Send(agent.Id, "DocumentCreated", {CorrelationId, TransformedContentRef, MimeType})
CommunicationCenter --> AgentCallback

@enduml
```