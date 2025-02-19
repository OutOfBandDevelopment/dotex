**README**

This repository contains a set of test files for the OoBDev.System.Security.Cryptography namespace. The files are used to test the functionality of the HashSelector class and its derived classes (Md5Hash, Sha256Hash, and Sha512Hash) for hash generation.

The HashSelector class is responsible for selecting the correct hash algorithm based on the provided configuration. The derived classes (Md5Hash, Sha256Hash, and Sha512Hash) implement the specific hash algorithms.

The tests in this repository cover the following functionality:

1. **HashSelectorTest**: Tests the default hash selection based on the configuration settings.
2. **Md5HashTests**: Tests the Md5Hash class for correct hash generation.
3. **Sha256HashTests**: Tests the Sha256Hash class for correct hash generation.
4. **Sha512HashTests**: Tests the Sha512Hash class for correct hash generation.

**Technical Summary**

The code uses the following design patterns:

* **Dependency Injection**: The HashSelector class depends on the `IConfiguration` interface for configuration settings.
* **Factory Pattern**: The `HashSelector` class uses a factory method to create instances of the hash algorithm classes based on the configuration settings.
* **Strategy Pattern**: The hash algorithm classes (Md5Hash, Sha256Hash, and Sha512Hash) implement the specific hash algorithms, and the `HashSelector` class acts as a wrapper to select the correct algorithm based on the configuration settings.

**Component Diagram (Using PlantUML)**

```plantuml
@startuml
class HashSelector {
  - config: IConfiguration
  - hashAlgorithm: IHash
}

class IConfiguration {
  + DefaultHashType: HashTypes
}

class IHash {
  + GetHash(input: string): string
}

class Md5Hash {
  + GetHash(input: string): string
}

class Sha256Hash {
  + GetHash(input: string): string
}

class Sha512Hash {
  + GetHash(input: string): string
}

HashSelector -- config: uses
HashSelector -- hashAlgorithm: gets
IHash ..> Md5Hash | Sha256Hash | Sha512Hash
IConfiguration ..> HashSelector | uses

@enduml
```
In this diagram, the `HashSelector` class interacts with the `IConfiguration` interface for configuration settings and gets an instance of the `IHash` interface. The `IHash` interface has three implementations (Md5Hash, Sha256Hash, and Sha512Hash) that provide the specific hash generation functionality.