using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public enum Character {
    Elina,
    Cecilia,
    Sophia,
    Coco,
    Player,
    Narration,
    Principal,
    Professor,
    Student,
    Silhouette1,
    Silhouette2
}

public enum BG {
    Hall,
    Black,
    Bedroom,
    ClassroomTesting,
    Classroom,
    CampusOverview,
    Forest,
    FrontGate,
    Hallway,
    AlchemyClassroom,
    Laboratory,
    Library,
    Schoolyard,
    DarkSchoolyard,
    BanquetHall,
    BreakRoom
}

public enum NPC {
    Left,
    Middle,
    Right,
    Absence
}

public enum EMOTION {
    basic,
    happy,
    sad,
    surprised,
    serious,
    jealous
}

public class Chat : MonoBehaviour
{
    [SerializeField] public Image emptyImage;
    [SerializeField] public Image left;
    [SerializeField] public Image right;
    [SerializeField] private Image main;
    
    [SerializeField] private Sprite[] characters;
    [SerializeField] private Sprite[] backgrounds;
    [SerializeField] private Image fadeImage;
    [SerializeField] private float fadeDuration = 1.5f;

    private Dictionary<string, Sprite> backgroundSprites;

    public TMPro.TMP_Text ChatText;
    public TMPro.TMP_Text CharacterName;

    private string[] names = {"엘리나", "세실리아", "소피아", "코코", "교장", "교수", "학생", "그림자1", "그림자2"};

    public string writerText = "";

    private JsonData dialogueRoot;
    private GameObject namebox;

    private bool isAutoMode = false;
    public float autoChatDelay = 3f;

    private bool isSkip = false;
    private bool isTyping = false;
    private bool canInput = true;
    public Button AutoModeButton;
    private ColorBlock originalColors;
    public static string currentID;
    bool isSame = false;
    public static string playerName;
    private int currentChapter = 1;
    private int totalChapters = 5;

    void Start()
    {
        namebox = GameObject.Find("Name");
        LoadChapter(1);
        originalColors = AutoModeButton.colors;
    }

