using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using TMPro;
public class MainMenu : MonoBehaviour
{
    [SerializeField] private TMP_Text versionText;
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioClip musicStart;
    [SerializeField] private Animator dadAnim;
    [SerializeField] private AudioMixer audioMixer;

    private float lastEatAnimPlayTime = 0;
    private void Start()
    {
        ShowGameVersion();

        musicSource.PlayOneShot(musicStart);
        musicSource.PlayScheduled(AudioSettings.dspTime + musicStart.length);
    }

    public void SetVolume(float sliderValue)
    {
        //TODO find the way to change value on mixergroup
        audioMixer.SetFloat("MusicValue", Mathf.Log10(sliderValue * 20));
    }

    public void PlayGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void QuitGame()
    {
        Debug.Log("QUIT!");
        Application.Quit();
    }



    public void ShowGameVersion()
    {
        versionText.text = Application.version;
    }
}
