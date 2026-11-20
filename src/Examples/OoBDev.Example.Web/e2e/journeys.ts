import { expect, type Page } from '@playwright/test';
import type { Journey } from './journey';
import { testUser } from './signin';

const press = (page: Page, label: string) => page.locator('fluent-button', { hasText: label }).first().click();
const nav = (page: Page, label: string) => page.locator('nav.left').getByRole('link', { name: label, exact: true }).click();

export const journeys: Journey[] = [
  {
    id: 'explore-the-examples',
    title: 'Explore the examples',
    persona: 'A developer evaluating the OoBDev framework',
    goal: 'See which framework features have a working example and confirm the API behind them is healthy.',
    start: '/',
    prerequisites: ['The example stack is running (see the deployment guide).'],
    features: ['OoBDev.Example.WebApi', 'OoBDev.AspNetCore.Mvc (OpenAPI and AsyncAPI documents)'],
    why: 'One place to confirm every example endpoint is alive and to jump to the API and event references, before you start coding against them.',
    problems: [
      { symptom: 'An endpoint shows a red status', cause: 'The API container is not running or still starting', fix: 'Run `scripts/examples/example-up.ps1`, wait for healthy, press Check again' },
      { symptom: 'Every endpoint answers 0', cause: 'The browser cannot reach the API through the SPA proxy', fix: 'Open http://localhost:8088 (not the Vite port) or check `docker compose ps`' },
    ],
    steps: [
      {
        title: 'Open the overview',
        narrative: 'The overview lists every example endpoint and checks that each one answers.',
        expect: ['The page title reads "OoBDev framework examples".', 'Every probed endpoint shows OK.'],
        run: async page => {
          await expect(page.getByRole('heading', { name: 'OoBDev framework examples' })).toBeVisible();
          await expect(page.getByRole('cell', { name: 'OK' }).first()).toBeVisible();
        },
      },
      {
        title: 'Pick a feature from the left navigation',
        narrative: 'The navigation groups the examples by area. Choosing an entry opens its screen in the main blade and updates the breadcrumb.',
        expect: ['The breadcrumb shows Home > AI > Embeddings.'],
        run: async page => {
          await nav(page, 'Embeddings');
          await expect(page.locator('.crumbs')).toHaveText('Home > AI > Embeddings');
        },
      },
    ],
  },
  {
    id: 'chat-with-a-local-model',
    title: 'Chat with a local model',
    persona: 'A developer trying the AI abstractions without a cloud account',
    goal: 'Ask a question and read the answer from a model that runs in the Ollama container.',
    start: '/ai/chat',
    prerequisites: ['The Ollama container is running with the phi3 model.'],
    features: ['OoBDev.AI.Abstractions (`IMessageCompletion`, `IStreamedCompletion`)', 'OoBDev.Ollama'],
    why: 'Talk to a model that runs on your machine with no API key and no data leaving it, through the same `IMessageCompletion` interface you would use for a cloud model.',
    problems: [
      { symptom: 'Send never completes', cause: 'The phi3 model is still loading on first use', fix: 'Wait up to a minute, or check the Ollama container logs' },
      { symptom: 'Error toast with 500 or 502', cause: 'Ollama is not reachable from the API', fix: 'Confirm the Ollama options host and that the container is healthy' },
    ],
    steps: [
      {
        title: 'Write the instructions and the question',
        narrative: 'The instructions tell the model how to behave; the prompt is the question itself.',
        expect: ['The Send button is disabled until a prompt is entered.'],
        run: async page => {
          await expect(page.locator('fluent-button', { hasText: 'Send' })).toHaveAttribute('disabled', '');
          await page.getByLabel('Prompt').fill('In one sentence, what is an embedding?');
          await expect(page.locator('fluent-button', { hasText: 'Send' })).not.toHaveAttribute('disabled', '');
        },
      },
      {
        title: 'Send the prompt',
        narrative: 'Send waits for the whole answer. Stream shows the answer as the model produces it.',
        expect: ['The answer appears in the result panel.'],
        run: async page => {
          await press(page, 'Send');
          await expect(page.locator('pre.result')).not.toBeEmpty({ timeout: 120_000 });
        },
      },
    ],
  },
  {
    id: 'hold-a-conversation',
    title: 'Hold a conversation with Ollama',
    persona: 'A developer checking that the model remembers earlier turns',
    goal: 'Ask two questions in a row and see the conversation context carried between them.',
    start: '/ai/ollama',
    prerequisites: ['The Ollama container is running.'],
    features: ['OoBDev.AI.Abstractions (`CompletionRequest.Context`)', 'OoBDev.Ollama'],
    why: 'Keep context between questions by passing the previous exchange, so follow-ups are understood.',
    problems: [
      { symptom: 'Follow-up ignores earlier answers', cause: 'The conversation was reset', fix: 'Do not press New conversation between questions' },
      { symptom: 'Slow replies late in a conversation', cause: 'The context grows with each turn', fix: 'Start a new conversation' },
    ],
    steps: [
      {
        title: 'Ask the first question',
        narrative: 'Keep context between turns is on by default, so the model returns a context that the next request sends back.',
        expect: ['The model answer appears as a turn in the conversation.'],
        run: async page => {
          await page.getByLabel('Prompt').fill('Hello');
          await press(page, 'Send');
          await expect(page.locator('.turns')).toContainText('model:');
        },
      },
      {
        title: 'Ask a follow-up',
        narrative: 'The second request carries the context from the first, and the counter shows how much context is held.',
        expect: ['Both exchanges are listed (a user and a model line each).', 'The context token counter is shown.'],
        run: async page => {
          await page.getByLabel('Prompt').fill('Explain photosynthesis');
          await press(page, 'Send');
          await expect(page.locator('.turns p')).toHaveCount(4);
          await expect(page.getByText('context tokens:')).toBeVisible();
        },
      },
      {
        title: 'Start a new conversation',
        narrative: 'New conversation clears the turns and the stored context.',
        expect: ['The turn list is empty.'],
        run: async page => {
          await press(page, 'New conversation');
          await expect(page.locator('.turns p')).toHaveCount(0);
        },
      },
    ],
  },
  {
    id: 'compare-two-texts',
    title: 'Compare two texts with embeddings',
    persona: 'A developer learning how semantic similarity works',
    goal: 'Turn two sentences into vectors and read how close they are.',
    start: '/ai/embeddings',
    prerequisites: ['The all-MiniLM embedder is available in the API.'],
    features: ['OoBDev.Onnx.SentenceEmbeddings', 'OoBDev.AI.Abstractions (`IEmbeddingProvider`)'],
    why: 'Turn text into vectors and measure similarity in process, the basis of semantic search, deduplication and clustering without a remote service.',
    problems: [
      { symptom: 'First call is slow', cause: 'The ONNX model is downloaded on first use into the hub cache', fix: 'Wait once; later calls are fast' },
      { symptom: 'Download fails', cause: 'No internet access for the first download', fix: 'Provide the model files in the shared hub cache folder' },
    ],
    steps: [
      {
        title: 'Enter two texts',
        narrative: 'Choose the model and type the two sentences to compare.',
        expect: ['Both text boxes hold the entered sentences.'],
        run: async page => {
          await page.getByLabel('Text A').fill('The cat sat on the mat.');
          await page.getByLabel('Text B').fill('A kitten rests on a rug.');
          await expect(page.getByLabel('Text B')).toHaveValue('A kitten rests on a rug.');
        },
      },
      {
        title: 'Embed and compare',
        narrative: 'The screen shows the vector size, the cosine similarity (1 means identical meaning) and the first values of the vector.',
        expect: ['Dimensions shows 384.', 'A cosine similarity is displayed.'],
        run: async page => {
          await press(page, 'Embed and compare');
          await expect(page.locator('pre.result')).toContainText('Dimensions: 384');
          await expect(page.locator('pre.result')).toContainText('Cosine similarity: ');
        },
      },
    ],
  },
  {
    id: 'ask-your-own-documents',
    title: 'Ask a question about your own text',
    persona: 'A developer learning retrieval augmented generation',
    goal: 'Give the model a few passages, ask a question, and see which passages the answer came from.',
    start: '/ai/rag',
    prerequisites: ['The Ollama container is running with the phi3 model.'],
    features: ['OoBDev.Onnx.SentenceEmbeddings', 'OoBDev.Ollama', 'OoBDev.AI.Abstractions'],
    why: 'Ground answers in your own text (retrieval augmented generation) so the model answers from your passages instead of guessing.',
    problems: [
      { symptom: 'Answer ignores the passages', cause: 'Too few passages are selected', fix: 'Raise Passages to use' },
      { symptom: 'Request times out', cause: 'The model is cold or CPU only', fix: 'Retry; the first call loads the model' },
    ],
    steps: [
      {
        title: 'Review the passages and the question',
        narrative: 'Sample passages are filled in. Separate your own passages with a blank line.',
        expect: ['The Ask button is enabled.'],
        run: async page => {
          await expect(page.getByLabel('Question')).toHaveValue(/queues/);
          await expect(page.locator('fluent-button', { hasText: 'Ask' })).not.toHaveAttribute('disabled', '');
        },
      },
      {
        title: 'Ask',
        narrative: 'The API finds the closest passages by cosine similarity and asks the model to answer from them only.',
        expect: ['An answer is shown.', 'The sources table lists the passages used, closest first.'],
        run: async page => {
          await press(page, 'Ask');
          await expect(page.locator('pre.result')).not.toBeEmpty({ timeout: 120_000 });
          await expect(page.locator('table.grid tbody tr').first()).toBeVisible();
        },
      },
    ],
  },
  {
    id: 'check-the-running-service',
    title: 'Check the running service',
    persona: 'An operator',
    goal: 'See which environment the API runs in and find a configuration setting.',
    start: '/operations/info',
    prerequisites: [],
    features: ['OoBDev.AspNetCore.Mvc', 'Microsoft.Extensions.Configuration'],
    why: 'See the environment, runtime and effective configuration of a running API without shell access; secrets are masked.',
    problems: [
      { symptom: 'Configuration list is empty', cause: 'The call failed', fix: 'Read the error under Refresh and check authorization' },
      { symptom: 'A setting is missing', cause: 'It is not set for this environment', fix: 'Check the compose environment and appsettings files' },
    ],
    steps: [
      {
        title: 'Read the runtime facts',
        narrative: 'The page loads the environment, runtime and machine the API runs on.',
        expect: ['The environment is shown.'],
        run: async page => {
          await expect(page.getByRole('row', { name: /^Environment/ })).toBeVisible();
        },
      },
      {
        title: 'Find a setting',
        narrative: 'Type part of a key to narrow the configuration list. Secret values are masked by the API.',
        expect: ['Only matching settings remain.'],
        run: async page => {
          await page.getByLabel('Filter').fill('Templat');
          await expect(page.locator('table.grid').last().locator('tbody tr')).not.toHaveCount(0);
        },
      },
    ],
  },
  {
    id: 'cache-a-value',
    title: 'Cache a value',
    persona: 'A developer adding caching to a service',
    goal: 'Store text under a key, read it back, and remove it.',
    start: '/data/cache',
    prerequisites: [],
    features: ['OoBDev.Caching (`ICachingProvider`)', 'OoBDev.Microsoft.Caching', 'OoBDev.Redis.Caching'],
    why: 'Keep expensive results close by behind one interface; switch between in-memory and Redis by configuration, not code.',
    problems: [
      { symptom: 'Look up says not found right after Store', cause: 'The lifetime passed or the provider is per request', fix: 'Raise the seconds, and check which provider the lookup reports' },
      { symptom: 'Error about no ICachingProvider', cause: 'No caching provider is registered or selected', fix: 'Register a provider and set OoBDev:CachingProvider:Type' },
    ],
    steps: [
      {
        title: 'Store a value',
        narrative: 'Pick a key, some text and how long it should live, then press Store.',
        expect: ['A confirmation names the key.'],
        run: async page => {
          await press(page, 'Store');
          await expect(page.getByText('Stored "greeting"')).toBeVisible();
        },
      },
      {
        title: 'Look it up',
        narrative: 'Look the key up. The result also names the caching provider that answered.',
        expect: ['The stored text is shown.', 'The provider name is shown.'],
        run: async page => {
          await press(page, 'Look up');
          await expect(page.locator('pre.result')).toContainText('hello from the cache');
          await expect(page.locator('pre.result')).toContainText('Provider:');
        },
      },
    ],
  },
  {
    id: 'use-the-toolbox',
    title: 'Hash, sign, compare and calculate',
    persona: 'A developer checking integrity and similarity',
    goal: 'Hash some text, sign a message with a shared secret, measure how far apart two vectors are, evaluate an expression and get a one-time code.',
    start: '/utilities/toolbox',
    prerequisites: [],
    features: ['OoBDev.System (`IHash`, `IHMACCalculator`)', 'OoBDev.System.Abstractions (`VectorMath`)'],
    why: 'Pick the algorithm by key from configuration instead of hard-coding it; the default hash is SHA-512, and signing and vector math are one call each.',
    problems: [
      { symptom: 'Unknown algorithm error', cause: 'The key is not one of the registered keys', fix: 'Use the keys in the lists (sha512, hmac-256, ...)' },
      { symptom: 'Could not evaluate the expression', cause: 'Variables must start with a capital letter and have a value', fix: 'Use A or Rate2, and give each a value as name = value' },
      { symptom: 'Compare is disabled', cause: 'The vectors are not numbers or differ in length', fix: 'Enter the same count of numbers in both' },
    ],
    steps: [
      {
        title: 'Hash some text',
        narrative: 'Choose an algorithm and press Hash. The same text always gives the same hash.',
        expect: ['A hash is shown.'],
        run: async page => {
          await press(page, 'Hash');
          await expect(page.getByLabel('Hash result')).not.toBeEmpty();
        },
      },
      {
        title: 'Sign a message',
        narrative: 'Sign a message with a shared secret; the receiver recomputes the signature to prove the message is unchanged.',
        expect: ['A signature is shown.'],
        run: async page => {
          await press(page, 'Sign');
          await expect(page.getByLabel('Signature')).not.toBeEmpty();
        },
      },
      {
        title: 'Compare two vectors',
        narrative: 'Compare two number lists under cosine, Euclidean, dot product and Manhattan distance.',
        expect: ['All four distances are shown.'],
        run: async page => {
          await press(page, 'Compare');
          await expect(page.getByRole('table', { name: 'Distances' })).toContainText('Manhattan');
        },
      },
      {
        title: 'Calculate an expression',
        narrative: 'Type an expression with a variable, give the variable a value and calculate. The service also shows the optimized form.',
        expect: ['The result, optimized form and variable names are shown.'],
        run: async page => {
          await press(page, 'Calculate');
          await expect(page.getByRole('table', { name: 'Calculation' })).toContainText('Result');
        },
      },
      {
        title: 'Get a one-time code',
        narrative: 'Leave the secret empty to generate one, then read the current code and the authenticator URI.',
        expect: ['A code and an otpauth URI are shown.'],
        run: async page => {
          await press(page, 'Get code');
          await expect(page.getByRole('table', { name: 'One-time code' })).toContainText('otpauth://');
        },
      },
    ],
  },
  {
    id: 'query-and-render-text',
    title: 'Query a document and render Markdown',
    persona: 'A developer pulling values out of mixed documents',
    goal: 'Query a JSON document with XPath, see the XML view XPath works on, and render Markdown.',
    start: '/utilities/text',
    prerequisites: [],
    features: ['OoBDev.System (`JsonNavigator`)', 'OoBDev.System.Text.Yaml (`YamlNavigator`)', 'OoBDev.System.Text.Html (`HtmlNavigator`)', 'OoBDev.System.Text.Markdown (Markdig)'],
    why: 'One query language, XPath, reads JSON, YAML, HTML and XML, so extraction code does not change when the format does.',
    problems: [
      { symptom: 'Could not query the document', cause: 'The XPath is invalid or the document does not parse in the chosen format', fix: 'Check the format selector and open Document as XML to see the names XPath can use' },
      { symptom: 'No matches', cause: 'JSON and YAML property names become element names', fix: 'Use // to search at any depth, for example //total' },
    ],
    steps: [
      {
        title: 'Run an XPath query',
        narrative: 'The sample JSON document and query are filled in. Run the query to list every match.',
        expect: ['The match count and the matching elements are shown.'],
        run: async page => {
          await press(page, 'Run query');
          await expect(page.getByLabel('Match count')).toContainText('match');
          await expect(page.getByLabel('Matches')).toContainText('total');
        },
      },
      {
        title: 'Render Markdown',
        narrative: 'Render the sample Markdown to HTML; scripts and event handlers are stripped before it is shown.',
        expect: ['The rendered preview appears.'],
        run: async page => {
          await press(page, 'Render');
          await expect(page.getByLabel('Rendered Markdown')).toBeVisible();
        },
      },
    ],
  },
  {
    id: 'store-and-search-vectors',
    title: 'Store texts and find the closest',
    persona: 'A developer adding semantic search to an application',
    goal: 'Embed a few sentences into a collection and find the ones closest in meaning to a query.',
    start: '/utilities/vectors',
    prerequisites: [],
    features: ['OoBDev.Search.Abstractions (`IVectorStore`)', 'OoBDev.Data.Vectors.SqlStore (`SqlVectorStoreProvider`)', 'OoBDev.SBert.AllMiniLmL6V2 (embeddings)'],
    why: 'The vector store interface hides where vectors live, so the same code runs on SQL Server, Qdrant or in memory and ranks by meaning instead of keywords.',
    problems: [
      { symptom: 'Search returns nothing', cause: 'The collection is empty or holds vectors of a different length', fix: 'Press Store texts first, and keep one embedding model per collection' },
      { symptom: 'Stored in the memory store', cause: 'No SqlVectorStoreOptions:ConnectionString is configured, so the in-process store is used and is lost on restart', fix: 'Deploy OoBDev.Data.Vectors.DB and set the connection string to keep vectors in SQL Server' },
    ],
    steps: [
      {
        title: 'Store the sample texts',
        narrative: 'Four sentences are filled in, two about pets and two about money. Store them in the notes collection.',
        expect: ['A line reports how many texts were stored and in which store.'],
        run: async page => {
          await press(page, 'Store texts');
          await expect(page.getByLabel('Stored')).toContainText('Stored 4');
        },
      },
      {
        title: 'Search by meaning',
        narrative: 'The query a sleeping pet shares no words with the stored sentences but is close to the cat and kitten ones.',
        expect: ['The closest sentences are listed first with their distance.'],
        run: async page => {
          await press(page, 'Search');
          await expect(page.getByLabel('Hits')).toContainText('kitten');
        },
      },
    ],
  },
  {
    id: 'compare-vectors',
    title: 'Compare a set of vectors',
    persona: 'A data scientist checking how close embeddings are',
    goal: 'Measure every pair in a small set of vectors and see their lengths and centroid.',
    start: '/utilities/vector-lab',
    prerequisites: [],
    features: ['OoBDev.System.Abstractions (`Vector`, `VectorMath`, `VectorDistanceMetrics`)'],
    why: 'One vector type and four metrics give a quick check of whether embeddings group the way you expect before they go into a store.',
    problems: [
      { symptom: 'Compare is disabled', cause: 'A line is not numeric or the lines have different lengths', fix: 'Give one vector per line with the same number of values' },
      { symptom: 'Cosine distance is NaN', cause: 'A vector of all zeros has no direction', fix: 'Remove zero vectors or use the Euclidean metric' },
    ],
    steps: [
      {
        title: 'Compare the sample vectors',
        narrative: 'Four three-value vectors are filled in. Compare them with the cosine metric.',
        expect: ['A 4 by 4 distance matrix appears with zeros on the diagonal.', 'The lengths and centroid are shown.'],
        run: async page => {
          await press(page, 'Compare');
          await expect(page.getByLabel('Distance matrix')).toBeVisible();
          await expect(page.getByLabel('Centroid')).toContainText('Centroid');
        },
      },
    ],
  },
  {
    id: 'label-an-image',
    title: 'Label an image without training',
    persona: 'A developer adding image understanding',
    goal: 'Rank free-text labels against a picture and see its embedding.',
    start: '/ai/images',
    prerequisites: ['The first call downloads the CLIP model into the shared hub cache, so it needs internet once.'],
    features: ['OoBDev.Vision.ClipVitB32 (zero-shot classifier)', 'OoBDev.Onnx.ImageEmbeddings', 'OoBDev.Onnx.ImageEmbeddings.Skia'],
    why: 'CLIP scores any label you type against an image, so new categories need no retraining, and the same model gives a vector for image search.',
    problems: [
      { symptom: 'Classify is disabled', cause: 'No image is chosen or fewer than two labels are given', fix: 'Choose an image and enter at least two comma-separated labels' },
      { symptom: 'The first call fails', cause: 'The model could not be downloaded', fix: 'Provide internet access once, or copy the model files into the shared hub cache folder' },
    ],
    steps: [
      {
        title: 'Choose an image',
        narrative: 'Pick a picture; the default labels are already filled in.',
        expect: ['The Classify and Embed buttons are enabled.'],
        run: async page => {
          const png = Buffer.from('iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mP8z8BQDwAEhQGAhKmMIQAAAABJRU5ErkJggg==', 'base64');
          await page.locator('input[type=file]').setInputFiles({ name: 'cat.png', mimeType: 'image/png', buffer: png });
        },
      },
      {
        title: 'Rank the labels',
        narrative: 'Classify the image against the labels.',
        expect: ['A table lists the labels with probabilities, best first.'],
        run: async page => {
          await press(page, 'Classify');
          await expect(page.getByLabel('Ranked labels')).toContainText('a cat');
        },
      },
    ],
  },
  {
    id: 'compare-embedding-models',
    title: 'Compare sentence embedding models',
    persona: 'A developer choosing an embedding model',
    goal: 'Score the same pair of sentences with a chosen model and see its vector size and speed.',
    start: '/ai/model-compare',
    prerequisites: ['Each model downloads once into the shared hub cache, so the first use of a model needs internet.'],
    features: ['OoBDev.SBert.AllMiniLmL6V2', 'OoBDev.SBert.AllMpnetBaseV2', 'OoBDev.SBert.NomicEmbedTextV1_5', 'OoBDev.Onnx.SentenceEmbeddings'],
    why: 'Models are registered side by side under keys, so switching or comparing models is a change of key, not of code.',
    problems: [
      { symptom: 'Score is disabled', cause: 'A text is empty or no model is ticked', fix: 'Fill in both texts and tick at least one model' },
      { symptom: 'A model fails on first use', cause: 'The model files could not be downloaded', fix: 'Provide internet access once, or copy the files into the shared hub cache folder' },
    ],
    steps: [
      {
        title: 'Score the sample sentences',
        narrative: 'Two similar sentences are filled in with the first model ticked. Score them.',
        expect: ['A table shows the model, its vector size, the similarity and the time taken.'],
        run: async page => {
          await press(page, 'Score');
          await expect(page.getByLabel('Model scores')).toContainText('all-minilm-l6-v2');
        },
      },
    ],
  },
  {
    id: 'see-recent-traces',
    title: 'See what the API just did',
    persona: 'An operator checking a request',
    goal: 'List the latest traces the API sent to the dashboard and jump to one.',
    start: '/operations/telemetry',
    prerequisites: ['The compose stack is running, which includes the Aspire dashboard.'],
    features: ['OoBDev.OpenTelemetry (OTLP export)', 'Aspire dashboard telemetry API'],
    why: 'Traces and logs leave the service over OTLP, so any dashboard can show them; this page reads the latest back and links into the dashboard for the detail.',
    problems: [
      { symptom: 'No traces yet', cause: 'Nothing has called the API since it started', fix: 'Use another screen, then refresh' },
      { symptom: 'The dashboard could not be read', cause: 'The API cannot reach the dashboard or the API key is wrong', fix: 'Check Telemetry__DashboardUrl and Telemetry__ApiKey in the compose environment' },
    ],
    steps: [
      {
        title: 'Open the telemetry page',
        narrative: 'The page loads the most recent traces on its own; use Refresh to reload them.',
        expect: ['A table lists recent spans with their duration and a trace link.', 'A link opens the dashboard.'],
        run: async page => {
          await expect(page.getByLabel('Recent traces')).toContainText('HttpRequestIn');
          await press(page, 'Refresh');
          await expect(page.getByRole('link', { name: 'Open the dashboard' })).toBeVisible();
        },
      },
    ],
  },
  {
    id: 'render-a-template',
    title: 'Render a text template',
    persona: 'A developer producing documents or email bodies from data',
    goal: 'Fill a template with JSON data and read the result.',
    start: '/content/templates',
    prerequisites: [],
    features: ['OoBDev.Text.Templating'],
    why: 'Generate emails, pages and documents from files named after template, target type and engine, so content changes without code changes.',
    problems: [
      { symptom: 'No supported types listed', cause: 'The template folder is wrong', fix: 'Check FileTemplatingOptions:TemplatePath and that the files are copied to the output' },
      { symptom: 'Placeholders stay unreplaced', cause: 'The model property names differ from the template variables', fix: 'Match the JSON property names to the template' },
    ],
    steps: [
      {
        title: 'See the supported template types',
        narrative: 'On load the screen asks the API which template formats it supports.',
        expect: ['The supported types list is shown.'],
        run: async page => {
          await expect(page.locator('pre.result').last()).toContainText('.hbs');
        },
      },
      {
        title: 'Apply the template',
        narrative: 'Give the template name and its data, then apply it.',
        expect: ['The rendered output appears under Output.'],
        run: async page => {
          await page.getByLabel('Template name').fill('order-shipped');
          await page.getByLabel('Data (JSON)').fill('{ "name": "Ada", "order": 1042 }');
          await press(page, 'Apply');
          await expect(page.locator('pre.result').first()).toContainText('Hello Ada');
        },
      },
    ],
  },
  {
    id: 'send-an-email',
    title: 'Send an email through a channel',
    persona: 'A developer wiring notifications into an application',
    goal: 'Send a message through the public channel and read the acknowledgement.',
    start: '/messaging/communications',
    prerequisites: ['The SMTP4Dev container is running to receive the message.'],
    features: ['OoBDev.Communications', 'OoBDev.MailKit'],
    why: 'Send mail through the communications abstraction, so channels can change without touching callers.',
    problems: [
      { symptom: 'Send fails with connection refused', cause: 'The SMTP host points at localhost inside the container', fix: 'Set MailKitSmtpClientOptions__Host to the smtp4dev service name' },
      { symptom: 'Message not seen', cause: 'Wrong inbox', fix: 'Open the smtp4dev web page and refresh' },
    ],
    steps: [
      {
        title: 'Choose a channel and write the message',
        narrative: 'Public sends straight away, Queued hands the message to the queue, Private needs a signed-in user.',
        expect: ['The Public channel is selected by default.'],
        run: async page => {
          await expect(page.getByRole('radio', { name: 'Public' })).toBeChecked();
          await page.getByLabel('To').fill('ada@example.test');
          await page.getByLabel('Subject').fill('Your order has shipped');
          await page.getByLabel('Body').fill('Order 1042 is on its way.');
        },
      },
      {
        title: 'Send',
        narrative: 'The API acknowledges the message with a reference id.',
        expect: ['The result says the mail was accepted.'],
        run: async page => {
          await press(page, 'Send');
          await expect(page.locator('pre.result')).toContainText('accepted');
        },
      },
    ],
  },
  {
    id: 'queue-a-message',
    title: 'Put a message on a queue',
    persona: 'A developer testing asynchronous processing',
    goal: 'Enqueue a message and find the event reference that describes the queue.',
    start: '/messaging/queues',
    prerequisites: ['RabbitMQ (or another queue adapter) is running.'],
    features: ['OoBDev.MessageQueue', 'OoBDev.AsyncApi'],
    why: 'Hand work to a queue through one interface so RabbitMQ, Service Bus, Storage queues or SQS can be swapped by configuration.',
    problems: [
      { symptom: 'Queued but nothing consumes it', cause: 'No consumer is running for the example queue', fix: 'Check the AsyncAPI page for the channel and start a consumer' },
      { symptom: 'Connection error', cause: 'The queue host is localhost inside the container', fix: 'Set MessageQueue__NAME__Config__HostName to the service name' },
    ],
    steps: [
      {
        title: 'Write the message',
        narrative: 'Give the message input text and optional JSON data.',
        expect: ['The Enqueue button is enabled once input is entered.'],
        run: async page => {
          await page.getByLabel('Input').fill('rebuild-search-index');
          await page.getByLabel('Data (JSON)').fill('{ "full": true }');
        },
      },
      {
        title: 'Enqueue',
        narrative: 'The API confirms the message was queued. The link beside the button opens the generated AsyncAPI event reference.',
        expect: ['The result says the message was queued.', 'The event reference link points to the "all" AsyncAPI document.'],
        run: async page => {
          await press(page, 'Enqueue');
          await expect(page.locator('pre.result')).toContainText('Queued');
          await expect(page.getByRole('link', { name: 'Open event reference' })).toHaveAttribute('href', '/asyncapi/all');
        },
      },
    ],
  },
  {
    id: 'convert-a-document',
    title: 'Convert and store a document',
    persona: 'A developer handling uploaded files',
    goal: 'Upload a file, convert it to plain text and keep it in the document store.',
    start: '/content/documents',
    prerequisites: [],
    features: ['OoBDev.Documents'],
    why: 'Convert and store documents (text, HTML, PDF, markdown) behind one service, so callers never depend on a converter library.',
    problems: [
      { symptom: 'Conversion shows a binary placeholder', cause: 'The target type is not text', fix: 'Use the Download or PDF links for binary results' },
      { symptom: 'Upload does nothing', cause: 'The store name is empty', fix: 'Enter a name in store before uploading' },
    ],
    steps: [
      {
        title: 'Choose a file',
        narrative: 'Pick the file to work with; every action below uses it.',
        expect: ['The Convert button is enabled once a file is chosen.'],
        run: async page => {
          await page.locator('input[type=file]').setInputFiles({ name: 'report.txt', mimeType: 'text/plain', buffer: Buffer.from('Quarterly report\n\nRevenue grew 12 percent.') });
        },
      },
      {
        title: 'Convert it',
        narrative: 'Choose the target type and convert. The converted text appears below.',
        expect: ['The converted text appears in the result panel.'],
        run: async page => {
          await press(page, 'Convert');
          await expect(page.locator('pre.result')).toContainText('Revenue grew 12 percent');
        },
      },
      {
        title: 'See what is stored',
        narrative: 'The Stored documents list shows every document in the store; choosing a name fills the Name in store box.',
        expect: ['The stored document is listed.'],
        run: async page => {
          await expect(page.locator('.stored-list li').first()).toContainText('quarterly-report.docx');
          await press(page, 'quarterly-report.docx');
        },
      },
    ],
  },
  {
    id: 'sign-in',
    title: 'Sign in with Keycloak',
    persona: 'A user opening the example for the first time',
    goal: 'Sign in through the identity provider and land on the screen that was asked for.',
    start: '/identity/me',
    liveOnly: true,
    prerequisites: ['The compose stack is running with sign-in on (the default) and the `oobdev-example` realm imported.', 'Use the sample user `ada` (password `ada`) or `grace` (password `grace`, also an admin).'],
    features: ['OoBDev.Identity', 'OoBDev.AspNetCore.JwtAuthentication', 'Keycloak (authorization code with PKCE)'],
    why: 'Authenticate with OpenID Connect and PKCE so the SPA and API handle only tokens, never passwords.',
    problems: [
      { symptom: 'Redirect error invalid_redirect_uri', cause: 'The client redirect URIs do not include this origin', fix: 'Update the realm client or re-import the realm (remove the Keycloak volume)' },
      { symptom: 'Credentials rejected', cause: 'Wrong sample user', fix: 'See containers/example/keycloak/dev-users.md' },
    ],
    steps: [
      {
        title: 'Get sent to the identity provider',
        narrative: 'Every screen needs a token, so the application sends you to Keycloak. The application never sees your password.',
        expect: ['The Keycloak sign-in form is shown.'],
        run: async page => {
          await page.waitForURL(/\/realms\//);
          await expect(page.locator('#kc-login')).toBeVisible();
        },
      },
      {
        title: 'Enter your credentials',
        narrative: 'Type the user name and password of a sample user and sign in.',
        expect: ['You return to the screen you asked for.', 'The header shows your user name.'],
        run: async page => {
          await page.locator('#username').fill(testUser.name);
          await page.locator('#password').fill(testUser.password);
          await page.locator('#kc-login').click();
          await expect(page.locator('header')).toContainText(testUser.name);
        },
      },
      {
        title: 'Read your claims',
        narrative: 'The current user screen lists the claims the API read from your access token.',
        expect: ['The preferred user name claim is listed.'],
        run: async page => {
          await expect(page).toHaveURL(/identity\/me/);
          await expect(page.getByText(testUser.name).first()).toBeVisible();
        },
      },
    ],
  },
  {
    id: 'see-who-you-are',
    title: 'See the claims on your token',
    persona: 'A developer checking authentication',
    goal: 'Read the claims the API sees for the current user.',
    start: '/identity/me',
    prerequisites: ['Sign-in is configured, or the API runs with identity disabled for local work.'],
    features: ['OoBDev.Identity', 'Keycloak (OIDC, JWT bearer)'],
    why: 'Inspect the claims the API sees on your token, the quickest way to debug roles and rights.',
    problems: [
      { symptom: '401 Unauthorized', cause: 'Not signed in or the token expired', fix: 'Sign in again' },
      { symptom: 'Expected role missing', cause: 'The user does not have the role in the realm', fix: 'Check the user in Keycloak and dev-users.md' },
    ],
    steps: [
      {
        title: 'Open the current user screen',
        narrative: 'The screen calls the API and lists every claim on the access token.',
        expect: ['The name claim is listed.'],
        run: async page => {
          await expect(page.getByText('Ada Lovelace')).toBeVisible();
        },
      },
    ],
  },
  {
    id: 'search-three-ways',
    title: 'Search passages by keyword, meaning and both',
    persona: 'A developer choosing a search strategy',
    goal: 'Run one query three ways and see what each mode finds.',
    start: '/ai/search',
    prerequisites: ['The all-MiniLM embedder is available in the API.'],
    features: ['OoBDev.Onnx.SentenceEmbeddings', 'OoBDev.AI.Abstractions (`IEmbeddingProvider`)'],
    why: 'See why keyword search misses paraphrases, why embeddings rank everything, and how rank fusion combines them, with the scoring in plain view.',
    problems: [
      { symptom: 'Keywords find nothing', cause: 'The query shares no word with any passage', fix: 'Use the meaning or hybrid list, or reword the query' },
      { symptom: 'Bad request', cause: 'The query or the passages are empty', fix: 'Fill in both' },
    ],
    steps: [
      {
        title: 'Review the passages and the query',
        narrative: 'Sample passages and a query are filled in. Separate your own passages with a blank line.',
        expect: ['The Search button is enabled.'],
        run: async page => {
          await expect(page.locator('fluent-button', { hasText: 'Search' })).not.toHaveAttribute('disabled', '');
        },
      },
      {
        title: 'Search',
        narrative: 'The same query runs by keywords (BM25), by meaning (embeddings) and by reciprocal rank fusion of both.',
        expect: ['Three result lists are shown.', 'The first list has a hit with a score.'],
        run: async page => {
          await press(page, 'Search');
          await expect(page.getByRole('heading', { name: 'Hybrid (rank fusion)' })).toBeVisible();
          await expect(page.locator('table.grid tbody tr').first()).toBeVisible();
        },
      },
    ],
  },
  {
    id: 'preview-a-template',
    title: 'Open and preview a template',
    persona: 'A developer working with text templates',
    goal: 'Open a template file, change its text and see it rendered with sample data without saving anything.',
    start: '/content/templates',
    prerequisites: ['The example Templates folder holds the sample templates.'],
    features: ['OoBDev.System (`ITemplateEngine`, `FileTemplateSource`)', 'OoBDev.Handlebars'],
    why: 'Try a Handlebars template against JSON data and see the output at once, before wiring it into code.',
    problems: [
      { symptom: 'Preview shows a template error', cause: 'The draft text is not valid for the engine', fix: 'Fix the braces or the helper name shown in the message' },
      { symptom: 'The file list is empty', cause: 'The template folder does not exist or holds no template files', fix: 'Check the file template path option' },
    ],
    steps: [
      {
        title: 'Open a template file',
        narrative: 'The list shows every template file; choosing one loads its text into the source box.',
        expect: ['The source box holds the template text.'],
        run: async page => {
          await press(page, 'order-shipped.html.hbs');
          await expect(page.getByLabel('Template source')).toHaveValue(/Hello/);
        },
      },
      {
        title: 'Preview the draft',
        narrative: 'Preview renders the text in the box with the JSON data. Nothing is saved.',
        expect: ['The output shows the rendered text.'],
        run: async page => {
          await press(page, 'Preview draft');
          await expect(page.locator('pre.result').first()).toContainText('Hello Ada');
        },
      },
    ],
  },
  {
    id: 'watch-the-queues',
    title: 'See the queue channels and received messages',
    persona: 'A developer checking the message queue wiring',
    goal: 'Confirm which provider and queue each channel uses and whether this host receives from it.',
    start: '/messaging/queues',
    prerequisites: ['The example API is running.'],
    features: ['OoBDev.MessageQueueing', 'OoBDev.RabbitMq, OoBDev.Microsoft.Azure.StorageAccount'],
    why: 'See the configured channels without opening configuration files, and watch what the handler receives.',
    problems: [
      { symptom: 'Received list stays empty', cause: 'The channel has DisableReceiver set, or nothing was sent', fix: 'Enable the receiver in the MessageQueue section and send a message' },
    ],
    steps: [
      {
        title: 'Read the configured channels',
        narrative: 'The table lists each channel with its provider, queue, host and whether the receiver is on. Connection strings are never shown.',
        expect: ['The channel table lists the provider and queue.'],
        run: async page => {
          await expect(page.locator('table.channels tbody tr').first()).toContainText('rabbit-mq');
        },
      },
      {
        title: 'Look at the received messages',
        narrative: 'The list shows the last messages this host received, newest first. Refresh to update it.',
        expect: ['A received message is listed with its time and text.'],
        run: async page => {
          await press(page, 'Refresh');
          await expect(page.locator('.received-list li').first()).toContainText('hello');
        },
      },
    ],
  },
  {
    id: 'run-a-cgi-program',
    title: 'Run a CGI program',
    persona: 'A developer checking the CGI hosting',
    goal: 'Pick a program the host can start and see its answer.',
    start: '/developer/cgi',
    prerequisites: ['The example API is running and cgi-bin holds a program built for this system.'],
    features: ['OoBDev.AspNetCore.Cgi', 'OoBDev.AspNetCore.Cgi.OpenApi'],
    why: 'See a CGI program answer through the middleware, with its status, without a command line.',
    problems: [
      { symptom: 'The program list is empty', cause: 'No program for this system is in cgi-bin', fix: 'Run cgi-src/build.ps1' },
      { symptom: '500 The program could not start', cause: 'A binary for another system', fix: 'Build the program for this system' },
    ],
    steps: [
      {
        title: 'Run the program',
        narrative: 'The first program is selected. Get calls it with the name; the status and the page it returned are shown.',
        expect: ['The status is 200.', 'The answer greets the name.'],
        run: async page => {
          await press(page, 'Get');
          await expect(page.locator('.cgi-status')).toContainText('200');
          await expect(page.locator('pre.result')).toContainText('Hello, Ada');
        },
      },
    ],
  },
  {
    id: 'send-a-webhook',
    title: 'Send a webhook to a channel',
    persona: 'A developer testing inbound provider calls',
    goal: 'Post a provider call to a channel and see how the host answers.',
    start: '/messaging/webhooks',
    prerequisites: ['The example API is running.'],
    features: ['OoBDev.Communications (`IReceiveService`)'],
    why: 'See how the host answers a provider call: 204 when processed, 400 for a bad signature, 404 for an unknown channel.',
    problems: [
      { symptom: '404 Webhook rejected', cause: 'The channel is not in OoBDev:Communications:Channels', fix: 'Use a configured channel name' },
      { symptom: '400 Webhook rejected', cause: 'The vendor signature is missing or wrong', fix: 'Send the call from the provider, or sign it as the vendor does' },
    ],
    steps: [
      {
        title: 'Send the call',
        narrative: 'The channel and a sample body are filled in. The response status and problem details are shown.',
        expect: ['A response status is shown.', 'The problem details name the rejection.'],
        run: async page => {
          await press(page, 'Send webhook');
          await expect(page.locator('.webhook-status')).toContainText('404');
          await expect(page.locator('pre.result')).toContainText('Webhook rejected');
        },
      },
    ],
  },
  {
    id: 'check-your-rights',
    title: 'Check which rights an endpoint needs',
    persona: 'A developer working out why an API call is refused',
    goal: 'See the access each endpoint needs and check the current user against a right.',
    start: '/identity/rights',
    prerequisites: ['The example API is running.'],
    features: ['OoBDev.AspNetCore.Abstractions (`[ApplicationRight]`, `IApplicationRightChecker`)'],
    why: 'See the authorization model in one place: anonymous, authenticated or application rights, the same on every API surface.',
    problems: [
      { symptom: 'The check says Unauthenticated', cause: 'The host runs anonymously or the token is missing', fix: 'Sign in, then check again' },
      { symptom: 'The check says Forbidden', cause: 'The token has none of the listed rights', fix: 'Ask for a role that is translated to one of the rights' },
    ],
    steps: [
      {
        title: 'Read the endpoint table',
        narrative: 'Every endpoint is listed with its access. The filter narrows the list to a route or a right.',
        expect: ['The table lists the rights.demo endpoint with access rights.'],
        run: async page => {
          await page.getByLabel('Filter').fill('demo');
          await expect(page.locator('.rights-table')).toContainText('rights.demo');
        },
      },
      {
        title: 'Check a right',
        narrative: 'Check asks the host whether the current user holds at least one of the listed rights.',
        expect: ['A result is shown with the rights the user holds.'],
        run: async page => {
          await press(page, 'Check');
          await expect(page.locator('.rights-result')).toContainText('Unauthenticated');
          await expect(page.locator('.rights-held')).toContainText('none');
        },
      },
    ],
  },
  {
    id: 'review-the-right-audit-trail',
    title: 'Review the right audit trail',
    persona: 'An operator looking into a refused call',
    goal: 'See which right checks the API allowed or refused, and narrow the list to the refusals.',
    start: '/operations/right-audit',
    prerequisites: ['The example API runs with OoBDev:RightAudit:ConnectionString set.', 'The signed-in user holds the audit.read right.'],
    features: ['OoBDev.AspNetCore.Abstractions (`IApplicationRightChecker`)', 'OoBDev.AspNetCore.EntityFrameworkCore (`TryAddEntityFrameworkRightAudit`)'],
    why: 'Answer "who was refused, and for which right" from the durable trail, without reading logs.',
    problems: [
      { symptom: 'The screen says Not configured', cause: 'The host has no audit connection string, so the endpoint is not mapped', fix: 'Set OoBDev:RightAudit:ConnectionString (SQL Server) and restart the API' },
      { symptom: 'The screen shows 403', cause: 'Your token has no role that maps to audit.read', fix: 'Ask for a role that is translated to audit.read' },
    ],
    steps: [
      {
        title: 'Read the trail',
        narrative: 'Every right check is listed newest first with who asked, the rights and the result.',
        expect: ['Both checks are listed, the allowed one and the forbidden one.'],
        run: async page => {
          await expect(page.locator('table.grid')).toContainText('ann');
          await expect(page.locator('table.grid')).toContainText('Forbidden');
        },
      },
      {
        title: 'Show only the refusals',
        narrative: 'Choose Forbidden as the result and refresh to narrow the list.',
        expect: ['Only the forbidden check remains.'],
        run: async page => {
          await page.getByLabel('Result').selectOption('Forbidden');
          await press(page, 'Refresh');
          await expect(page.locator('table.grid')).toContainText('bob');
          await expect(page.locator('table.grid')).not.toContainText('Allowed');
        },
      },
    ],
  },
  {
    id: 'browse-users-and-roles',
    title: 'Browse users and roles',
    persona: 'An administrator checking who can do what',
    goal: 'See the realm users with their roles and the application rights those roles grant.',
    start: '/identity/directory',
    prerequisites: ['Sign-in is configured and the signed-in user holds the identity.read right.', 'The Keycloak service account has the view-users and view-realm roles.'],
    features: ['OoBDev.Keycloak (`KeycloakDirectoryReader`)', 'OoBDev.AspNetCore.Abstractions (`IRoleRightMapper`)'],
    why: 'Answer "why can this person do that" without opening the Keycloak console; the screen is read-only.',
    problems: [
      { symptom: 'The screen shows 403', cause: 'Your token has no role that maps to identity.read', fix: 'Ask for a role that is translated to identity.read' },
      { symptom: 'The screen shows 500 or 401 from Keycloak', cause: 'The service account is missing or lacks view-users', fix: 'Give the client the realm-management view-users and view-realm roles' },
    ],
    steps: [
      {
        title: 'Read the user table',
        narrative: 'Each user shows their realm roles and the rights the role translation grants.',
        expect: ['The user ann is listed with the editor role and the docs.read right.'],
        run: async page => {
          await expect(page.locator('.identity-users')).toContainText('ann');
          await expect(page.locator('.identity-users')).toContainText('docs.read');
        },
      },
      {
        title: 'Read the role table',
        narrative: 'The role table shows what each role grants, including roles that grant nothing.',
        expect: ['The viewer role is listed with no rights.'],
        run: async page => {
          await expect(page.locator('.identity-roles')).toContainText('viewer');
          await expect(page.locator('.identity-roles')).toContainText('none');
        },
      },
    ],
  },
  {
    id: 'server-driven-ui',
    title: 'Render a panel from a server-supplied definition',
    persona: 'A developer who wants one UI contract for every front end',
    goal: 'Open a UI definition and watch the host decide which controls show, enable and validate.',
    start: '/utilities/server-ui',
    prerequisites: ['The example API is running.'],
    features: ['OoBDev.UiDefinitions (`UiDefinition`, `ValueValidator`, `FormDefinitionGenerator`)'],
    why: 'See the framework UI contract rendered by a client that has no knowledge of the form.',
    problems: [
      { symptom: 'The Voltage field is disabled', cause: 'Its enabled-when condition is not met', fix: 'Switch the output on' },
      { symptom: 'A red message appears under a field', cause: 'The value broke the control constraint', fix: 'Enter a value in the stated range' },
    ],
    steps: [
      {
        title: 'Open the definition',
        narrative: 'Pick the bench supply. The page fetches the definition and asks the host for each control state.',
        expect: ['The Output section shows with Voltage disabled and its reason.'],
        run: async page => {
          await press(page, 'Bench power supply');
          await expect(page.locator('.ui-reason')).toContainText('Switch the output on first.');
        },
      },
      {
        title: 'Enable the output and enter a bad value',
        narrative: 'Switching the output on enables Voltage. A value above 30 is rejected by the host.',
        expect: ['Voltage accepts input and shows a range error.'],
        run: async page => {
          await page.getByLabel('Output enabled').check();
          await page.getByLabel('Voltage', { exact: true }).fill('42');
          await expect(page.locator('.ui-error')).toContainText('0 to 30');
        },
      },
      {
        title: 'Switch mode to reveal a section',
        narrative: 'Choosing Constant current makes the Current limit section visible.',
        expect: ['The Current limit section appears with a Reset limit button.'],
        run: async page => {
          await page.getByLabel('Constant current', { exact: true }).check();
          await expect(page.locator('[data-control="reset"]')).toBeVisible();
        },
      },
    ],
  },
];
