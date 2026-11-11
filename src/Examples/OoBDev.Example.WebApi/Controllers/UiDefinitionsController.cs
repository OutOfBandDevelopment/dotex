using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OoBDev.UiDefinitions;
using OoBDev.WebApi.Models;
using OoBDev.WebApi.Services;
using System.Collections.Generic;

namespace OoBDev.WebApi.Controllers;

/// <summary>Serves UI definitions so a client can render a form it was not built for, and evaluates their conditions and constraints.</summary>
/// <param name="catalog">The sample definitions.</param>
/// <param name="evaluator">Evaluates conditions and validation.</param>
[Route("api/[controller]")]
[ApiController]
[AllowAnonymous]
public class UiDefinitionsController(UiDefinitionCatalog catalog, UiDefinitionEvaluator evaluator) : ControllerBase
{
    /// <summary>Lists the definitions the host serves.</summary>
    /// <returns>The definitions.</returns>
    [HttpGet]
    public ActionResult<IReadOnlyList<UiDefinitionInfoModel>> List() => Ok(catalog.List());

    /// <summary>Gets one definition as the framework's JSON form.</summary>
    /// <param name="key">The key from the list.</param>
    /// <returns>The definition, or 404.</returns>
    [HttpGet("{key}")]
    public ActionResult<UiDefinition> Get(string key) => catalog.Find(key) is { } definition ? Ok(definition) : NotFound();

    /// <summary>Evaluates the definition for the given values.</summary>
    /// <param name="key">The key from the list.</param>
    /// <param name="request">The values entered so far.</param>
    /// <returns>One state per control, or 404.</returns>
    [HttpPost("{key}/evaluate")]
    public ActionResult<IReadOnlyList<UiControlStateModel>> Evaluate(string key, [FromBody] UiEvaluateRequestModel request) =>
        catalog.Find(key) is { } definition ? Ok(evaluator.Evaluate(definition, request.Values)) : NotFound();
}
