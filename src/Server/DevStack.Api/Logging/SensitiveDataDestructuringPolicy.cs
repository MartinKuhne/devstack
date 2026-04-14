using Serilog.Core;
using Serilog.Events;

namespace DevStack.Api.Logging;

public class SensitiveDataDestructuringPolicy : IDestructuringPolicy
{
    public bool TryDestructure(object value, ILogEventPropertyValueFactory propertyValueFactory, out LogEventPropertyValue result)
    {
        result = propertyValueFactory.CreatePropertyValue(value, true);
        return false;
    }
}
