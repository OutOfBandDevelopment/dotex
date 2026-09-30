# OoBDev.AI.Abstractions

## Summary

This library contains the abstract definitions for interfacing with ML/AI such as 
sentence embeddings and language models.

## Notes

This document outlines the OoBDev.AI namespace, detailing three key classes:

* AI.IEmbeddingProvider:
  * Represents a provider for word embeddings.
  * Length: Retrieves the length of the embeddings.
  * GetEmbeddingAsync(content): Retrieves the embedding vector for the given content. Takes 
    content as input and returns an array of single-precision floats representing the 
    embedding vector.

* AI.ILanguageModelProvider:
  * Represents a provider for a language model.
  * GetResponseAsync(promptDetails, userInput): Retrieves a response from the language model 
    based on provided prompt details and user input. Takes prompt details and user input as 
    inputs and returns a response from the language model.

* AI.IMessageCompletion:
  * Represents a provider for message completion.
  * GetCompletionAsync(modelName, prompt): Retrieves a completion for the given prompt from 
    the specified model. Takes model name and prompt as inputs and returns the completion 
    for the prompt.
