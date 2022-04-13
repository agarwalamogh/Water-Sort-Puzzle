using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LevelTileUI : MonoBehaviour,IPointerClickHandler
{
    public event Action<LevelTileUI> Clicked; 

    [SerializeField] private Text _txt;
    [SerializeField] private GameObject _completeMark;
    [SerializeField] private GameObject _lockMark;
    [SerializeField] private Image _fillImg;

    private ViewModel _mViewModel;


    public ViewModel MViewModel
    {
        get => _mViewModel;
        set
        {
            _txt.text = value.Level.no.ToString();
            _fillImg.color = _fillImg.color.WithAlpha(value.Locked ? 0 : 1);
            _completeMark.SetActive(value.Completed);
            _lockMark.SetActive(value.Locked);
            _mViewModel = value;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public void OnPointerClick(PointerEventData eventData)
    {
        Clicked?.Invoke(this);
    }


    public struct ViewModel
    {
        public Level Level { get; set; }
        public bool Locked { get; set; }
        public bool Completed { get; set; }
    }
}