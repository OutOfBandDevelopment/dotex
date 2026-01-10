# OoBDev - Amazon SQS

## Configuration

### Connection String

Connection String is a formatted string with all properties required to connect and authenticate to Amazon SQS

```keys
    Amazon:SimpleQueue:{Class or Simple Name}:ConnectionString
    Amazon:SimpleQueue:Default:ConnectionString
```
This is a set of key value pairs delimited by semicolon

```example
Region=us-east-1;AccessKeyId=XXXX;SecretAccessKey=YYYY
```

### Other Values

#### Queue Name

If not defined in code the queue name may be configured with the following keys. 
Default queue name is the name of the channel class

```keys
Amazon:SimpleQueue:TestTarget:QueueName
Amazon:SimpleQueue:Default:QueueName
```

#### Delay Seconds

Minimum time to hold between reads.  

```keys
Amazon:SimpleQueue:TestTarget:DelaySeconds
Amazon:SimpleQueue:Default:DelaySeconds
```

```notes
Default: 0
Minimum: 0
Maximum: 900
```

#### Lead Out Seconds

Minimum period before next read to end current read.

```keys
Amazon:SimpleQueue:TestTarget:LeadOutSeconds
Amazon:SimpleQueue:Default:LeadOutSeconds
```

```notes
Default: 10
Minimum: 50
Maximum: 300
```

#### Max Number Of Messages

Maximum number of messages to read from the queue at a time.

```keys
Amazon:SimpleQueue:TestTarget:MaxNumberOfMessages
Amazon:SimpleQueue:Default:MaxNumberOfMessages
```

```notes
Default: 10
Minimum: 0
Maximum: 10
```

#### Wait Time Seconds

Time to wait for next read

```keys
Amazon:SimpleQueue:TestTarget:WaitTimeSeconds
Amazon:SimpleQueue:Default:WaitTimeSeconds
```

```notes
Default: 20
Minimum: 0
Maximum: 20
```

## Regions

 | Display Name                   | System Name     |
 | ------------------------------ | --------------- |
 | Africa (Cape Town)             | af-south-1      |
 | Asia Pacific (Hong Kong)       | ap-east-1       |
 | Asia Pacific (Tokyo)           | ap-northeast-1  |
 | Asia Pacific (Seoul)           | ap-northeast-2  |
 | Asia Pacific (Mumbai)          | ap-south-1      |
 | Asia Pacific (Singapore)       | ap-southeast-1  |
 | Asia Pacific (Sydney)          | ap-southeast-2  |
 | Canada (Central)               | ca-central-1    |
 | EU Central (Frankfurt)         | eu-central-1    |
 | EU North (Stockholm)           | eu-north-1      |
 | Europe (Milan)                 | eu-south-1      |
 | EU West (Ireland)              | eu-west-1       |
 | EU West (London)               | eu-west-2       |
 | EU West (Paris)                | eu-west-3       |
 | Middle East (Bahrain)          | me-south-1      |
 | South America (Sao Paulo)      | sa-east-1       |
 | US East (Virginia)             | us-east-1       |
 | US East (Ohio)                 | us-east-2       |
 | US West (N. California)        | us-west-1       |
 | US West (Oregon)               | us-west-2       |
 | China (Beijing)                | cn-north-1      |
 | China (Ningxia)                | cn-northwest-1  |
 | US GovCloud East (Virginia)    | us-gov-east-1   |
 | US GovCloud West (Oregon)      | us-gov-west-1   |
 | US ISO East                    | us-iso-east-1   |
 | US ISOB East (Ohio)            | us-isob-east-1  |

## Notes

* https://docs.aws.amazon.com/AWSSimpleQueueService/latest/SQSDeveloperGuide/sqs-add-permissions.html
* https://docs.aws.amazon.com/AWSSimpleQueueService/latest/SQSDeveloperGuide/sqs-setting-up.html
