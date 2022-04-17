using System;

namespace Community.Wsa.Sdk;

static class StringExtensions
{
    public static int FindIndex(
        this string stringValue,
        Func<char, bool> predicate,
        int valueStartIndex = 0
    )
    {
        for (var i = valueStartIndex; i < stringValue.Length; i++)
        {
            if (predicate(stringValue[i]))
            {
                return i;
            }
        }

        return -1;
    }
}
