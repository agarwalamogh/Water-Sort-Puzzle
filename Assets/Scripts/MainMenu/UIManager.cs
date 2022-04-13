using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MainMenu
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [SerializeField] private LevelsPanel _levelsPanel;
        [SerializeField] private GameModePanel _gameModePanel;
       

        public GameModePanel GameModePanel => _gameModePanel;
        public LevelsPanel LevelsPanel => _levelsPanel;


        private void Awake()
        {
            Instance = this;
           
        }

    }
}