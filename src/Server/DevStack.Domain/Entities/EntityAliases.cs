using DevStack.Domain.Enums;

namespace DevStack.Domain.Entities;

/// <summary>
/// Legacy type alias for backward compatibility during migration.
/// Use Item with ItemSubtype.Feature instead.
/// </summary>
[Obsolete("Use Item with Subtype = ItemSubtype.Feature instead")]
public class Feature : Item
{
}

/// <summary>
/// Legacy type alias for backward compatibility during migration.
/// Use Item with ItemSubtype.Defect instead.
/// </summary>
[Obsolete("Use Item with Subtype = ItemSubtype.Defect instead")]
public class Defect : Item
{
}
