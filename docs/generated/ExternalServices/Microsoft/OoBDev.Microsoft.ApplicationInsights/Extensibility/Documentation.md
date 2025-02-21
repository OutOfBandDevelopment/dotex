**Documentation**

### CorrelationInfoTelemetryProcessor

**Class Diagram (PlantUML)**

```plantuml
@startuml
class CorrelationInfoTelemetryProcessor {
  - correlationAccessor: IAccessor<CorrelationInfo>
}

class ITelemetryProcessor {
  + void Process(ITelemetry item)
}

CorrelationInfoTelemetryProcessor -- audiences ITelemetryProcessor

@enduml
```

**Description**

The `CorrelationInfoTelemetryProcessor` class is an implementation of the `ITelemetryProcessor` interface. It adds correlation information to telemetry items by accessing correlation information through an `IAccessor` and setting it as global properties.

**Constructors**

* `public CorrelationInfoTelemetryProcessor(IAccessor<CorrelationInfo> stringAccessor)`: Initializes a new instance of the `CorrelationInfoTelemetryProcessor` class with the specified correlation accessor.

**Methods**

* `void Process(ITelemetry item)`: Processes a telemetry item by adding correlation information to the global properties.

**Properties**

* `correlationAccessor`: An accessor for managing correlation information.

### UserTelemetryProcessor

**Class Diagram (PlantUML)**

```plantuml
@startuml
class UserTelemetryProcessor {
  - accessor: IHttpContextAccessor
}

class ITelemetryProcessor {
  + void Process(ITelemetry item)
}

UserTelemetryProcessor -- audiences ITelemetryProcessor

class IHttpContextAccessor {
  + HttpContext HttpContext
}

UserTelemetryProcessor -- audiences IHttpContextAccessor

@enduml
```

**Description**

The `UserTelemetryProcessor` class is an implementation of the `ITelemetryProcessor` interface. It extracts user information from the HTTP context and adds it to telemetry items by accessing the HTTP context through an `IHttpContextAccessor` and setting it as global properties.

**Constructors**

* `public UserTelemetryProcessor(IHttpContextAccessor accessor)`: Initializes a new instance of the `UserTelemetryProcessor` class with the specified HTTP context accessor.

**Methods**

* `void Process(ITelemetry item)`: Processes a telemetry item by extracting user information from the HTTP context and adding it to the global properties.

**Properties**

* `accessor`: An accessor for accessing the current HTTP context.

**License**

The PlantUML diagrams are licensed under the [PlantUML License](https://plantuml.com/license).