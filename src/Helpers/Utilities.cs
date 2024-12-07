using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace src.Helpers;
internal static class Utilities
{
    public static string EnsureEndsWith(this string value,
        string suffix,
        StringComparison comparison = StringComparison.InvariantCulture) =>
        value.EndsWith(suffix, comparison)
            ? value
            : value + suffix;
}
