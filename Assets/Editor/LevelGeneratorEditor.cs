

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Windows;
using Random = UnityEngine.Random;

[CustomEditor(typeof(LevelGenerator))]
public class LevelGeneratorEditor : Editor
{
    private SerializedProperty _targetSwapRangeProperty;
    private SerializedProperty _targetGroupRangeProperty;
    private SerializedProperty _extraHolderRangeProperty;
    private SerializedProperty _targetLevelCountProperty;

    private void OnEnable()
    {
        _targetSwapRangeProperty = serializedObject.FindProperty(LevelGenerator.TARGET_SWAP_RANGE);
        _targetGroupRangeProperty = serializedObject.FindProperty(LevelGenerator.TARGET_GROUP_RANGE);
        _extraHolderRangeProperty = serializedObject.FindProperty(LevelGenerator.EXTRA_HOLDER_RANGE);
        _targetLevelCountProperty = serializedObject.FindProperty(LevelGenerator.TARGET_LEVEL_COUNT);
    }


    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (GUILayout.Button("Generate"))
        {
            var lvlCount = _targetLevelCountProperty.intValue;
            var targetGroupRange = _targetGroupRangeProperty.vector2IntValue;
            var extraHolderRange = _extraHolderRangeProperty.vector2IntValue;
            if (lvlCount <= 0 || extraHolderRange.x<=0)
                return;

            var levels = new List<Level>();

            for (var i = 0; i < lvlCount; i++)
            {
                var targetSwap = Mathf.RoundToInt(Mathf.Lerp(_targetSwapRangeProperty.vector2IntValue.x,
                    _targetSwapRangeProperty.vector2IntValue.y, (float) i / lvlCount));
                var expectGroup = Mathf.RoundToInt(Mathf.Lerp(targetGroupRange.x,
                    targetGroupRange.y, (float) i / lvlCount));
                var targetGroup = Mathf.RoundToInt(Mathf.Clamp(Random.Range(expectGroup - 1, expectGroup + 1),
                    targetGroupRange.x,
                    targetGroupRange.y));
                var targetHolder
                    = //i < lvlCount / 4f ? Mathf.Lerp(targetHolderRange.x,targetHolderRange.Lerb(0.25f),(float)i/lvlCount) : 
                    targetGroup + extraHolderRange.RandomWithIn();
//                   Mathf.RoundToInt(extraHolderRange.Lerb(1-Mathf.Clamp01(Random.Range((float) i / lvlCount - 0.7f,
//                        (float) i / lvlCount + 0.7f))));

                var generateLevel = GenerateLevel(targetHolder,targetGroup,targetSwap);
                levels.Add(new Level
                {
                    map = generateLevel.Select(items => new LevelColumn
                    {
                        values = items.ToList()
                    }).ToList(),
                    no = i+1
                });
            }


            var path = EditorUtility.SaveFilePanel("Save File As Json", "", "levels.json", ".json");

            if (path.Length > 0)
            {
                System.IO.File.WriteAllText(path,JsonUtility.ToJson(new LevelGroup
                {
                    levels = levels
                }));
            }
        }
    }

    public static IEnumerable<int[]> GenerateLevel(int holderCount, int groupCount, int swapCount)
    {
        var holders = Enumerable.Range(0,holderCount).Select(i => new List<int>()).ToList();
        var list = holders.GetRandom(groupCount).ToList();
        for (var i = 0; i < list.Count; i++)
        {
            list[i].AddRange(Enumerable.Repeat(i, 4));
        }
//        Debug.Log($"Holder Count-{holderCount} Group Count-{groupCount} Swap Count-{swapCount}");
        for (var i = 0; i < swapCount; i++)
        {
            var fromLists = holders.Where(l => l.Count > 0 &&(l.Count==1 || l.Last() == l[l.Count-2])).OrderByDescending(l =>l.Count(j => j == l.Last())).ToList();

            if(fromLists.Count==0)
                continue;

            var fromList = fromLists.GetRandomWithReduceFactor(0.1f);
            var toList = holders.Where(l => l.Count < 4 && l != fromList).OrderBy(l => l.Count).GetRandomWithReduceFactor(0.3f);

            var last = fromList.Last();
            fromList.RemoveAt(fromList.Count - 1);
            toList.Add(last);
        }

        return holders.Select(items => items.ToArray());
    }
}