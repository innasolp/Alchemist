using System;
using System.Collections.Generic;

namespace Alchemist.Product.Data;

public partial class ComponentGroup
{
    public string Name { get; set; } = null!;

    public int? ParentGroupId { get; set; }

    public int Id { get; set; }
}
