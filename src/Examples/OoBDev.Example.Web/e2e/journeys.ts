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
        expect: ['Both turns are listed.', 'The context token counter is shown.'],
        run: async page => {
          await page.getByLabel('Prompt').fill('Explain photosynthesis');
          await press(page, 'Send');
          await expect(page.locator('.turns p')).toHaveCount(2);
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
    id: 'ask-your-own-documents',
    title: 'Ask a question about your own text',
    persona: 'A developer learning retrieval augmented generation',
    goal: 'Give the model a few passages, ask a question, and see which passages the answer came from.',
    start: '/ai/rag',
    prerequisites: ['The Ollama container is running with the phi3 model.'],
    features: ['OoBDev.Onnx.SentenceEmbeddings', 'OoBDev.Ollama', 'OoBDev.AI.Abstractions'],
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
    id: 'sign-in',
    title: 'Sign in with Keycloak',
    persona: 'A user opening the example for the first time',
    goal: 'Sign in through the identity provider and land on the screen that was asked for.',
    start: '/identity/me',
    liveOnly: true,
    prerequisites: ['The compose stack is running with sign-in on (the default) and the `oobdev-example` realm imported.', 'Use the sample user `ada` (password `ada`) or `grace` (password `grace`, also an admin).'],
    features: ['OoBDev.Identity', 'OoBDev.AspNetCore.JwtAuthentication', 'Keycloak (authorization code with PKCE)'],
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
