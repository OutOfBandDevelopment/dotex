FROM ollama/ollama:latest

# Start Ollama server in background, pull the chat (phi3) and embedding (all-minilm) models, then stop
RUN /bin/ollama serve & \
    sleep 5 && \
    /bin/ollama pull phi3 && \n    /bin/ollama pull all-minilm && \
    pkill ollama

# Expose API port
EXPOSE 11434

# Start Ollama normally (base image already has ENTRYPOINT ["/bin/ollama"])
CMD ["serve"]
