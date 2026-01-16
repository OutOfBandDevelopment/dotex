using OoBDev.ComplexEvents.Contracts;
using OoBDev.ComplexEvents.Contracts.Services;
using OoBDev.Extensions;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;

namespace OoBDev.ComplexEvents.Common.Resolvers
{
    public class ComplexEventHandlerResolver : IComplexEventHandlerResolver
    {
        private readonly ILogger<ComplexEventHandlerResolver> _log;

        public ComplexEventHandlerResolver(
            ILogger<ComplexEventHandlerResolver> log
            )
        {
            _log = log;
        }

        public bool CheckTarget(string? target, Type handler)
        {
            if (handler == null)
            {
                _log.LogWarning($"No handler provided. Nothing to do.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(target))
            {
                _log.LogWarning($"No target provided.  Handler \"{handler}\" will not be executed");
                return false;
            }

            var attributes = handler.GetAttributes<ComplexEventHandlerAttribute>();

            if (!attributes.Any() || attributes.Any(t => t.TargetType == null))
            {
                //It no attribute on handler then it should be executed
                //It an attribute on the handler does not have a target type it should be executed

                _log.LogInformation($"Handler \"{handler}\" has no target type defined so it will be executed.");
                return true;
            }

            //match input string to type on handler attribute

            var possibleMatches =
                from attribute in attributes
                where attribute.TargetType != null
                from value in new[] {
                    attribute.TargetType?.FullName,
                    $"clr/object+{attribute.TargetType?.FullName}",
                }
                where string.Equals(target, value, StringComparison.InvariantCultureIgnoreCase)
                select value;

            if (possibleMatches.Any())
            {
                //Target type matched selector type

                _log.LogInformation($"Handler \"{handler}\" matches TargetType for \"{target}\" so it will be executed");
                return true;
            }

            _log.LogDebug($"Handler \"{handler}\" does not match \"{target}\" so it will not be executed");
            return false;
        }
    }
}
