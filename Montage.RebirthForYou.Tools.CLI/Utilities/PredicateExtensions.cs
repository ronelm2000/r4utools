using JasperFx.CodeGeneration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Montage.RebirthForYou.Tools.CLI.Utilities
{
    public static class PredicateExtensions
    {
        public static Predicate<T> And<T>(this Predicate<T> predicate, Predicate<T>? otherPredicate)
            => t => predicate(t) && (otherPredicate?.Invoke(t) ?? true);

        public static Predicate<T> Or<T>(this Predicate<T> predicate, Predicate<T>? otherPredicate)
            => t => predicate(t) || (otherPredicate?.Invoke(t) ?? false);
    }
}
