using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class PopUpPanel : ShowHidable
{
    [SerializeField] private Text _titleTxt;
    [SerializeField] private Text _messageTxt;
    [SerializeField] private List<Button> _buttons = new List<Button>();
    private ViewModel _mViewModel;

    public ViewModel MViewModel
    {
        get { return _mViewModel; }
        set
        {
            if (_buttons.Count < value.Buttons.Length)
                throw new Exception("Too Many Buttons");

            _titleTxt.text = value.Title;
            _messageTxt.text = value.Message;

            for (var i = 0; i < _buttons.Count; i++)
            {
                _buttons[i].gameObject.SetActive(i < value.Buttons.Length);
                if (i < value.Buttons.Length)
                {
                    _buttons[i].onClick.RemoveAllListeners();
                    var j = i;
                    _buttons[i].onClick.AddListener(() => OnClickButton(value.Buttons[j].Callback));
                    _buttons[i].GetComponentInChildren<Text>().text = value.Buttons[i].Title;
                }
            }

            _mViewModel = value;
        }
    }

    private void OnClickButton(Action action = null)
    {
        Hide();
        action?.Invoke();
    }

    public override void Show(bool animate = true, Action completed = null)
    {
        //SharedUIManager.Blur = true;
        base.Show(animate, completed);
    }

    public override void Hide(bool animate = true, Action completed = null)
    {
        //SharedUIManager.Blur = false;
        base.Hide(animate, completed);
    }

    public void ShowAsConfirmation(string title, string message, Action<bool> callback = null)
    {
        MViewModel = new ViewModel
        {
            Title = title,
            Message = message,
            Buttons = new[]
           {

                new ViewModel.Button
                {
                    Title = "No",
                    Callback = () =>
                    {
                        callback?.Invoke(false);
                    }
                },
                new ViewModel.Button
                {
                    Title = "Yes",
                    Callback = () =>
                    {
                        callback?.Invoke(true);
                    }
                }
            }
        };
        Show();
    }

    public void ShowAsInfo(string title, string message, Action onClose = null)
    {
        MViewModel = new ViewModel
        {
            Title = title,
            Message = message,
            Buttons = new[]
           {
                new ViewModel.Button
                {
                    Title = "Ok",
                    Callback = () =>
                    {
                        onClose?.Invoke();
                    }
                },

            }
        };
        Show();
    }

    public struct ViewModel
    {
        public string Title { get; set; }
        public string Message { get; set; }
        public Button[] Buttons { get; set; }

        public struct Button
        {
            public string Title { get; set; }
            public Action Callback { get; set; }
        }
    }
}
