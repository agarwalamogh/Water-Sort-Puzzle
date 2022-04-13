

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public static class LinqExtensions
{
    public static void AddOrUpdate<T, TJ>(this IDictionary<T, TJ> dict, T key, TJ val)
    {
        if (dict.ContainsKey(key))
        {
            dict[key] = val;
        }
        else
        {
            dict.Add(key, val);
        }
    }

    public static TJ GetOrDefault<T, TJ>(this IDictionary<T, TJ> dict, T key) =>
        dict.ContainsKey(key) ? dict[key] : default;

    public static T GetRandom<T>(this IEnumerable<T> enumerable,out int index)
    {
        var list = enumerable.ToList();
        index = Random.Range(0, list.Count);
        return list[index];
    }

    public static T GetRandomWithReduceFactor<T>(this IEnumerable<T> enumerable,float factor)
    {
        var list = enumerable.ToList();
    
        var probabilityList = new List<float>();

        var currentProbability = 1f;
        probabilityList.Add(1f);
        for (var i = 1; i < list.Count; i++)
        {
            currentProbability = currentProbability*factor;
            probabilityList.Add(currentProbability);
        }

        var p = Random.Range(0f, probabilityList.Sum());

        for (var i = 0; i < list.Count; i++)
        {
            p -= probabilityList[i];
            if (p <= 0)
            {
//                Debug.Log(i);
                return list[i];
            }

        }

        return list.GetRandom();
    }

    

    public static T GetRandom<T>(this IEnumerable<T> enumerable) => 
        enumerable.GetRandom(out var index);

    public static IEnumerable<T> GetRandom<T>(this IEnumerable<T> enumerable, int count)
    {
        var list = enumerable.ToList();

        if (list.Count<count)
        {
            throw new InvalidOperationException();
        }

        for (var i = 0; i < count; i++)
        {
            var index = Random.Range(0,list.Count);
            yield return list[index];
            list.RemoveAt(index);
        }
    }


    public static T GetRandomOrDefault<T>(this IEnumerable<T> enumerable)
    {
        var list = enumerable.ToList();

        if (list.Count == 0)
            return default;

        return list.GetRandom();
    }

    public static T GetAndRemove<T>(this IList<T> list, T item)
    {
       return list.GetAndRemove(list.IndexOf(item));
    }

    public static T GetAndRemove<T>(this IList<T> list, int index)
    {
        if (index < 0 || index>=list.Count)
            return default;
        var item = list[index];
        list.RemoveAt(index);
        return item;
    }

    public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> action)
    {
        foreach (var item in enumerable)
        {
            action?.Invoke(item);
        }
    }
}