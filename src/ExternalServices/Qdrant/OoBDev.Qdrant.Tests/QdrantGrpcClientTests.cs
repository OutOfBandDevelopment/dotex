//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using Qdrant.Client.Grpc;
//using System.Threading.Tasks;

//namespace OoBDev.Qdrant.Tests;

//[TestClass]
//public class QdrantGrpcClientTests
//{
//    public required TestContext TestContext { get; set; }

//    //TODO: fix up

//    [DataTestMethod, TestCategory("setup")]
//    //[DataRow("192.168.1.170", "service-registry")]
//    //[DataRow("192.168.1.170", "csharp-files")]
//    //[DataRow("192.168.1.170", "csharp-files-phi")]
//    public async Task A_DeleteCollectionTest(string hostName, string collectionName)
//    {
//        var client = new QdrantGrpcClientFactory().Build($"http://{hostName}:6334");
//        var result = await client.Collections.DeleteAsync(new()
//        {
//            CollectionName = collectionName,
//        });
//        this.TestContext.WriteLine(result.Result ? "Created" : "Failed");
//    }

//    [DataTestMethod, TestCategory("setup")]
//    //[DataRow("192.168.1.170", "service-registry", Distance.Cosine, "llama2:13b")]
//    //[DataRow("192.168.1.170", "csharp-files", Distance.Cosine, "llama2:13b")]
//    //[DataRow("192.168.1.170", "csharp-files-phi", Distance.Cosine, "phi")]

//    //[DataRow("192.168.1.170", "d-example-phi", Distance.Cosine, "phi")]
//    //[DataRow("192.168.1.170", "d-example-llama2-13", Distance.Cosine, "llama2:13b")]
//    //[DataRow("192.168.1.170", "d-example-llama2-7", Distance.Cosine, "llama2:7b")]
//    //[DataRow("192.168.1.170", "d-example-orca-mini-3", Distance.Cosine, "orca-mini:3b")]
//    //[DataRow("192.168.1.170", "d-example-orca-mini-7", Distance.Cosine, "orca-mini:7b")]

//    //[DataRow("192.168.1.170", "d-example-orca-mini-7", Distance.Cosine, "orca-mini:7b")]
//    public async Task B_CreateCollectionTest(string hostName, string collectionName, Distance distanceCalculation, string modelName)
//    {
//        var ollama = new OllamaApiClientFactory().Build(hostName);
//        var qdrant = new QdrantGrpcClientFactory().Build($"http://{hostName}:6334");

//        var embeddingSize = (await ollama.GetEmbeddingSingleAsync("hello world", modelName)).Length;

//        if (collectionName.StartsWith("d-"))
//            embeddingSize /= 2;

//        var result = await qdrant.Collections.CreateAsync(new()
//        {
//            CollectionName = collectionName,
//            VectorsConfig = new()
//            {
//                Params = new()
//                {
//                    Size = (ulong)embeddingSize,
//                    Distance = distanceCalculation,
//                },
//            }
//        });
//        this.TestContext.WriteLine(result.Result ? "Created" : "Failed");
//    }

//    [DataTestMethod, TestCategory("setup")]
//    //[DataRow("192.168.1.170", "example-sbert", Distance.Cosine)]
//    [DataRow("192.168.1.170", "search-keys", Distance.Cosine)]
//    public async Task B2_CreateCollectionTest(string hostName, string collectionName, Distance distanceCalculation)
//    {
//        var client = new SBertClient($"http://{hostName}:5080");
//        var qdrant = new QdrantGrpcClientFactory().Build($"http://{hostName}:6334");

//        var embeddingSize = (await client.GetEmbeddingAsync("hello world")).Length;

//        var result = await qdrant.Collections.CreateAsync(new()
//        {
//            CollectionName = collectionName,
//            VectorsConfig = new()
//            {
//                Params = new()
//                {
//                    Size = (ulong)embeddingSize,
//                    Distance = distanceCalculation,
//                },
//            }
//        });
//        this.TestContext.WriteLine(result.Result ? "Created" : "Failed");
//    }

