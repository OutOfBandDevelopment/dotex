#!/bin/sh
# Creates the SQS queues the integration tests expect in the Moto server (runs once from the moto-init service)

set -e

ENDPOINT="${MOTO_URL:-http://moto:5000}"

echo "Initializing SQS queues at ${ENDPOINT}..."

aws --endpoint-url="${ENDPOINT}" sqs create-queue --queue-name integration-test-queue --region us-east-1

echo "SQS initialization complete!"
