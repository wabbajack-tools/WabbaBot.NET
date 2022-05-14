using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

namespace WabbaBot {
    public static class Extensions {
        // https://stackoverflow.com/questions/17560201/join-liststring-together-with-commas-plus-and-for-last-element
        public static string CreateJoinedString<T>(this IEnumerable<T> values, string separator = ", ", string? lastSeparator = " and ") {
            if (values == null)
                throw new ArgumentNullException(nameof(values));
            if (separator == null)
                throw new ArgumentNullException(nameof(separator));

            var sb = new StringBuilder();
            var enumerator = values.GetEnumerator();

            if (enumerator.MoveNext())
                sb.Append(enumerator.Current);

            bool objectIsSet = false;
            object? obj = null;
            if (enumerator.MoveNext()) {
                obj = enumerator.Current!;
                objectIsSet = true;
            }

            while (enumerator.MoveNext()) {
                sb.Append(separator);
                sb.Append(obj);
                obj = enumerator.Current!;
                objectIsSet = true;
            }

            if (objectIsSet) {
                sb.Append(lastSeparator ?? separator);
                sb.Append(obj);
            }

            return sb.ToString();
        }

        public static T? RandomOrDefault<T>(this IEnumerable<T> enumerable) {
            var count = enumerable.Count();
            if (count == 0)
                return default(T);

            try {
                return enumerable.ElementAt(new Random().Next(0, count));
            }
            catch (NullReferenceException) {
                return default(T);
            }
        }
    }
}
