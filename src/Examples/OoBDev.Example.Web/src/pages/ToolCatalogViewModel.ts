import { computed, ref } from 'vue';

export interface CliTool { command: string; project: string; summary: string; install: string; }

const tool = (command: string, project: string, summary: string, packable = true): CliTool => ({
  command, project, summary, install: packable ? `dotnet tool install --global ${project}` : `dotnet run --project src/Tools/${project}`,
});

/** The command-line tools of the repository. The browser never runs them; each has a readme under src/Tools. */
export const cliTools: CliTool[] = [
  tool('dacpactools', 'OoBDev.DacPacCompiler.Cli', 'Builds a SQL Server DacPac from a SQL CLR assembly, or merges several DacPacs into one.'),
  tool('file-convert', 'OoBDev.DocumentConverter.Cli', 'Converts one file to another format with the framework document handlers (for example HTML to PDF).'),
  tool('file-rag', 'OoBDev.FileRagEngine.Cli', 'Runs a prompt template over the files of a folder tree and saves the model answers in a mirrored tree.'),
  tool('fix-sourcelink', 'OoBDev.FixSourceLinks.Cli', 'Rewrites source-link URLs in XML reports (for example Cobertura) back to local file paths.'),
  tool('image-mask', 'OoBDev.ImageMask.Cli', 'Paints redaction blocks over listed zones and compares images with volatile regions masked.'),
  tool('llmcodegen', 'OoBDev.LlmCodeGen.Cli', 'Walks a folder tree, renders a prompt per folder, asks a language model and saves the files in the answer.'),
  tool('oobdev-packman', 'OoBDev.PackMan.Cli', 'Keeps Directory.Packages.props current and aligns PackageReference versions.'),
  tool('templateengine', 'OoBDev.TemplateEngine.Cli', 'Transforms files into text documents with Handlebars or XSLT templates.'),
  tool('migrationhelper', 'OoBDev.MigrationHelper.Cli', 'Swaps a name prefix in folder names, file names and text of a copied code tree (not packaged).', false),
];

export class ToolCatalogViewModel {
  readonly filter = ref('');
  readonly tools = computed(() => {
    const text = this.filter.value.trim().toLowerCase();
    return text === '' ? cliTools : cliTools.filter(t => `${t.command} ${t.project} ${t.summary}`.toLowerCase().includes(text));
  });
}
