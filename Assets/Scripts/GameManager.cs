using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("UI — assigner dans l'Inspector")]
    public TMP_Text scoreText;      // TextMeshPro affichant le score
    public TMP_Text gameOverText;   // TextMeshPro affiché au Game Over (désactivé au départ)

    [Header("Paramètres")]
    public int startScore = 20;
    public float restartDelay = 3f;

    private int score;
    private bool isGameOver;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        score = startScore;
        isGameOver = false;

        if (gameOverText != null)
            gameOverText.gameObject.SetActive(false);

        RefreshScoreUI();
        Debug.Log($"[Score] Démarrage : {score}");
    }

    // ─── Score ────────────────────────────────────────────────────────────────

    public void AddScore(int points)
    {
        if (isGameOver) return;

        score += points;
        Debug.Log($"[Score] {(points >= 0 ? "+" : "")}{points}  →  total : {score}");

        if (score <= 0)
        {
            score = 0;
            RefreshScoreUI();
            GameOver();
            return;
        }

        RefreshScoreUI();
    }

    void RefreshScoreUI()
    {
        if (scoreText != null)
            scoreText.text = "Score : " + score;
    }

    // ─── Game Over ────────────────────────────────────────────────────────────

    public void GameOver()
    {
        if (isGameOver) return;
        isGameOver = true;

        Debug.Log("[Score] GAME OVER");
        Time.timeScale = 0f;

        if (gameOverText != null)
            gameOverText.gameObject.SetActive(true);

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) player.SetActive(false);

        StartCoroutine(RestartAfterDelay());
    }

    IEnumerator RestartAfterDelay()
    {
        yield return new WaitForSecondsRealtime(restartDelay);
        Time.timeScale = 1f;
        isGameOver = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
