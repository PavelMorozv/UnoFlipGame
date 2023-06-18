using GameCore.Enums;
using System;
using System.Collections.Generic;

namespace GameCore.Classes
{
    public static class ListExtension
    {
        public static void Flush<T>(this List<T> list)
        {
            var random = new Random();
            for (int i = 0; i < list.Count; i++)
            {
                int j = random.Next(0, i + 1);
                var temp = list[i];
                list[j] = list[i];
                list[i] = temp;
            }
        }
    }
}
