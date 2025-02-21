# OoBDev.System.Providers Documentation

## Overview

The OoBDev.System.Providers namespace contains two provider classes: `DateTimeProvider` and `GuidProvider`. These classes provide functionality for working with dates and times, and for generating and handling GUIDs.

### Class Diagram

```plantuml
@startuml
class DateTimeProvider implements IDateTimeProvider {
  -Now: DateTimeOffset
  -UtcNow: DateTimeOffset
}

class GuidProvider implements IGuidProvider {
  -Empty: Guid
  -NewGuid: Guid
}

interface IDateTimeProvider {
  Now(): DateTimeOffset
  UtcNow(): DateTimeOffset
}

interface IGuidProvider {
  Empty(): Guid
  NewGuid(): Guid
}

(DateTimeProvider) -- IDateTimeProvider
(GuidProvider) -- IGuidProvider
@enduml
```

### DateTimeProvider Class

The `DateTimeProvider` class provides methods for getting the current local date and time, as well as the current Coordinated Universal Time (UTC) date and time.

#### Class Documentation

```csharp
/// <summary>
/// Provides date and time functionality.
/// </summary>
public class DateTimeProvider : IDateTimeProvider
{
    /// <summary>
    /// Gets the current local date and time.
    /// </summary>
    /// <remarks>
    /// This property returns the current local date and time.
    /// </remarks>
    public DateTimeOffset Now => DateTimeOffset.Now;

    /// <summary>
    /// Gets the current Coordinated Universal Time (UTC) date and time.
    /// </summary>
    /// <remarks>
    /// This property returns the current Coordinated Universal Time (UTC) date and time.
    /// </remarks>
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
```

### GuidProvider Class

The `GuidProvider` class provides methods for generating and handling GUIDs.

#### Class Documentation

```csharp
/// <summary>
/// Represents a provider for generating and handling GUIDs.
/// </summary>
public class GuidProvider : IGuidProvider
{
    /// <summary>
    /// Gets an empty GUID.
    /// </summary>
    /// <remarks>
    /// This property returns a GUID with all bits set to zero.
    /// </remarks>
    public Guid Empty => Guid.Empty;

    /// <summary>
    /// Generates a new GUID.
    /// </summary>
    /// <returns>A new GUID.</returns>
    public Guid NewGuid() => Guid.NewGuid();
}
```

### Sequence Diagram

```plantuml
@startuml
sequenceDiagram
    participant DateTimeProvider as dp
    participant GuidProvider as gp
    participant Caller as c

    c->>dp: Get current local date and time
    dp->>dp: Now
    dp->>c: Now
    c->>gp: Get empty GUID
    gp->>gp: Empty
    gp->>c: Empty
    c->>gp: Generate new GUID
    gp->>gp: NewGuid()
    gp->>c: NewGuid
    note right: Caller uses DateTimeProvider to get the current local date and time and GuidProvider to get an empty GUID and generate a new GUID.
@enduml
```