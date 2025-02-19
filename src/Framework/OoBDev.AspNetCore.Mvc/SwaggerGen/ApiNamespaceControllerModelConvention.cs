using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace OoBDev.AspNetCore.Mvc.SwaggerGen;

/// <summary>
/// SwaggerGen extension to configure controller group as the related assembly name
/// </summary>
public class ApiNamespaceControllerModelConvention : IControllerModelConvention
{
    /// <summary>
    /// Applies the convention to the specified controller model.
    /// </summary>
    /// <param name="controller">The controller model to apply the convention to.</param>
    public void Apply(ControllerModel controller) => controller.ApiExplorer.GroupName = controller.ControllerType.Assembly.GetName().Name;
}
