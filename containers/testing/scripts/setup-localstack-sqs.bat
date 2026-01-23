@echo off
REM Setup LocalStack SQS queues for integration testing

setlocal

set LOCALSTACK_URL=http://localhost:4566
set REGION=us-east-1

echo Setting up SQS queues in LocalStack at %LOCALSTACK_URL%...

REM Export AWS credentials for LocalStack (dummy values)
set AWS_ACCESS_KEY_ID=test
set AWS_SECRET_ACCESS_KEY=test
set AWS_DEFAULT_REGION=%REGION%

echo Creating queue: integration-test-queue
aws --endpoint-url=%LOCALSTACK_URL% sqs create-queue --queue-name integration-test-queue --region %REGION%

echo Creating queue: integration-test-fifo.fifo
aws --endpoint-url=%LOCALSTACK_URL% sqs create-queue --queue-name integration-test-fifo.fifo --attributes FifoQueue=true,ContentBasedDeduplication=true --region %REGION%

echo Creating queue: integration-test-dlq
aws --endpoint-url=%LOCALSTACK_URL% sqs create-queue --queue-name integration-test-dlq --region %REGION%

echo.
echo ✅ SQS queues created successfully!
echo.
echo Available queues:
aws --endpoint-url=%LOCALSTACK_URL% sqs list-queues --region %REGION%
echo.
echo Use these in your tests:
echo   Standard Queue: http://localhost:4566/000000000000/integration-test-queue
echo   FIFO Queue:     http://localhost:4566/000000000000/integration-test-fifo.fifo
echo   DLQ:            http://localhost:4566/000000000000/integration-test-dlq
echo.
echo Environment variables:
echo   set AWS_ACCESS_KEY_ID=test
echo   set AWS_SECRET_ACCESS_KEY=test
echo   set AWS_DEFAULT_REGION=us-east-1
echo   set SQS_QUEUE_URL=http://localhost:4566/000000000000/integration-test-queue

endlocal
