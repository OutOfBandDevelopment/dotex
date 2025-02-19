Here is the documentation for the provided source code files, including class diagrams in PlantUML:

**ClientExampleTests.cs**
==========================

### Class Diagram

```
@startuml
class ClientExampleTests {
  - TestContext: TestContext
  - SendSmtpTest(string) : Task
  - GetImapTest(string) : Task
}

clientExampleTests ..> TestContext
clientExampleTests ..> (MimeMessage)
clientExampleTests ..> (InternetAddress)
@enduml
```

### Description

The `ClientExampleTests` is a test class that tests the `SmtpClient` and `ImapClient` classes. It contains two test methods: `SendSmtpTest` and `GetImapTest`. The `SendSmtpTest` method tests the ability to send an email using the `SmtpClient` class. The `GetImapTest` method tests the ability to retrieve email messages using the `ImapClient` class.

**OoBDev.MailKit.Tests.csproj**
==============================

### Project File

This is a .NET Core test project that references the `OoBDev.TestUtilities` and `OoBDev.MailKit` projects.

### Dependencies

* Microsoft.NET.Test.Sdk (version 17.10.0)
* MSTest.TestAdapter (version 3.4.3)
* MSTest.TestFramework (version 3.4.3)
* Coverlet.Collector (version 6.0.2)

**MailkitImapHealthCheckTests.cs**
=================================

### Class Diagram

```
@startuml
class MailkitImapHealthCheckTests {
  - HealthCheckContext: HealthCheckContext
  - IImapClientFactory: IImapClientFactory
  - IImapClient: IImapClient
  - CheckHealthAsync(HealthCheckContext) : HealthCheckResult
}

mailkitImapHealthCheckTests ..> HealthCheckContext
mailkitImapHealthCheckTests ..> IImapClientFactory
mailkitImapHealthCheckTests ..> IImapClient
@enduml
```

### Description

The `MailkitImapHealthCheckTests` class is a test class that tests the `MailkitImapHealthCheck` class, which is responsible for checking the health of an IMAP server. It contains two test methods: `CheckHealthAsyncTest_Healthy` and `CheckHealthAsyncTest_Degraded`. These methods test the `CheckHealthAsync` method of the `MailkitImapHealthCheck` class, which checks the health of the IMAP server and returns a `HealthCheckResult` object.

**MailkitSmtpHealthCheckTests.cs**
================================

### Class Diagram

```
@startuml
class MailkitSmtpHealthCheckTests {
  - HealthCheckContext: HealthCheckContext
  - ISmtpClientFactory: ISmtpClientFactory
  - ISmtpClient: ISmtpClient
  - CheckHealthAsync(HealthCheckContext) : HealthCheckResult
}

mailkitSmtpHealthCheckTests ..> HealthCheckContext
mailkitSmtpHealthCheckTests ..> ISmtpClientFactory
mailkitSmtpHealthCheckTests ..> ISmtpClient
@enduml
```

### Description

The `MailkitSmtpHealthCheckTests` class is a test class that tests the `MailkitSmtpHealthCheck` class, which is responsible for checking the health of an SMTP server. It contains two test methods: `CheckHealthAsyncTest_Healthy` and `CheckHealthAsyncTest_Degraded`. These methods test the `CheckHealthAsync` method of the `MailkitSmtpHealthCheck` class, which checks the health of the SMTP server and returns a `HealthCheckResult` object.

I hope this documentation is helpful! Let me know if you have any questions or need further clarification on any of the classes or methods.