#!/bin/bash
# LocalStack init script to create SQS queues on startup
# This runs automatically when LocalStack container starts

set -e

echo "Initializing SQS queues..."

# Wait for LocalStack to be fully ready
awslocal sqs create-queue \
    --queue-name integration-test-queue \
    --region us-east-1 \
    || echo "Queue 'integration-test-queue' may already exist"

echo "SQS initialization complete!"
echo "Available queue: integration-test-queue"