    void Update() 
    {
        if (isTyping && canInput && !isAutoMode)
        {
            if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Z))
            {  
                isSkip = true;
                canInput = false;
            }
        }
    }

    void InitializeBackgroundSprites(SceneData scene)
    {
        switch (scene.background)
        {
            case BG.Hall:
                main.sprite = backgrounds[0];
                break;
            case BG.Black:
                main.sprite = backgrounds[1];
                break;
            case BG.Bedroom:
                main.sprite = backgrounds[2];
                break;
            case BG.ClassroomTesting:
                main.sprite = backgrounds[3];
                break;
            case BG.Classroom:
                main.sprite = backgrounds[4];
                break;
            case BG.CampusOverview:
                main.sprite = backgrounds[5];
                break;
            case BG.Forest:
                main.sprite = backgrounds[6];
                break;
            case BG.FrontGate:
                main.sprite = backgrounds[7];
                break;
            case BG.Hallway:
                main.sprite = backgrounds[8];
                break;
            case BG.AlchemyClassroom:
                main.sprite = backgrounds[9];
                break;
            case BG.Laboratory:
                main.sprite = backgrounds[10];
                break;
            case BG.Library:
                main.sprite = backgrounds[11];
                break;
            case BG.Schoolyard:
                main.sprite = backgrounds[12];
                break;
            case BG.DarkSchoolyard:
                main.sprite = backgrounds[13];
                break;
            case BG.BanquetHall:
                main.sprite = backgrounds[14];
                break;
            case BG.BreakRoom:
                main.sprite = backgrounds[15];
                break;
        }
    }

    void LoadDialogueData(string fileName)
    {
        string path = Path.Combine(Application.streamingAssetsPath, fileName);
        if (File.Exists(path))
        {
            string jsonText = File.ReadAllText(path);
            dialogueRoot = JsonUtility.FromJson<JsonData>(jsonText);
            Debug.Log("Dialogue data loaded successfully.");
        }
        else
        {
            Debug.LogError($"File not found at path: {path}");
        }
    }

    IEnumerator FadeIn()
    {
        float elapsedTime = 0f;
        Color color = fadeImage.color;
        color.a = 0f;
        fadeImage.color = color;
        fadeImage.gameObject.SetActive(true);

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            color.a = Mathf.Clamp01(elapsedTime / fadeDuration);
            fadeImage.color = color;
            yield return null;
        }
    }

    IEnumerator FadeOut()
    {
        float elapsedTime = fadeDuration;
        Color color = fadeImage.color;
        color.a = 1f;
        fadeImage.color = color;

        while (elapsedTime > 0)
        {
            elapsedTime -= Time.deltaTime;
            color.a = Mathf.Clamp01(elapsedTime / fadeDuration);
            fadeImage.color = color;
            yield return null;
        }

        fadeImage.gameObject.SetActive(false);
    }

    public void LoadChapter(int chapterNumber)
    {
        StartCoroutine(TransitionToChapter(chapterNumber));
    }

    IEnumerator TransitionToChapter(int chapterNumber)
    {
        if (chapterNumber != 1 && SaveLoad.LoadID == "d1")
        {
            yield return StartCoroutine(FadeIn());
            yield return new WaitForSeconds(0.5f);
        }
        string fileName = $"chapter{chapterNumber}.json";
        LoadDialogueData(fileName);
        ApplyPlayerName();
        StartCoroutine(PlayDialogues());

        yield return StartCoroutine(FadeOut()); 
    }

    void ApplyPlayerName()
    {
        foreach (var scene in dialogueRoot.scenes)
        {
            foreach (var dialogue in scene.dialogueDatas)
            {
                if (dialogue.text.Contains("Player"))
                {
                    dialogue.text = dialogue.text.Replace("Player", playerName);
                }
            }
        }
    }

    IEnumerator PlayDialogues()
    {
        foreach (var scene in dialogueRoot.scenes)
        {
            InitializeBackgroundSprites(scene);
    
            foreach (DialogueData data in scene.dialogueDatas)
            {
                if (!this.enabled) 
                {
                    yield return new WaitUntil(() => this.enabled);
                }
                currentID = data.dialogueID;
                if (data.dialogueID == SaveLoad.LoadID)
                    isSame = true;
                if (isSame)
                    yield return StartCoroutine(NormalChat(data));
            }
        }

        if (currentChapter < totalChapters)
        {
            currentChapter++;
            LoadChapter(currentChapter);
        }
    }

    void SpriteSetting(DialogueData data, int num)
    {
        switch (data.position)
        {
            case NPC.Left:
                emptyImage.sprite = characters[0];
                left.sprite = characters[num];
                break;
            case NPC.Middle:
                left.sprite = characters[0];
                emptyImage.sprite = characters[num];
                right.sprite = characters[0];
                break;
            case NPC.Right:
                emptyImage.sprite = characters[0];
                right.sprite = characters[num];
                break;
            case NPC.Absence:
                left.sprite = characters[0];
                emptyImage.sprite = characters[0];
                right.sprite = characters[0];
                break;
        }
    }

    IEnumerator NormalChat(DialogueData data)
    {
        if (data.character == Character.Narration) {
            namebox.SetActive(false);
        } 
        else {
            namebox.SetActive(true);
        }

        switch (data.character)
        {
            case Character.Elina:
                CharacterName.text = names[0];
                switch (data.emotion)
                {
                    case EMOTION.basic:
                        SpriteSetting(data, 1);
                        break;
                    case EMOTION.happy:
                        SpriteSetting(data, 5);
                        break;
                    case EMOTION.sad:
                        SpriteSetting(data, 6);
                        break;
                    case EMOTION.serious:
                        SpriteSetting(data, 7);
                        break;
                }
                break;
            case Character.Cecilia:
                CharacterName.text = names[1];
                switch (data.emotion)
                {
                    case EMOTION.basic:
                        SpriteSetting(data, 2);
                        break;
                    case EMOTION.happy:
                        SpriteSetting(data, 8);
                        break;
                    case EMOTION.surprised:
                        SpriteSetting(data, 9);
                        break;
                    case EMOTION.jealous:
                        SpriteSetting(data, 10);
                        break;
                }
                break;
            case Character.Sophia:
                CharacterName.text = names[2];
                switch (data.emotion)
                {
                    case EMOTION.basic:
                        SpriteSetting(data, 3);
                        break;
                    case EMOTION.happy:
                        SpriteSetting(data, 11);
                        break;
                    case EMOTION.sad:
                        SpriteSetting(data, 12);
                        break;
                    case EMOTION.surprised:
                        SpriteSetting(data, 13);
                        break;
                }
                break;
            case Character.Coco:
                CharacterName.text = names[3];
                switch (data.emotion)
                {
                    case EMOTION.basic:
                        SpriteSetting(data, 4);
                        break;
                    case EMOTION.happy:
                        SpriteSetting(data, 14);
                        break;
                    case EMOTION.surprised:
                        SpriteSetting(data, 15);
                        break;
                    case EMOTION.serious:
                        SpriteSetting(data, 16);
                        break;
                }
                break;
            case Character.Player:
                CharacterName.text = playerName;
                break;
            default:
                left.sprite = characters[0];
                emptyImage.sprite = characters[0];
                right.sprite = characters[0];
                if (data.character == Character.Principal)
                    CharacterName.text = names[4];
                else if (data.character == Character.Professor)
                    CharacterName.text = names[5];
                else if (data.character == Character.Student)
                    CharacterName.text = names[6];
                else if (data.character == Character.Silhouette1)
                    CharacterName.text = names[7];
                else
                    CharacterName.text = names[8];
                break;
        }
       
        ChatText.fontSize = data.fontSize;
        writerText = "";
        isTyping = true;
        canInput = true;

        for (int i = 0; i < data.text.Length; i++)
        {
            if (!this.enabled) 
            {
                yield return new WaitUntil(() => this.enabled);
            }

            if(isSkip)
            {
                writerText = data.text;
                ChatText.text = writerText;
                break;
            }

            writerText += data.text[i];
            ChatText.text = writerText;
            yield return new WaitForSeconds(1f/data.textSpeed);
        }

        isTyping = false;
        isSkip = false;
        if (data.text != "")
            yield return new WaitForSeconds(0.1f);
        canInput = true;

        float autoTimer = 0f;
        while (true)
        {
            if (!this.enabled) 
            {
                yield return new WaitUntil(() => this.enabled);
            }
            
            if (isAutoMode)
            {
                autoTimer += Time.deltaTime;
                if (data.text == "")
                    autoTimer = autoChatDelay;
                if (autoTimer >= autoChatDelay)
                    break;
            }
            else 
            {
                autoTimer = 0f;
                if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Z))
                {
                    break;
                }
            }
            yield return null;
        }

        if (data.dialogueID == "h103")
        {
            yield return StartCoroutine(FadeIn());
            yield return new WaitForSeconds(0.5f);
            SceneManager.LoadScene("Main");
        }
    }

    public void AutoMode()
    {   
        isAutoMode = !isAutoMode;
        Debug.Log($"Auto Mode: {isAutoMode}");

        ColorBlock colors = originalColors;
        if (isAutoMode)
        {
            colors.normalColor = originalColors.selectedColor;
        }
        else
        {
            colors.normalColor = originalColors.normalColor;
            colors.selectedColor = originalColors.normalColor;
        }
        AutoModeButton.colors = colors;
    }
}

