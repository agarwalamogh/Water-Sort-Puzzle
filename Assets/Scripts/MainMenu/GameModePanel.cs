
namespace MainMenu
{
    public class GameModePanel : ShowHidable
    {

        public void OnClickButton(int mode)
        {
            var levelsPanel = UIManager.Instance.LevelsPanel;
            levelsPanel.GameMode = (GameMode)mode;
            levelsPanel.Show();
        }

    }
}