//    [DataTestMethod, TestCategory("setup")]
//    //[DataRow("192.168.1.170", "service-registry", typeof(LocalTimeProvider), LocalTimeProvider.EXAMPLE_QUESTION, "llama2:13b")]
//    //[DataRow("192.168.1.170", "service-registry", typeof(WeatherLookupProvider), WeatherLookupProvider.EXAMPLE_QUESTION, "llama2:13b")]
//    public async Task C_RegisterService(string hostName, string collectionName, Type serviceType, string serviceDescription, string modelName)
//    {
//        var registry = new ServiceRegistry(
//            new OllamaApiClientFactory().Build(hostName),
//            new QdrantGrpcClientFactory().Build($"http://{hostName}:6334"),
//            new PointStructFactory(),
//            modelName: modelName,
//            serviceCollectionName: collectionName
//            );
//        await registry.RegisterServiceAsync(
//            (IServiceInstance)Activator.CreateInstance(serviceType)
//            );
//    }

//    [DataTestMethod, TestCategory("setup")]
//    //[DataRow("192.168.1.170", "example-phi", Distance.Cosine, "phi")]
//    //[DataRow("192.168.1.170", "example-llama2-13", Distance.Cosine, "llama2:13b")]
//    //[DataRow("192.168.1.170", "example-llama2-7", Distance.Cosine, "llama2:7b")]
//    //[DataRow("192.168.1.170", "example-orca-mini-3", Distance.Cosine, "orca-mini:3b")]
//    //[DataRow("192.168.1.170", "example-orca-mini-7", Distance.Cosine, "orca-mini:7b")]

//    //[DataRow("192.168.1.170", "d-example-phi", Distance.Cosine, "phi")]
//    //[DataRow("192.168.1.170", "d-example-llama2-13", Distance.Cosine, "llama2:13b")]
//    //[DataRow("192.168.1.170", "d-example-llama2-7", Distance.Cosine, "llama2:7b")]
//    //[DataRow("192.168.1.170", "d-example-orca-mini-3", Distance.Cosine, "orca-mini:3b")]
//    //[DataRow("192.168.1.170", "d-example-orca-mini-7", Distance.Cosine, "orca-mini:7b")]
//    public async Task D_LoadExamplesTest(string hostName, string collectionName, Distance distanceCalculation, string modelName)
//    {
//        var ollama = new OllamaApiClientFactory().Build(hostName);
//        var qdrant = new QdrantGrpcClientFactory().Build($"http://{hostName}:6334");

//        var resource = this.GetType().Assembly.GetManifestResourceNames().FirstOrDefault(l => l.EndsWith(".sentences.txt"));
//        using var stream = this.GetType().Assembly.GetManifestResourceStream(resource);
//        using var reader = new StreamReader(stream);

//        while (!reader.EndOfStream)
//        {
//            var line = await reader.ReadLineAsync();
//            try
//            {
//                if (string.IsNullOrWhiteSpace(line.Trim())) continue;

//                var parts = line.Split([','], 2);
//                var section = parts[0].Trim();
//                var segment = parts[1].Trim();

//                this.TestContext.WriteLine(line);

//                var embedding =
//                    collectionName.StartsWith("d-") ?
//                    Array.ConvertAll(await ollama.GetEmbeddingDoubleAsync(segment, modelName), Convert.ToSingle) :
//                    (await ollama.GetEmbeddingSingleAsync(segment, modelName))
//                    ;
//                await qdrant.Points.UpsertAsync(new()
//                {
//                    CollectionName = collectionName,
//                    Points = {
//                    new PointStruct
//                    {
//                        Id = new() { Uuid = new Guid( segment.ToLower().GetHashBytes()).ToString(), },
//                        Payload =
//                        {
//                            ["line"]= line,
//                            ["section"]= section,
//                            ["segment"]= segment,
//                        },
//                        Vectors = new Vectors{ Vector = embedding }
//                    }
//                }
//                });
//            }
//            catch (Exception ex)
//            {
//                this.TestContext.WriteLine($"ERROR: \"{line}\" -> {ex.Message}");
//            }
//        }
//    }

