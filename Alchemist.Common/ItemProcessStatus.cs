using System.ComponentModel;

namespace Alchemist.Common;

[DefaultValue(Error)]
public enum ItemProcessStatus
{
    New = 0,
    Error = -1,
    AlreadyExists = 1,
    Updated = 2,
    Warning = 3
}
