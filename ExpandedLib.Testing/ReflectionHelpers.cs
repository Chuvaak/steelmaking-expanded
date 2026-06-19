using System;
using System.Reflection;

namespace ExpandedLib.Testing;

/// <summary>
/// Small reflection shims for poking at production members that are not publicly settable but
/// must be primed for a headless test (e.g. the network manager's server-world back-reference,
/// normally assigned only inside <c>StartServerSide</c>).
/// </summary>
public static class ReflectionHelpers
{
  /// <summary>Sets a property's value through its (possibly non-public) setter.</summary>
  public static void SetProperty(
    object target,
    string propertyName,
    object? value
  )
  {
    PropertyInfo prop =
      target
        .GetType()
        .GetProperty(
          propertyName,
          BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance
        )
      ?? throw new InvalidOperationException(
        $"Property '{propertyName}' not found on {target.GetType().Name}."
      );

    MethodInfo setter =
      prop.GetSetMethod(nonPublic: true)
      ?? throw new InvalidOperationException(
        $"Property '{propertyName}' on {target.GetType().Name} has no setter."
      );

    setter.Invoke(target, [value]);
  }
}
