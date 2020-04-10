using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BotSpawner : MonoBehaviour
{
    public GameObject botPrefab, bot, nameTextPrefab, nameText;
    Vector3 randPos;
    public int botNum;
    public List<Color> colorList = new List<Color>();
    public GameObject[] holeArr = new GameObject[0];
    bool once = true;
    public LayerMask obstacleMask;
    public float radius = 2f;
    public CanvasController canvasController;
    public GameController gameController;
    public Transform namesParent;
    PlayerController playerController;
    string[] names = new string[] { "Parker", "Thomas", "Robert", "Will", "Herman", "Eliza", "Hamilton", "Kendyl", "Aliya", "Hunter", "Deanna", "Selena", "Elizabeth", "Mariana", "Zoe", "Tommy" };
    public List<Transform> botList = new List<Transform>();
    void Awake()
    {
        holeArr = GameObject.FindGameObjectsWithTag("Hole");
        playerController = FindObjectOfType<PlayerController>();
    }
    void Start()
    {
        for (int i = 0; i < botNum; i++)
        {
            BotSpawn(i);
        }

    }

    void Update()
    {
        if (GameController.State == GameState.Playing)
        {
            canvasController.textHudBotNum.text = (transform.childCount + 1) + "/" + (botNum + 1);
        }

        if (GameController.State == GameState.Playing && once)
        {
            for (int i = 0; i < holeArr.Length; i++)
            {
                if (holeArr[i].GetComponent<BoxCollider>())
                {
                    holeArr[i].GetComponent<BoxCollider>().enabled = false;
                }
                else
                {
                    holeArr[i].GetComponent<MeshCollider>().enabled = false;
                }

            }

            once = false;
        }

    }

    public bool IsOverlap(Vector3 pos, bool isPlayer)
    {
        for (int i = 0; i < botList.Count; i++)
        {
            if (Vector3.Distance(botList[i].position, pos) < 3f)
            {
                return true;
            }
        }

        if (Vector3.Distance(playerController.transform.position, pos) < 7f)
        {
            return true;
        }
        return false;
    }

    Vector3 RandomPos()
    {
        randPos = new Vector3(Random.Range(-21.6f, 21.6f), 0.7f, Random.Range(-21.6f, 21.6f));
        while (Physics.CheckSphere(randPos, radius, obstacleMask) || IsOverlap(randPos, false))
        {
            randPos = new Vector3(Random.Range(-21.6f, 21.6f), 0.7f, Random.Range(-21.6f, 21.6f));
        }
        return randPos;
    }


    void BotSpawn(int i)
    {
        bot = Instantiate(botPrefab);
        bot.transform.position = RandomPos();
        bot.transform.SetParent(transform);
        bot.transform.eulerAngles = new Vector3(0, Random.Range(0, 360), 0);
        botList.Add(bot.transform);

        nameText = Instantiate(nameTextPrefab);
        nameText.transform.SetParent(namesParent);
        nameText.GetComponent<NameText>().target = bot.transform;
        nameText.GetComponent<TextMesh>().text = names[i];

        bot.GetComponent<Renderer>().material.color = colorList[Random.Range(0, colorList.Count)];
        bot.name = names[i];
        Pathfinding.AIDestinationSetter a = bot.GetComponent<Pathfinding.AIDestinationSetter>();
        a.killData = new KillData(bot.name, 0);
        gameController.killList.Add(a.killData);
    }

}
