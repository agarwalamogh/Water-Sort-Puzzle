

using System.Collections.Generic;
using System.Linq;
using dotmob;
using UnityEngine;

public class LevelsPanel : ShowHidable
{
    [SerializeField] private LevelTileUI _levelTileUIPrefab;
    [SerializeField] private RectTransform _content;

    public GameMode GameMode
    {
        get => _gameMode;
        set
        {
            _gameMode = value;

            var levels = ResourceManager.GetLevels(value).ToList();

            for (var i = 0; i < levels.Count; i++)
            {
                var level = levels[i];
                if (_tiles.Count<=i)
                {
                    var levelTileUI = Instantiate(_levelTileUIPrefab,_content);
                    levelTileUI.Clicked +=LevelTileUIOnClicked;
                    _tiles.Add(levelTileUI);
                }

                _tiles[i].MViewModel = new LevelTileUI.ViewModel
                {
                    Level = level,
                    Locked = ResourceManager.IsLevelLocked(value,level.no),
                    Completed = ResourceManager.GetCompletedLevel(value)>=level.no
                };
            }

        }
    }



    private readonly List<LevelTileUI> _tiles = new List<LevelTileUI>();
    private GameMode _gameMode;


    private void LevelTileUIOnClicked(LevelTileUI tileUI)
    {
        if (tileUI.MViewModel.Locked)
        {
            return;
        }

        GameManager.LoadGame(new LoadGameData
        {
            Level = tileUI.MViewModel.Level,
            GameMode = GameMode
        });
    }
}