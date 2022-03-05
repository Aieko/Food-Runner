using System.Collections;
using UnityEngine;
using TMPro;


    public class Highscores : MonoBehaviour
    {
        [SerializeField] private GameObject _score;
        [SerializeField] private GameObject _content;

        
        private bool isScoresDownloaded = false;
        const string privateCode = "LTVgbitqB0aaYMkItYNOSQL_5xAVjHTEiYrdzBV1USOg";
        const string publicCode = "61e73bb88f40bb10345f260b";
        const string webURL = "http://dreamlo.com/lb/";

        public Highscore[] highscoresList;

        public void AddNewHighscore(string userName, int score)
        {
            StartCoroutine(UploadNewHighscore(userName, score));
        }

        public void DownloadHighscores()
        {
            isScoresDownloaded = false;

            StartCoroutine("DownloadHighscoresFromDatabase");
        }

        public void ClearContent()
        {
            if (_content.GetComponentsInChildren<TMP_Text>().Length > 0)
            {
                foreach (var text in _content.GetComponentsInChildren<TMP_Text>())
                {
                    GameObject.Destroy(text.gameObject);
                }

            _content.GetComponent<RectTransform>().sizeDelta = new Vector2(0,0);
        }   
        }

        private IEnumerator UploadNewHighscore(string userName, int score)
        {
            /*List<IMultipartFormSection> wwwForm = new List<IMultipartFormSection>();
            wwwForm.Add(new MultipartFormDataSection("/add/" + UnityWebRequest.EscapeURL(userName) + "/" + score));

            UnityWebRequest www = UnityWebRequest.Post(webURL + privateCode, wwwForm);
            */
            WWW www = new WWW(webURL + privateCode + "/add/" + WWW.EscapeURL(userName) + "/" + score);

            yield return www;

            if (string.IsNullOrEmpty(www.error))
            {
                print("Upload Successful");
            }
            else print("Error uploading: " + www.error);
        }

        private IEnumerator DownloadHighscoresFromDatabase()
        {
            WWW www = new WWW(webURL + publicCode + "/pipe/");

            yield return www;

            if (string.IsNullOrEmpty(www.error))
            {
                FormatHighscores(www.text);

                yield return new WaitUntil(() => isScoresDownloaded);

                foreach (var highscore in highscoresList)
                {
                    var score = Instantiate(_score);
                    score.transform.SetParent(_content.transform);
                    _content.GetComponent<RectTransform>().sizeDelta += new Vector2(0, 50);
                    score.GetComponent<TextMeshProUGUI>().SetText(highscore.username + " : " + highscore.score);
                }
            }
            else print("Error uploading: " + www.error);
        }

        private void FormatHighscores(string textStream)
        {
            string[] entries = textStream.Split(new char[] { '\n' }, System.StringSplitOptions.RemoveEmptyEntries);

            highscoresList = new Highscore[entries.Length];

            for (int i = 0; i < entries.Length; i++)
            {
                string[] entryInfo = entries[i].Split(new char[] { '|' });
                string username = entryInfo[0];
                int score = int.Parse(entryInfo[1]);
                highscoresList[i] = new Highscore(username, score);
            }

            isScoresDownloaded = true;
        }

    }

    public struct Highscore
    {
        public string username;
        public int score;

        public Highscore(string _userName, int _score)
        {
            username = _userName;
            score = _score;
        }
    }


