using System;
using System.Collections.Generic;

public static class ListExtensions {
	public static IList<T> Trim<T>(this IList<T> list, Func<T, bool> predicate) {
		var last = list.Count - 1;
		while (last >= 0 && predicate(list[last])) list.RemoveAt(last--);
		return (list);
	}
}
