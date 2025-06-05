using System.ComponentModel;

namespace Alchemist.Common;

[DefaultValue(Error)]
public enum ItemProcessStatus
{
    Error = 0,
    New = 1,
    AlreadyExists = 2,
    Updated = 3,
    Warning = 4
}
