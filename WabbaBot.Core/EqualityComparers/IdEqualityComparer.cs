using System.Diagnostics.CodeAnalysis;
using WabbaBot.Core.Interfaces;

namespace WabbaBot.Core.EqualityComparers {
    public class IdEqualityComparer : IEqualityComparer<IHasId> {
        public bool Equals(IHasId? x, IHasId? y) => x?.Id.Equals(y?.Id) ?? x == null && y == null;
        public int GetHashCode([DisallowNull] IHasId obj) => obj.Id.GetHashCode();
    }
}
