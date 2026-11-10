using Microsoft.Extensions.Options;
using OoBDev.System.Text.Templating;
using OoBDev.WebApi.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OoBDev.WebApi.Services;

/// <summary>
/// Lists the template files of the configured template folder, using the same naming rule as <see cref="FileTemplateSource"/>:
/// <c>{name}{target extension}{template extension}</c>, for example <c>order-shipped.html.hbs</c>.
/// </summary>
public class TemplateCatalog(IOptions<FileTemplatingOptions> options, IEnumerable<IFileType> fileTypes)
{
    /// <summary>Reads one listed template; a file name that is not in the list (for example a path) is never opened.</summary>
    /// <param name="file">The file name from <see cref="List"/>.</param>
    /// <returns>The template and its text, or <c>null</c> when no listed template has that file name.</returns>
    public TemplateDetailModel? Read(string file)
    {
        var info = List().FirstOrDefault(t => string.Equals(t.File, file, StringComparison.Ordinal));
        return info == null
            ? null
            : new TemplateDetailModel { Info = info, Text = File.ReadAllText(Path.Combine(options.Value.TemplatePath, info.File)) };
    }

    /// <summary>Overwrites one listed template file; a file name that is not in the list is never written, so no file is created and no path is followed.</summary>
    /// <param name="file">The file name from <see cref="List"/>.</param>
    /// <param name="text">The new template text.</param>
    /// <returns><c>true</c> when the file was written; <c>false</c> when no listed template has that file name.</returns>
    public bool Save(string file, string text)
    {
        var info = List().FirstOrDefault(t => string.Equals(t.File, file, StringComparison.Ordinal));
        if (info == null)
        {
            return false;
        }

        File.WriteAllText(Path.Combine(options.Value.TemplatePath, info.File), text);
        return true;
    }

    /// <summary>Lists the template files, ordered by name.</summary>
    /// <returns>The templates found; empty when the folder does not exist.</returns>
    public IReadOnlyList<TemplateInfoModel> List()
    {
        var folder = options.Value.TemplatePath;
        if (!Directory.Exists(folder))
        {
            return [];
        }

        var templateTypes = fileTypes.Where(t => t.IsTemplateType).OrderByDescending(t => t.Extension.Length).ToList();
        var targetTypes = fileTypes.Where(t => !t.IsTemplateType).OrderByDescending(t => t.Extension.Length).ToList();
        var found = new List<TemplateInfoModel>();
        foreach (var path in Directory.EnumerateFiles(folder))
        {
            var file = Path.GetFileName(path);
            var template = templateTypes.FirstOrDefault(t => file.EndsWith(t.Extension, StringComparison.OrdinalIgnoreCase));
            if (template == null)
            {
                continue;
            }

            var stem = file[..^template.Extension.Length];
            var target = targetTypes.FirstOrDefault(t => t.Extension.Length > 0 && stem.EndsWith(t.Extension, StringComparison.OrdinalIgnoreCase));
            found.Add(new TemplateInfoModel
            {
                Name = target == null ? stem : stem[..^target.Extension.Length],
                File = file,
                TemplateContentType = template.ContentType,
                TargetExtension = target?.Extension ?? string.Empty,
            });
        }

        return found.OrderBy(t => t.Name, StringComparer.OrdinalIgnoreCase).ThenBy(t => t.File, StringComparer.OrdinalIgnoreCase).ToList();
    }
}
