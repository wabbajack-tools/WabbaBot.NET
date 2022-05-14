using System.Diagnostics.CodeAnalysis;
using WabbaBot.Interfaces;

namespace WabbaBot.EqualityComparers {
    public class IdEqualityComparer : IEqualityComparer<IHasId> {
        public bool Equals(IHasId? x, IHasId? y) => x?.Id.Equals(y?.Id) ?? x == null && y == null;
        public int GetHashCode([DisallowNull] IHasId obj) => obj.Id.GetHashCode();
    }
}
