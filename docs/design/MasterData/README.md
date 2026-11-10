# Master Data and Test Data

Infrastructure for loading, versioning and managing reference data (master data) and for loading and cleaning up test data (Epic 05). Three features share one data-source abstraction.

**Status:** proposed (requirements dated 2026-01-22); no code in `src` yet.

**Table 1 — Features**

| Feature | Purpose | Documents |
|---------|---------|-----------|
| [MasterDataLoader](MasterDataLoader/requirements.md) | load, version and resolve dependencies between reference data sets | requirements, architecture, API design, testing strategy |
| [TestDataLoader](TestDataLoader/requirements.md) | scenario-based test data with isolation and automatic cleanup | requirements, architecture, API design, testing strategy |
| [DataSourceProviders](DataSourceProviders/requirements.md) | pluggable sources: JSON, CSV, XML, SQL, REST | requirements, architecture |
