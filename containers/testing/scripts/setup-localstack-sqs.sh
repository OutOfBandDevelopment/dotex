#!/bin/bash
# Setup LocalStack SQS queues for integration testing

set -e

LOCALSTACK_URL="${LOCALSTACK_URL:-http://localhost:4566}"
REGION="${AWS_DEFAULT_REGION:-us-east-1}"

echo "Setting up SQS queues in LocalStack at $LOCALSTACK_URL..."

# Export AWS credentials for LocalStack (dummy values)
export AWS_ACCESS_KEY_ID=test
export AWS_SECRET_ACCESS_KEY=test
export AWS_DEFAULT_REGION=$REGION

# Function to create queue
create_queue() {
    local queue_name=$1
    local is_fifo=$2

    echo "Creating queue: $queue_name"

    if [ "$is_fifo" = "true" ]; then
        aws --endpoint-url=$LOCALSTACK_URL sqs create-queue \
            --queue-name "${queue_name}.fifo" \
            --attributes FifoQueue=true,ContentBasedDeduplication=true \
            --region $REGION
    else
        aws --endpoint-url=$LOCALSTACK_URL sqs create-queue \
            --queue-name "$queue_name" \
            --region $REGION
    fi
}

# Create test queues
create_queue "integration-test-queue" false
create_queue "integration-test-fifo" true
create_queue "integration-test-dlq" false

echo ""
echo "✅ SQS queues created successfully!"
echo ""
echo "Available queues:"
aws --endpoint-url=$LOCALSTACK_URL sqs list-queues --region $REGION | grep -o 'http[^"]*'
echo ""
echo "Use these in your tests:"
echo "  Standard Queue: http://localhost:4566/000000000000/integration-test-queue"
echo "  FIFO Queue:     http://localhost:4566/000000000000/integration-test-fifo.fifo"
echo "  DLQ:            http://localhost:4566/000000000000/integration-test-dlq"
echo ""
echo "Environment variables:"
echo "  export AWS_ACCESS_KEY_ID=test"
echo "  export AWS_SECRET_ACCESS_KEY=test"
echo "  export AWS_DEFAULT_REGION=us-east-1"
echo "  export SQS_QUEUE_URL=http://localhost:4566/000000000000/integration-test-queue"
