
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using dotmob;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }
    public static event Action LevelCompleted;

    [SerializeField] private float _minXDistanceBetweenHolders;
    [SerializeField] private Camera _camera;
    [SerializeField] private Holder _holderPrefab;

    [SerializeField] private AudioClip _winClip;

    public GameMode GameMode { get; private set; } = GameMode.Easy;
    public Level Level { get; private set; }

    private readonly List<Holder> _holders = new List<Holder>();

    private readonly Stack<MoveData> _undoStack = new Stack<MoveData>();

    public State CurrentState { get; private set; } = State.None;

    public bool HaveUndo => _undoStack.Count > 0;

    public bool IsTransfer { get; set; }

    
    private void Awake()
    {
        Instance = this;
        var loadGameData = GameManager.LoadGameData;
        GameMode = loadGameData.GameMode;
        Level = loadGameData.Level;
        LoadLevel();
        CurrentState = State.Playing;
    }

    private void LoadLevel()
    {
        var list = PositionsForHolders(Level.map.Count, out var width).ToList();
        _camera.orthographicSize = 0.5f * width * Screen.height / Screen.width;

        var levelMap = Level.LiquidDataMap;
        for (var i = 0; i < levelMap.Count; i++)
        {
            var levelColumn = levelMap[i];
            var holder = Instantiate(_holderPrefab, list[i], Quaternion.identity);
            holder.Init(levelColumn);
            _holders.Add(holder);
        }
    }

    public void OnClickUndo()
    {
       // Debug.Log("UNDO :" + _undoStack.Count);
        if (CurrentState != State.Playing || _undoStack.Count <= 0)
            return;

        var moveData = _undoStack.Pop();
    }

    private void Update()
    {
        if (CurrentState != State.Playing)
            return;

        if (Input.GetMouseButtonDown(0))
        {
            var collider = Physics2D.OverlapPoint(_camera.ScreenToWorldPoint(Input.mousePosition));
            if (collider != null)
            {
                var holder = collider.GetComponent<Holder>();

                if (holder != null)
                    OnClickHolder(holder);
            }
        }
    }

    // ReSharper disable Unity.PerformanceAnalysis
    private void OnClickHolder(Holder holder)
    {
        if(IsTransfer)
            return;

        
            
        var pendingHolder = _holders.FirstOrDefault(h => h.IsPending);

        if (pendingHolder != null && pendingHolder != holder)
        {
            if (holder.TopLiquid == null ||
                (pendingHolder.TopLiquid.GroupId == holder.TopLiquid.GroupId && !holder.IsFull ))
            {
               
              
                IsTransfer = true;
                StartCoroutine(SimpleCoroutine.CoroutineEnumerator(pendingHolder.MoveAndTransferLiquid(holder, CheckAndGameOver),()=>
                {
                    
                    IsTransfer = false;
                 
                }));
            }
            else
            {
                pendingHolder.ClearPending();
                holder.StartPending();
            }
        }
        else if (holder.Liquids.Any())
        {
            if (!holder.IsPending)
                holder.StartPending();
            else
            {
                holder.ClearPending();
            }
        }
    }



    private void CheckAndGameOver()
    {

    



        if (
            _holders.All(holder =>
        {
            var liquids = holder.Liquids.ToList();
            Debug.Log(_holders.Count);
            return liquids.Count == 0 || liquids.Count == 1;
        }) &&
        _holders.Where(holder => holder.Liquids.Any()).GroupBy(holder => holder.Liquids.First().GroupId)
            .All(holders => holders.Count() == 1))
        {
            //Debug.Log("Xong 1 chai");
            OverTheGame();
        }
    }

    private void OverTheGame()
    {
        if (CurrentState != State.Playing)
            return;

        PlayClipIfCan(_winClip);
        CurrentState = State.Over;

        ResourceManager.CompleteLevel(GameMode, Level.no);
        LevelCompleted?.Invoke();
    }

    private void PlayClipIfCan(AudioClip clip, float volume = 0.35f)
    {
        if (!AudioManager.IsSoundEnable || clip == null)
            return;
        AudioSource.PlayClipAtPoint(clip, Camera.main.transform.position, volume);
    }

    public IEnumerable<Vector2> PositionsForHolders(int count, out float expectWidth)
    {
        expectWidth = 4 * _minXDistanceBetweenHolders;
        if (count <= 6)
        {
            var minPoint = transform.position - ((count - 1) / 2f) * _minXDistanceBetweenHolders * Vector3.right -
                           Vector3.up * 1f;

            expectWidth = Mathf.Max(count * _minXDistanceBetweenHolders, expectWidth);

            return Enumerable.Range(0, count)
                .Select(i => (Vector2) minPoint + i * _minXDistanceBetweenHolders * Vector2.right);
        }

        var aspect = (float) Screen.width / Screen.height;

        var maxCountInRow = Mathf.CeilToInt(count / 2f);

        if ((maxCountInRow + 1) * _minXDistanceBetweenHolders > expectWidth)
        {
            expectWidth = (maxCountInRow + 1) * _minXDistanceBetweenHolders;
        }

        var height = expectWidth / aspect;


        var list = new List<Vector2>();
        var topRowMinPoint = transform.position + Vector3.up * height / 6f -
                             ((maxCountInRow - 1) / 2f) * _minXDistanceBetweenHolders * Vector3.right - Vector3.up * 1f;
        list.AddRange(Enumerable.Range(0, maxCountInRow)
            .Select(i => (Vector2) topRowMinPoint + i * _minXDistanceBetweenHolders * Vector2.right));

        var lowRowMinPoint = transform.position - Vector3.up * height / 6f -
                             (((count - maxCountInRow) - 1) / 2f) * _minXDistanceBetweenHolders * Vector3.right -
                             Vector3.up * 1f;
        list.AddRange(Enumerable.Range(0, count - maxCountInRow)
            .Select(i => (Vector2) lowRowMinPoint + i * _minXDistanceBetweenHolders * Vector2.right));

        return list;
    }


    public enum State
    {
        None,
        Playing,
        Over
    }

    public struct MoveData
    {
        public Holder FromHolder { get; set; }
        public Holder ToHolder { get; set; }
        public Liquid Liquid { get; set; }
    }
}

[Serializable]
public struct LevelGroup : IEnumerable<Level>
{
    public List<Level> levels;

    public IEnumerator<Level> GetEnumerator()
    {
        return levels?.GetEnumerator() ?? Enumerable.Empty<Level>().GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}

[Serializable]
public struct Level
{
    public int no;
    public List<LevelColumn> map;

    public List<IEnumerable<LiquidData>> LiquidDataMap => map.Select(GetLiquidDatas).ToList();

    public static IEnumerable<LiquidData> GetLiquidDatas(LevelColumn column)
    {
        var list = column.ToList();

        for (var j = 0; j < list.Count; j++)
        {
            var currentGroup = list[j];
            var count = 0;
            for (; j < list.Count; j++)
            {
                if (currentGroup == list[j])
                {
                    count++;
                }
                else
                {
                    j--;
                    break;
                }
            }

            yield return new LiquidData
            {
                groupId = currentGroup,
                value = count
            };
        }
    }
}

public struct LiquidData
{
    public int groupId;
    public float value;
}

[Serializable]
public struct LevelColumn : IEnumerable<int>
{
    public List<int> values;

    public IEnumerator<int> GetEnumerator()
    {
        return values?.GetEnumerator() ?? Enumerable.Empty<int>().GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}