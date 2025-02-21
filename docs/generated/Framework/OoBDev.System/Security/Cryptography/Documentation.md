**Hash Algorithm Suite Documentation**

**Overview**

The Hash Algorithm Suite is a collection of classes that provide hashing functionality for various algorithms. The suite includes classes for MD5, SHA-256, and SHA-512 hash algorithms.

**Hash Types**

### HashTypes Enum

```plantuml
@startuml
!include ./plantuml/src/uml/Sequence.puml
class HashTypes {
  - Md5
  - Sha256
  - Sha512
}

@enduml
```