//    [DataTestMethod, TestCategory("setup")]
//    //[DataRow("192.168.1.170", "example-sbert")]
//    [DataRow("127.0.0.1", "example-sbert")]
//    public async Task D2_LoadExamplesTest(string hostName, string collectionName)
//    {
//        var client = new SBertClient($"http://127.0.0.1:5080");
//        var qdrant = new QdrantGrpcClientFactory().Build($"http://{hostName}:6334");

//        var resource = this.GetType().Assembly.GetManifestResourceNames().FirstOrDefault(l => l.EndsWith(".sentences.txt"))
//            ?? throw new ApplicationException("missing .sentences.txt resource");
//        using var stream = this.GetType().Assembly.GetManifestResourceStream(resource)
//            ?? throw new ApplicationException("missing .sentences.txt resource");
//        using var reader = new StreamReader(stream);

//        while (!reader.EndOfStream)
//        {
//            var line = await reader.ReadLineAsync();
//            try
//            {
//                if (string.IsNullOrWhiteSpace(line?.Trim())) continue;

//                var parts = line.Split([','], 2);
//                var section = parts[0].Trim();
//                var segment = parts[1].Trim();

//                this.TestContext.WriteLine(line);

//                var embedding = await client.GetEmbeddingAsync(segment);
//                await qdrant.Points.UpsertAsync(new()
//                {
//                    CollectionName = collectionName,
//                    Points = {
//                    new PointStruct
//                    {
//                        Id = new() { Uuid = new Guid( segment.ToLower().GetHashBytes()).ToString(), },
//                        Payload =
//                        {
//                            ["line"]= line,
//                            ["section"]= section,
//                            ["segment"]= segment,
//                        },
//                        Vectors = new Vectors{ Vector = embedding }
//                    }
//                }
//                });
//            }
//            catch (Exception ex)
//            {
//                this.TestContext.WriteLine($"ERROR: \"{line}\" -> {ex.Message}");
//            }
//        }
//    }

//    [DataTestMethod, TestCategory("dev-local")]
//    //[DataRow("192.168.1.170", "docs", "smtp")]
//    [DataRow("127.0.0.1", "docs", "smtp")]
//    public async Task E_SearchTests(string hostName, string collectionName, string queryString)
//    {
//        var client = new SBertClient($"http://{hostName}:5080");
//        var qdrant = new QdrantGrpcClientFactory().Build($"http://{hostName}:6334");

//        var embedding = await client.GetEmbeddingAsync(queryString);

//        //todo: do something with paging
//        //todo: change to grouping 

//        // https://qdrant.tech/documentation/concepts/search/#search-groups
//        var results = await qdrant.Points.SearchAsync(new SearchPoints
//        {
//            CollectionName = collectionName,
//            Limit = 10,
//            Vector = { embedding },
//            WithPayload = true,
//        });
//        foreach (var item in results.Result)
//        {
//            Console.WriteLine(new string('-', 10));
//            Console.WriteLine(item.Payload["Content"]);
//            Console.WriteLine(item.Payload["PathHash"]);
//        }
//    }

//    [DataTestMethod, TestCategory("dev-local")]
//    //[DataRow("192.168.1.170", "docs", "smtp")]
//    [DataRow("127.0.0.1", "docs", "smtp")]
//    public async Task F_SearchTests(string hostName, string collectionName, string queryString)
//    {
//        var client = new SBertClient($"http://{hostName}:5080");
//        var qdrant = new QdrantGrpcClientFactory().Build($"http://{hostName}:6334");

//        var embedding = await client.GetEmbeddingAsync(queryString);

//        //todo: do something with paging
//        //todo: change to grouping 

//        // https://qdrant.tech/documentation/concepts/search/#search-groups
//        var results = await qdrant.Points.SearchGroupsAsync(new SearchPointGroups()
//        {
//            CollectionName = collectionName,
//            Limit = 10,
//            Vector = { embedding },
//            WithPayload = true,

//            GroupBy = "PathHash",
//            GroupSize = 1,
//        });

//        var groups = results.Result.Groups.SelectMany(s => s.Hits);
//        foreach (var item in groups)
//        {
//            Console.WriteLine(new string('-', 10));
//            Console.WriteLine(item.Payload["Content"]);
//            Console.WriteLine(item.Payload["PathHash"]);
//        }
//    }
//}
