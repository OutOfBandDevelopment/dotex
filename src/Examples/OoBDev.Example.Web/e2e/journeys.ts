import { expect, type Page } from '@playwright/test';
import type { Journey } from './journey';

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
          await expect(page.locator('pre.result')).toContainText('embedding turns text into a list of numbers');
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
    steps: [
      {
        title: 'Ask the first question',
        narrative: 'Keep context between turns is on by default, so the model returns a context that the next request sends back.',
        expect: ['The model answer appears as a turn in the conversation.'],
        run: async page => {
          await page.getByLabel('Prompt').fill('Hello');
          await press(page, 'Send');
          await expect(page.locator('.turns')).toContainText('Hello! How can I help you today?');
        },
      },
      {
        title: 'Ask a follow-up',
        narrative: 'The second request carries the context from the first, and the counter shows how much context is held.',
        expect: ['Both turns are listed.', 'The context token counter is shown.'],
        run: async page => {
          await page.getByLabel('Prompt').fill('Explain photosynthesis');
          await press(page, 'Send');
          await expect(page.locator('.turns')).toContainText('Photosynthesis');
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
    id: 'render-a-template',
    title: 'Render a text template',
    persona: 'A developer producing documents or email bodies from data',
    goal: 'Fill a template with JSON data and read the result.',
    start: '/content/templates',
    prerequisites: [],
    features: ['OoBDev.Text.Templating'],
    steps: [
      {
        title: 'See the supported template types',
        narrative: 'On load the screen asks the API which template formats it supports.',
        expect: ['The supported types list is shown.'],
        run: async page => {
          await expect(page.locator('pre.result').last()).toContainText('text/html');
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
        expect: ['The result shows status accepted.'],
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
        expect: ['The result shows status queued.', 'The event reference link points to the "all" AsyncAPI document.'],
        run: async page => {
          await press(page, 'Enqueue');
          await expect(page.locator('pre.result')).toContainText('queued');
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
];
