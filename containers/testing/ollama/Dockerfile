FROM ollama/ollama:latest

# Start Ollama server in background, pull phi3 model, then stop
RUN /bin/ollama serve & \
    sleep 5 && \
    /bin/ollama pull phi3 && \
    pkill ollama

# Expose API port
EXPOSE 11434

# Start Ollama normally (base image already has ENTRYPOINT ["/bin/ollama"])
CMD ["serve"]
