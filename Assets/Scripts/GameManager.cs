using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{

    public static GameManager Instance { get; private set; }
    public Highscores highscores { get; private set; }
    public int gamePhase { get; private set; }
    public bool gameOver { get; private set; }
    public AudioSource audioSource { get; private set; }

    private float secondsCount;
    private int minuteCount;
    private int hourCount;
    private int _score;

    private bool doubleScore = false;
    private float doubleScoreTimer = 0f;
    public bool immortality  { get; private set; }
    private float immortalityTimer = 0f;

    public ObstaclesMovementSpeed OMS;

    [Header("UI")]
    [SerializeField] private Text timerText;
    [SerializeField] private Text scoreText;
    [SerializeField] private GameObject gameoverMenu;
    [SerializeField] private GameObject gameUI;
    [Header("Power-ups")]
    [SerializeField] private Text doubleScoreText;
    [SerializeField] private Image immortalityImage;
    [Header("Sound Effects")]
    [SerializeField] private AudioClip eatSound;
    [SerializeField] private AudioClip explosionSound;
    [SerializeField] private AudioClip inedibleSound;

    private PlayerMovement playerMovementScript;
  
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            if (Instance != this)
            {
                Destroy(gameObject);
            }
        }
        gamePhase = 1;
    }

    // Start is called before the first frame update
    private void Start()
    {
        immortality = false;

        playerMovementScript = GameObject.Find("Player").GetComponent<PlayerMovement>();
        audioSource = GetComponent<AudioSource>();
        highscores = GetComponent<Highscores>();
        _score = 0;
        gameOver = false;
        doubleScoreText.gameObject.SetActive(false);
        immortalityImage.gameObject.SetActive(false);
        OMS.speed = 35;
    }

    private void Update()
    {
        UpdateTimerUI();
        CheckActivatedPowerups();

        if (SpawnManager.Instance?.wavesSpawned == 5)
        {
            OMS.speed += 5;
            gamePhase++;
            SpawnManager.Instance.ClearWavesCounting();
        }
    }

    private void CheckActivatedPowerups()
    {
        if(doubleScore)
        {
            if (doubleScoreTimer > 0)
            {
                doubleScoreTimer -= Time.deltaTime;
            }
            else
            {
                doubleScoreText.gameObject.SetActive(false);
                doubleScore = false;
            }
        }

        if (immortality)
        {
            if (immortalityTimer > 0)
            {
                immortalityTimer -= Time.deltaTime;
            }
            else
            {
                immortalityImage.gameObject.SetActive(false);
                immortality = false;
            }
        }
    }

    private void UpdateTimerUI()
    {
        if (playerMovementScript.isAlive)
        {
            //set timer UI
            secondsCount += Time.deltaTime;
            timerText.text = hourCount + "h:" + minuteCount + "m:" + (int)secondsCount + "s";
            if (secondsCount >= 60)
            {
                minuteCount++;
                secondsCount = 0;
            }
            else if (minuteCount >= 60)
            {
                hourCount++;
                minuteCount = 0;
            }
        }

    }

    private void UpdateScore()
    {
        scoreText.text = _score.ToString();
    }

    public void GameOver()
    {
        if (immortality) return;

        playerMovementScript.Death();
        SpawnManager.Instance.StopAllCoroutines();

        gameOver = true;
        gameUI.SetActive(false);
        gameoverMenu.SetActive(true);
    }

    public void AddScore(int scoreToAdd)
    {
        _score += doubleScore ? scoreToAdd*2 : scoreToAdd;

        UpdateScore();
    }

    public int GetCurrentScore()
    {
        return _score + (int)secondsCount + minuteCount * 60 + hourCount * 3600;
    }

    public void DestroyObstacle(GameObject obstacle)
    {
        GameObject.Destroy(obstacle, 5f);
    }

    #region Gameplay methods

    public void PickupEdible(int score)
    {
        playerMovementScript.playerAnimation.SetTrigger("Eat");

        AddScore(score);

        if(score > 0) audioSource.PlayOneShot(eatSound, 1f);
        else audioSource.PlayOneShot(inedibleSound, 0.5f);
    }

    public void PlayExplosionSound()
    {
        audioSource.PlayOneShot(explosionSound, 0.5f);
    }

    public void DoubleScorePowerupActivate()
    {
        if (!doubleScore)
        {
            doubleScore = true;
            doubleScoreText.gameObject.SetActive(true);
            doubleScoreTimer = 20f;
            StartCoroutine("UIDoubleScoreAnimation");
        }
        else doubleScoreTimer = 20f;
    }
   
    public void ImmortalityPowerupActivate()
    {
        if (!immortality)
        {
            immortality = true;
            immortalityImage.gameObject.SetActive(true);
            immortalityTimer = 10f;
            StartCoroutine("UIImmortalityAnimation");
        }
        else immortalityTimer = 10f;
    }
    
    private IEnumerator UIImmortalityAnimation()
    {
        bool fadeIn = true;

        float targetAlpha = 1f;

        var savedColor = immortalityImage.color;

        back:

        yield return new WaitUntil(() => immortalityTimer < 3);

        while (immortality)
        {
            if(immortalityTimer > 3)
            {
                immortalityImage.color = savedColor;
                goto back;
            }

            var curColor = immortalityImage.color;

            if (fadeIn)
            {
                if (Mathf.Abs(curColor.a - targetAlpha) > 0.0001f)
                {
                    curColor.a += 0.05f;
                }
                else fadeIn = false;
            }
            else
            {
                if (Mathf.Abs(curColor.a - targetAlpha) < 1.0001f)
                {
                    curColor.a -= 0.05f;
                }
                else fadeIn = true;
            }

            immortalityImage.color = curColor;

            yield return new WaitForEndOfFrame();
        }

        immortalityImage.color = savedColor;
    }

    private IEnumerator UIDoubleScoreAnimation()
    {
        bool inc = true;

        while (doubleScore)
        {
            if (doubleScoreText.fontSize == 40) inc = true;
            else if (doubleScoreText.fontSize == 65) inc = false;

            if (inc == true) doubleScoreText.fontSize += 1;
            else doubleScoreText.fontSize -= 1;

            yield return new WaitForEndOfFrame();
        }

        doubleScoreText.fontSize = 50;
    }

    #endregion
}
