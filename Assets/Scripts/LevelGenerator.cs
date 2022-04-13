using UnityEngine;

public partial class LevelGenerator : ScriptableObject
{
    [SerializeField] private Vector2Int _targetSwapRange;
    [SerializeField] private Vector2Int _targetGroupRange;
    [SerializeField] private Vector2Int _extraHolderRange;
    [SerializeField] private int _targetLevelCount;

#if UNITY_EDITOR
    //[UnityEditor.MenuItem("Dotmob/Level Generator")]
    public static void Open()
    {
        GamePlayEditorManager.OpenScriptableAtDefault<LevelGenerator>();
    }
#endif
}

public partial class LevelGenerator
{
    public const string TARGET_SWAP_RANGE = nameof(_targetSwapRange);
    public const string TARGET_GROUP_RANGE = nameof(_targetGroupRange);
    public const string EXTRA_HOLDER_RANGE = nameof(_extraHolderRange);
    public const string TARGET_LEVEL_COUNT = nameof(_targetLevelCount);
}