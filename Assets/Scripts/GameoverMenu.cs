using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameoverMenu : MonoBehaviour
{
    [SerializeField] private TMP_Text finalScoreText;
    [SerializeField] private TMP_Text placeholder;
    [SerializeField] private TMP_Text _name;


    private int score;

    private void OnEnable()
    {
        ShowScore();
    }

    public void BackToMenu()
    {
        SceneManager.LoadScene(0);
    }

    public void ShowScore()
    {
        score = GameManager.Instance.GetCurrentScore();

        finalScoreText.text = score.ToString();
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(1);
    }

    public void AddScoreToLeaderboard()
    {
        if(_name.text.Length < 4)
        {
            _name.text = null;
            placeholder.text = "Invalid nickname";
            return;
        }

        GameManager.Instance.highscores.AddNewHighscore(_name.text, score);
    }
}
