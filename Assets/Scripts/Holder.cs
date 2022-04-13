using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Holder : MonoBehaviour
{
    [SerializeField] private int _maxValue = 4;
    [SerializeField] private float _ballRadius;
 
    [SerializeField] private AudioClip _popClip,_putClip;
    [SerializeField] private Liquid _liquidPrefab;
    [SerializeField] private Transform _content;
    [SerializeField] private Transform _leftSideDeliverPoint;
    [SerializeField] private Transform _rightSideDeliverPoint;
    [SerializeField] private Vector2 _transferNearOffset;
    [SerializeField] private SpriteRenderer _liquidLine;
    [SerializeField] private AudioSource _audio;
    [SerializeField] private AudioClip _liquidTransferClip;
    
    private readonly List<Liquid> _liquids = new List<Liquid>();
    private Coroutine _moveCoroutine;
    private bool _isFront;

    public bool IsFull => Mathf.RoundToInt(_liquids.Sum(l=>l.Value))>=_maxValue;
    public Liquid TopLiquid => _liquids.LastOrDefault();
    public IEnumerable<Liquid> Liquids => _liquids;

    public int MAXValue => _maxValue;

    public float CurrentTotal => Liquids.Sum(l => l.Value);
    public bool IsPending { get;private set; }

    public bool Initialized { get; private set; }
    public Vector2 PendingPoint
    {
        get;
        private set;
    }
    
    public Vector3 OriginalPoint { get; private set; }

    public bool IsFront
    {
        get => _isFront;
        set
        {
            _isFront = value;
            foreach (var spriteRenderer in GetComponentsInChildren<SpriteRenderer>().Except(new []{_liquidLine}))
            {
                spriteRenderer.sortingLayerName = value ? "Front" : "Default";
            }
        }
    }




    public void StartPending()
    {
        if (IsPending)
            throw new InvalidOperationException();
        IsPending = true;
        IsFront = true;
        MoveTo(PendingPoint,speed:5);
        PlayClipIfCan(_popClip);
    }

    public void ClearPending()
    {
        IsPending = false;
        IsFront = false;
        MoveTo(OriginalPoint,speed:5);
        PlayClipIfCan(_putClip);
    }

    private IEnumerator MoveNearToHolderForTransfer(Holder holder)
    {
        var targetPoint = holder.transform.TransformPoint(transform.position.x > holder.transform.position.x
            ? _transferNearOffset.WithX(Mathf.Abs(_transferNearOffset.x))
            : _transferNearOffset.WithX(-Mathf.Abs(_transferNearOffset.x)));

        var speed = GetSpeedForDistance((transform.position - targetPoint).magnitude);
        StopMoveIfAlready();
        yield return MoveToEnumerator(targetPoint,Mathf.Max(speed,3));
    }

    private float GetSpeedForDistance(float distance)
    {
        return 5 / distance;
    }

    private IEnumerator ReturnToOriginalPoint()
    {
        StopMoveIfAlready();
        var speed = GetSpeedForDistance((transform.position - OriginalPoint).magnitude);
        yield return MoveToEnumerator(OriginalPoint, Mathf.Max(speed,3));
    }

    private void StopMoveIfAlready()
    {
        if (_moveCoroutine != null)
            StopCoroutine(_moveCoroutine);
    }

    public IEnumerator MoveAndTransferLiquid(Holder holder,Action onLiquidTransferComplete=null)
    {
        IsPending = false;
        
        var deliverAbsAngle = 82;
        var deliverTopPosition = holder.transform.TransformPoint(5 * Vector3.up);

        
        if(holder.IsFull || !_liquids.Any() || holder.Liquids.Any() && holder.Liquids.Last().GroupId != Liquids.Last().GroupId)
        {
            yield break;
        }

        yield return MoveNearToHolderForTransfer(holder);
        
        var isRightSide = holder.transform.position.x > transform.position.x;
        var sidePoint = isRightSide ? _rightSideDeliverPoint : _leftSideDeliverPoint;
        var deliverAngle = isRightSide ? -deliverAbsAngle : deliverAbsAngle;

        var relativePoint = transform.position - sidePoint.position;
        var rotatedRelativePoint = Quaternion.AngleAxis(deliverAngle, Vector3.forward) * relativePoint;

        var targetHolderPoint = rotatedRelativePoint + deliverTopPosition;
        var targetHolderRotation = Quaternion.AngleAxis(deliverAngle, Vector3.forward);

        var startPoint = transform.position;
        var startRotation = transform.rotation;

        var thisLiquid = _liquids.Last();

        yield return SimpleCoroutine.MoveTowardsEnumerator(onCallOnFrame: n =>
        {
            transform.position = Vector3.Lerp(startPoint, targetHolderPoint, n);
            transform.rotation = Quaternion.Lerp(startRotation, targetHolderRotation, n);
        }, speed: 2);

        var thisLiquidStartValue = thisLiquid.Value;
        var transferValue = Mathf.Min(thisLiquid.Value,holder.MAXValue - holder.CurrentTotal);
        
        
        if (holder.Liquids.LastOrDefault() == null)
        {
            holder.AddLiquid(thisLiquid.GroupId);
        }

        var targetLiquid = holder.Liquids.Last();
        var targetLiquidStartValue = targetLiquid.Value;

        _liquidLine.transform.position = sidePoint.position;
        _liquidLine.gameObject.SetActive(true);
        _liquidLine.transform.localScale =
            _liquidLine.transform.localScale.WithY(sidePoint.transform.position.y - holder.transform.position.y);
        _liquidLine.color = thisLiquid.Renderer.color;
        _liquidLine.transform.rotation = Quaternion.identity;
        _audio.clip = _liquidTransferClip;
        _audio.Play();
        _audio.volume = transferValue / 5;
        yield return SimpleCoroutine.MoveTowardsEnumerator(onCallOnFrame: n =>
        {
            thisLiquid.Value = Mathf.Lerp(thisLiquidStartValue, thisLiquidStartValue - transferValue, n);
            targetLiquid.Value = Mathf.Lerp(targetLiquidStartValue, targetLiquidStartValue + transferValue, n);
        }, speed: 2);

        if (thisLiquid.Value <= 0.05f)
        {
            _liquids.Remove(thisLiquid);
            Destroy(thisLiquid.gameObject);
        }
        else
        {
            thisLiquid.Value = Mathf.RoundToInt(thisLiquid.Value);
        }
        _audio.Stop();
        _liquidLine.gameObject.SetActive(false);
        targetLiquid.Value = Mathf.RoundToInt(targetLiquid.Value);
        onLiquidTransferComplete?.Invoke();
        yield return SimpleCoroutine.MoveTowardsEnumerator(onCallOnFrame: n =>
        {
            transform.position = Vector3.Lerp(targetHolderPoint, startPoint, n);
            transform.rotation = Quaternion.Lerp(targetHolderRotation, startRotation, n);
        }, speed: 2);

        yield return ReturnToOriginalPoint();
        IsFront = false;

    }
    

    public void AddLiquid(int groupId, float value = 0)
    {
        var topPoint = GetTopPoint();
        var liquid = Instantiate(_liquidPrefab,_content);

        liquid.IsBottomLiquid = !Liquids.Any();
        
        liquid.GroupId = groupId;
        liquid.transform.position = topPoint;
        
        
        liquid.Value = value;
        _liquids.Add(liquid);
    }

    public Vector2 GetTopPoint()
    {
        return transform.TransformPoint(Liquids.Sum(l => l.Size) * Vector2.up);
    }

    private void PlayClipIfCan(AudioClip clip,float volume=0.35f)
    {
        if(!AudioManager.IsSoundEnable || clip==null)
            return;
        AudioSource.PlayClipAtPoint(clip,Camera.main.transform.position,volume);
    }

    public void Init(IEnumerable<LiquidData> liquidDatas)
    {
        var list = liquidDatas.ToList();
        if(Initialized)
            return;
    
        list.ForEach(l=>AddLiquid(l.groupId,l.value));
        PendingPoint = transform.position + 0.5f * Vector3.up;
        OriginalPoint = transform.position ;
        Initialized = true;
    }


    public void MoveTo(Vector2 point, float speed = 1,Action onFinished=null)
    {
       StopMoveIfAlready();
        
        _moveCoroutine = StartCoroutine(SimpleCoroutine.CoroutineEnumerator(MoveToEnumerator(point, speed),onFinished));
    }
    
    private IEnumerator MoveToEnumerator(Vector2 toPoint,float speed=1)
    {
        var startPoint = transform.position;
        yield return SimpleCoroutine.MoveTowardsEnumerator(onCallOnFrame: n =>
        {
            transform.position = Vector3.Lerp(startPoint, toPoint, n);
        },speed:speed);
    }

}