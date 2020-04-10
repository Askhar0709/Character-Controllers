using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public Vector3 mouseStartPos;
    Vector3 desiredPos, lookPos;
    [Header("Attributes")]
    public float speed, fireRate, fireCountDown;
    public GunController gunController;
    Rigidbody rb;
    GameController gameController;
    public Vector3 centerMass;
    bool downing = false;
    Pathfinding.AIDestinationSetter setter;
    public class KillingMeObject
    {
        public GameObject killingMeObject;
        public float time;
        public KillingMeObject(GameObject killingMeObject, float time)
        {
            this.killingMeObject = killingMeObject;
            this.time = time;
        }
    };
    public List<KillingMeObject> killingMeObjects = new List<KillingMeObject>();
    float time = 0;
    public CanvasController canvasController;
    public KillData killData;
    public Vector3 scaleSize;
    int currentKill = 0;
    public float mass;
    public GameObject scaleUpText;
    public AnimationCurve scaleAnimCurve;
    public float scaleTime;
    bool once = true, die = false;
    public CameraController cameraController;
    void Awake()
    {
        gameController = FindObjectOfType<GameController>();
        rb = GetComponent<Rigidbody>();
        rb.centerOfMass = centerMass;
        cameraController.offset = cameraController.transform.position - transform.position;
        transform.position = new Vector3(0, 0.7f, Random.Range(-7.8f, -22.4f));
    }

    void Update()
    {
        if (!downing && GameController.State == GameState.Playing)
        {
            Controller();
        }

        if (GameController.State == GameState.Playing)
        {
            if (once)
            {
                killData = new KillData(name, 0);
                gameController.killList.Add(killData);
                once = false;
            }

            Fire();
            GameOverCheck();
            Kill();
            if (killData.kill > currentKill && Too.GetData<bool>(Data.killScale) && !downing)
            {
                Scale();
                TapticPlugin.TapticManager.Impact(TapticPlugin.ImpactFeedback.Heavy);
            }

            canvasController.textHudKillNum.text = "kill : " + (killData.name == name ? killData.kill : 0).ToString();
        }

        if (downing)
        {
            desiredPos = Input.mousePosition - mouseStartPos;
            lookPos = new Vector3(desiredPos.x, 0, desiredPos.y);
            transform.LookAt(lookPos);
        }

    }


    private void Controller()
    {
        if (Input.GetMouseButtonDown(0))
        {
            mouseStartPos = Input.mousePosition;
        }
        else if (Input.GetMouseButton(0))
        {

            if (Vector3.Distance(mouseStartPos, Input.mousePosition) > 5)
            {

                desiredPos = Input.mousePosition - mouseStartPos;
                lookPos = new Vector3(desiredPos.x, 0, desiredPos.y);
                transform.LookAt(lookPos);
                rb.MovePosition(
                new Vector3(transform.position.x, Mathf.Clamp(transform.position.y, 0, transform.localScale.x == 1 ? 0.71f : 0.71f + ((transform.localScale.x - 0.71f) - 0.3f) / 2), transform.position.z)
                //transform.position 
                + transform.forward * Time.deltaTime * speed);
            }
        }
    }

    void Fire()
    {
        if (fireCountDown <= 0)
        {
            gunController.Shoot();

            if (Too.GetData<bool>(Data.killedNumFireRate))
            {

                fireCountDown = 1f / (fireRate + (killData.kill * 5));
            }
            else
            {
                fireCountDown = 1f / fireRate;
            }

        }

        fireCountDown -= Time.deltaTime;
    }
    void GameOverCheck()
    {
        if (transform.position.y <= 0.45f)
        {
            downing = true;
            GetComponent<BoxCollider>().material = gameController.SlideMat;
            rb.mass = 20f;
        }

        if (transform.position.y < -5f)
        {
            gameController.GameOver();
        }
    }


    private void Kill()
    {
        if (downing && !die)
        {
            rb.constraints &= ~RigidbodyConstraints.FreezeRotation;

            float maxTime = -1;
            int idx = -1;

            for (int i = 0; i < killingMeObjects.Count; i++)
            {
                if (killingMeObjects[i].time > maxTime)
                {
                    maxTime = killingMeObjects[i].time;
                    idx = i;
                }
            }

            if (idx >= 0)
            {
                if (killingMeObjects[idx].killingMeObject != null)
                {
                    canvasController.textGameOverKilledBy.text = "Killed by " + "<color=#" + ColorUtility.ToHtmlStringRGB(killingMeObjects[idx].killingMeObject.GetComponent<Renderer>().material.color) + ">" + killingMeObjects[idx].killingMeObject.name + " </color> ";
                    killingMeObjects[idx].killingMeObject.GetComponent<Pathfinding.AIDestinationSetter>().killData.kill++;
                }
                else
                {
                    canvasController.textGameOverKilledBy.text = "Killed by " + "Suicide!";
                }
            }

            die = true;
        }
    }

    private void Scale()
    {
        currentKill = killData.kill;
        StartCoroutine(scaling(transform.localScale.x, transform.localScale.x + scaleSize.x, scaleTime));


        rb.mass = rb.mass + (mass * killData.kill);

        GameObject scaleText = Instantiate(scaleUpText);
        scaleText.transform.position = transform.position;
        scaleText.transform.SetParent(canvasController.transform);
        Destroy(scaleText, 1f);

        GameObject PS = Instantiate(canvasController.scaleParticle, transform.position, Quaternion.identity);
        PS.transform.GetChild(0).GetComponent<ParticleSystem>().startColor = GetComponent<Renderer>().material.color;
        PS.transform.GetChild(0).localScale = PS.transform.GetChild(0).localScale + (scaleSize * killData.kill);
        PS.transform.SetParent(transform);
        Destroy(PS, 0.5f);

    }
    float lerpe(float a, float b, float t)
    {
        return a + (b - a) * t;
    }
    WaitForEndOfFrame eof = new WaitForEndOfFrame();
    public IEnumerator scaling(float curScl, float nxtScl, float time)
    {
        float dt = 0;
        while (dt < time)
        {
            transform.localScale = Vector3.one * lerpe(curScl, nxtScl, scaleAnimCurve.Evaluate(dt / time));
            yield return eof;
            dt += Time.deltaTime;
        }
        transform.localScale = Vector3.one * nxtScl;
    }

    void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Bullet") && other.gameObject.GetComponent<Bullet>().target != null)
        {
            time = Time.time;
            GameObject killingMeObject = other.gameObject.GetComponent<Bullet>().target.gameObject;
            int idx = 0;
            bool isContains = false;

            for (int i = 0; i < killingMeObjects.Count; i++)
            {
                if (killingMeObjects[i].killingMeObject == killingMeObject)
                {
                    isContains = true;
                    idx = i;
                    break;
                }
            }

            if (isContains)
            {
                killingMeObjects[idx].time = time;
            }
            else
            {
                killingMeObjects.Add(new KillingMeObject(killingMeObject, time));
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Water"))
        {
            GameObject waterSplashPs = Instantiate(canvasController.waterSplashParticle, gameObject.transform.position, Quaternion.identity);
            Destroy(waterSplashPs, 1f);
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.black;
        Gizmos.DrawSphere(transform.localScale + transform.rotation * centerMass, 0.5f);
    }
}
