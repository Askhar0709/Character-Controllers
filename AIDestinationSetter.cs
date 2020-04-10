using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Pathfinding
{
    [UniqueComponent(tag = "ai.destination")]
    public class AIDestinationSetter : VersionedMonoBehaviour
    {
        public Vector3 target;
        public KillData killData;
        IAstarAI ai;
        public LayerMask obstacleMask;
        float radius = 2f;
        float fireRate, fireCountDown;
        public float speed;
        GunController gunController;
        CanvasController canvasController;
        public bool downing = false;
        int idx = -1;
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
        List<KillingMeObject> killingMeObjects = new List<KillingMeObject>();
        float time = 0;
        PlayerController playerController;
        Rigidbody rb;
        GameController gameController;
        CameraController cameraController;
        int currentKill = 0;
        void Awake()
        {
            rb = GetComponent<Rigidbody>();
            target = RandomPos();
            gunController = FindObjectOfType<GunController>();
            canvasController = FindObjectOfType<CanvasController>();
            playerController = FindObjectOfType<PlayerController>();
            gameController = FindObjectOfType<GameController>();
            cameraController = FindObjectOfType<CameraController>();
        }

        void Start()
        {
            speed = playerController.speed;
            fireRate = playerController.fireRate;
            fireCountDown = playerController.fireCountDown;
            rb.centerOfMass = playerController.centerMass;
        }

        void OnEnable()
        {
            ai = GetComponent<IAstarAI>();
            if (ai != null) ai.onSearchPath += Update;
        }

        void OnDisable()
        {
            if (ai != null) ai.onSearchPath -= Update;
        }

        void Update()
        {
            if (target != null && ai != null) ai.destination = target;

            if (GameController.State == GameState.Playing && !downing)
            {
                Shoot();

                fireCountDown -= Time.deltaTime;

                if (transform.position.y < 0.45f)
                {
                    downing = true;
                    GetComponent<BoxCollider>().material = gameController.SlideMat;
                    rb.mass = 20f;
                    CrownControl();
                }
                else if (transform.position.y < -2f)
                {
                    transform.parent = null;
                }

                if (downing && killingMeObjects.Count > 0)
                {
                    Kill();
                }

                if (killData.name == name && killData.kill > currentKill && Too.GetData<bool>(Data.killScale))
                {
                    Scale();
                }

            }

            if (transform.position.y < -9f)
            {

                Destroy(gameObject);
            }

        }

        void CrownControl()
        {
            if (gameController.killList[0].name == gameObject.name)
            {
                gameController.crown.transform.parent = null;
                gameController.crown.transform.position = new Vector3(0, -50f, 0);
            }
            int a = gameController.killList.FindIndex(i => i.name.Contains(gameObject.name));
            gameController.killList.RemoveAt(a);
        }

        public void Shoot()
        {
            time += Time.deltaTime;
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
        }

        public void Kill()
        {
            rb.constraints &= ~RigidbodyConstraints.FreezeRotation;

            float maxTime = -1;
            idx = -1;

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
                    if (killingMeObjects[idx].killingMeObject.name == playerController.name)
                    {
                        killingMeObjects[idx].killingMeObject.GetComponent<PlayerController>().killData.kill++;
                    }
                    else
                    {
                        killingMeObjects[idx].killingMeObject.GetComponent<AIDestinationSetter>().killData.kill++;
                    }
                }
            }
        }

        public void Scale()
        {
            currentKill = killData.kill;
            StartCoroutine(scaling(transform.localScale.x, transform.localScale.x + playerController.scaleSize.x, playerController.scaleTime));

            rb.mass = rb.mass + (playerController.mass * killData.kill);
            GameObject PS = Instantiate(canvasController.scaleParticle, transform.position, Quaternion.identity);
            PS.transform.GetChild(0).GetComponent<ParticleSystem>().startColor = GetComponent<Renderer>().material.color;
            PS.transform.GetChild(0).localScale = PS.transform.GetChild(0).localScale + (playerController.scaleSize * killData.kill);
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
                transform.localScale = Vector3.one * lerpe(curScl, nxtScl, playerController.scaleAnimCurve.Evaluate(dt / time));
                yield return eof;
                dt += Time.deltaTime;
            }
            transform.localScale = Vector3.one * nxtScl;
        }

        public Vector3 RandomPos()
        {
            target = new Vector3(Random.Range(-24f, 24f), 0, Random.Range(-24f, 24f));

            while ((Physics.CheckSphere(target, radius, obstacleMask)))
            {
                target = new Vector3(Random.Range(-24f, 24f), 0, Random.Range(-24f, 24f));
            }

            return target;
        }

        public void TargetPosChanger()
        {
            target = RandomPos();
        }

        void OnCollisionEnter(Collision other)
        {
            if (other.gameObject.CompareTag("Bot"))
            {
                TargetPosChanger();
            }

            if (other.gameObject.CompareTag("Bullet") && other.gameObject.GetComponent<Bullet>().target != null)
            {
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

    }
}
