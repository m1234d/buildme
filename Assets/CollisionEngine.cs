using Leap.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows.Speech;

public class CollisionEngine : MonoBehaviour
{
    public GameObject leftHand;
    public GameObject rightHand;
    public GameObject cubeBrickPrefab;
    public GameObject cubeRedPrefab;
    public GameObject cubeYellowPrefab;
    public GameObject cubeBluePrefab;
    public GameObject sphereBrickPrefab;
    public GameObject sphereRedPrefab;
    public GameObject sphereYellowPrefab;
    public GameObject sphereBluePrefab;
    public GameObject doorPrefab;
    public GameObject roofPrefab;
    public GameObject wallPrefab;
    float pubX;
    float pubZ;
    float offX;
    float offZ;
    GameObject cube;
    GameObject sphere;
    public List<GameObject> objects = new List<GameObject>();
    Vector3 lastVector = new Vector3(0, 0, 0);
    public ConfidenceLevel confidence = ConfidenceLevel.Medium;
    public float speed = 1;
    KeywordRecognizer recognizer;
    // keyword array
    public string[] Keywords_array;
    public string type;
    public string color;
    // Start is called before the first frame update
    void Start()
    {
        Physics.gravity = new Vector3(0, -7f, 0);

        type = "cube";
        color = "red";
        // Change size of array for your requirement
        Keywords_array = new string[8];
        Keywords_array[0] = "cube";
        Keywords_array[1] = "sphere";
        Keywords_array[2] = "door";
        Keywords_array[3] = "roof";
        Keywords_array[4] = "red";
        Keywords_array[5] = "brick";
        Keywords_array[6] = "yellow";
        Keywords_array[7] = "blue";





        // instantiate keyword recognizer, pass keyword array in the constructor
        recognizer = new KeywordRecognizer(Keywords_array, ConfidenceLevel.Medium);
        recognizer.OnPhraseRecognized += OnKeywordsRecognized;
        // start keyword recognizer
        recognizer.Start();

    }
    void OnKeywordsRecognized(PhraseRecognizedEventArgs args)
    {
        if (args.text == "red" || args.text == "yellow" || args.text == "blue" || args.text == "brick")
        {
            color = args.text;
        }
        else
        {
            type = args.text;
        }
        Debug.Log("Keyword: " + args.text + "; Confidence: " + args.confidence + "; Start Time: " + args.phraseStartTime + "; Duration: " + args.phraseDuration);
        // write your own logic
    }

    bool VectorEqual(Vector3 v1, Vector3 v2)
    {
        float e = 3.2f;
        if(Math.Abs(v1.x - v2.x) <= e && Math.Abs(v1.y - v2.y) <= e && Math.Abs(v1.z - v2.z) <= e )
        {
            return true;
        }
        return false;
    }
    bool VectorEqualRight(Vector3 v1, Vector3 v2)
    {
        float e = 2.5f;
        if (Math.Abs(v1.x - v2.x) <= e && Math.Abs(v1.y - v2.y) <= e && Math.Abs(v1.z - v2.z) <= e)
        {
            return true;
        }
        return false;
    }
    bool VectorClose(Vector3 v1, Vector3 v2)
    {
        float e = 4f;
        //Debug.Log(v1.x - v2.x);
        //Debug.Log(v1.y - v2.y);
        //Debug.Log(v1.z - v2.z);
        if (Math.Abs(v1.x - v2.x) <= e && Math.Abs(v1.y - v2.y) <= e && Math.Abs(v1.z - v2.z) <= e)
        {
            return true;
        }
        return false;
    }

    void SnapCollisions()
    {
        foreach(GameObject obj in objects)
        {
            Debug.Log(obj.GetComponent<FixedJoint>());
            if (obj.GetComponent<ObjectCollider>().isCollided && obj.GetComponent<FixedJoint>() == null)
            {
                Debug.Log("yay");
                obj.AddComponent<FixedJoint>();
                obj.GetComponent<FixedJoint>().connectedBody = obj.GetComponent<ObjectCollider>().collided.GetComponent<Rigidbody>();
            }
        }
    }
    void Reset()
    {
        foreach(GameObject obj in objects)
        {
            Destroy(obj);
        }
        if(cube != null)
        {
            Destroy(cube);
            cube = null;
        }
        objects = new List<GameObject>();
    }
    void EnablePhysics()
    {
        if (cube != null)
        {
            objects.Add(cube);
        }
        SnapCollisions();
        cube = null;
        foreach (GameObject obj in objects)
        {
            if(obj.GetComponent<BoxCollider>() != null)
            {
                obj.GetComponent<BoxCollider>().isTrigger = false;
            }
            if(obj.GetComponent<SphereCollider>() != null)
            {
                obj.GetComponent<SphereCollider>().isTrigger = false;
            }
            obj.GetComponent<Rigidbody>().isKinematic = false; 
        }
    }
    bool creating = false;
    bool rotating = false;
    bool finalMan = false;
    float lastZ = 0;
    float lastX = 0;

    void BuildRectangle(RiggedHand leftRig, RiggedHand rightRig)
    {
        Vector3 leftIndex = leftRig.fingers[1].GetTipPosition();
        Vector3 leftThumb = leftRig.fingers[0].GetTipPosition();
        Vector3 leftMiddle = leftRig.fingers[2].GetTipPosition();
        Vector3 leftRing = leftRig.fingers[3].GetTipPosition();
        Vector3 leftPinkey = leftRig.fingers[4].GetTipPosition();

        Vector3 rightIndex = rightRig.fingers[1].GetTipPosition();
        Vector3 rightThumb = rightRig.fingers[0].GetTipPosition();
        Vector3 rightMiddle = rightRig.fingers[2].GetTipPosition();
        Vector3 rightRing = rightRig.fingers[3].GetTipPosition();
        Vector3 rightPinkey = rightRig.fingers[4].GetTipPosition();

        bool deadMan = true;


        if (VectorEqual(leftIndex, leftThumb) && VectorEqual(leftIndex, leftMiddle) && VectorEqual(leftMiddle, leftRing) && VectorEqual(leftRing, leftPinkey)
            && VectorEqual(rightIndex, rightThumb) && VectorEqual(rightIndex, rightMiddle) && VectorEqual(rightMiddle, rightRing) && VectorEqual(rightRing, rightPinkey))
        {
            Vector3 midpoint = (rightIndex + leftIndex) / 2;
            Vector3 diff = rightIndex - midpoint;
            //this.gameObject.transform.position = midpoint;
            Vector3 projectedVectorZ = Vector3.ProjectOnPlane(diff, Vector3.up);
            float angleZ = (diff.y / Math.Abs(diff.y)) * Vector3.Angle(diff, projectedVectorZ);

            Vector3 projectedVectorX = Vector3.ProjectOnPlane(diff, Vector3.back);
            float angleX = -(diff.z / Math.Abs(diff.z)) * Vector3.Angle(diff, projectedVectorX);
            
            if(!float.IsNaN(angleX) && !float.IsNaN(angleZ))
            {
                pubX = angleX;
                pubZ = angleZ;
                this.gameObject.transform.localRotation = Quaternion.Euler(0, angleX+lastX, angleZ+lastZ);

            }
            // Debug.Log(angle);
            deadMan = false;
            finalMan = false;

        }

        else if ((VectorEqual(leftThumb, leftIndex) || VectorEqualRight(rightThumb, rightIndex)) && VectorClose(leftIndex, rightIndex)
            && leftHand.activeSelf && rightHand.activeSelf)
        {
            if (creating == false)
            {
                creating = true;
                if (cube != null)
                {
                    objects.Add(cube);
                }
                SnapCollisions();
                if(type == "cube" && color == "brick")
                {
                    cube = Instantiate(cubeBrickPrefab, this.gameObject.transform);
                    cube.transform.localScale = new Vector3(.1f, .1f, .1f);

                }
                else if(type == "cube" && color == "red")
                {
                    cube = Instantiate(cubeRedPrefab, this.gameObject.transform);
                    cube.transform.localScale = new Vector3(.1f, .1f, .1f);
                }
                else if (type == "cube" && color == "yellow")
                {
                    cube = Instantiate(cubeYellowPrefab, this.gameObject.transform);
                    cube.transform.localScale = new Vector3(.1f, .1f, .1f);
                }
                else if (type == "cube" && color == "blue")
                {
                    cube = Instantiate(cubeBluePrefab, this.gameObject.transform);
                    cube.transform.localScale = new Vector3(.1f, .1f, .1f);
                }
                else if(type == "sphere" && color == "brick")
                {
                    cube = Instantiate(sphereBrickPrefab, this.gameObject.transform);
                    cube.transform.localScale = new Vector3(.1f, .1f, .1f);

                }
                else if (type == "sphere" && color == "red")
                {
                    cube = Instantiate(sphereRedPrefab, this.gameObject.transform);
                    cube.transform.localScale = new Vector3(.1f, .1f, .1f);

                }
                else if (type == "sphere" && color == "yellow")
                {
                    cube = Instantiate(sphereYellowPrefab, this.gameObject.transform);
                    cube.transform.localScale = new Vector3(.1f, .1f, .1f);

                }
                else if (type == "sphere" && color == "blue")
                {
                    cube = Instantiate(sphereBluePrefab, this.gameObject.transform);
                    cube.transform.localScale = new Vector3(.1f, .1f, .1f);

                }
                else if (type == "door")
                {
                    cube = Instantiate(doorPrefab);
                    cube.transform.localScale = new Vector3(.01f, .01f, .01f);

                }
                else if (type == "roof")
                {
                    cube = Instantiate(roofPrefab);
                    cube.transform.localScale = new Vector3(.1f, .1f, .1f);

                }
                cube.transform.rotation = Quaternion.Euler(0, 0, 0);

            }
        }
        else if (VectorEqual(leftThumb, leftIndex) && VectorEqualRight(rightThumb, rightIndex) && !VectorEqual(leftIndex, leftMiddle))
        {
            creating = false;
            rotating = true;
        }
        else if ((!VectorEqual(leftThumb, leftIndex) || !VectorEqualRight(rightThumb, rightIndex)) && !(!VectorEqual(leftThumb, leftIndex) && !VectorEqualRight(rightThumb, rightIndex)) && rotating)
        {
            //lastVector = rightIndex - leftIndex;
            //rotating = false;
            //creating = true;
        }
        else if ((!VectorEqual(leftThumb, leftIndex) && !VectorEqualRight(rightThumb, rightIndex)))
        {
            creating = false;
            rotating = false;
        }

        if (creating)
        {
            Vector3 midpoint = (rightIndex + leftIndex) / 2;
            Vector3 diff = rightIndex - leftIndex;
            cube.transform.position = midpoint;
            cube.transform.localScale = new Vector3(diff.x, diff.y, diff.z);
        }
        else if(rotating)
        {
            Vector3 midpoint = (rightIndex + leftIndex) / 2;
            Vector3 diff = rightIndex - midpoint;
            cube.transform.position = midpoint;
            Vector3 projectedVectorZ = Vector3.ProjectOnPlane(diff, Vector3.up);
            float angleZ = (diff.y/ Math.Abs(diff.y)) * Vector3.Angle(diff, projectedVectorZ);

            Vector3 projectedVectorX = Vector3.ProjectOnPlane(diff, Vector3.back);
            float angleX = -(diff.z / Math.Abs(diff.z)) * Vector3.Angle(diff, projectedVectorX);

            Vector3 projectedVectorY = Vector3.ProjectOnPlane(diff, Vector3.up);
            float angleY = (diff.y / Math.Abs(diff.y)) * Vector3.Angle(diff, projectedVectorY);
            // Debug.Log(angle);
            if (!float.IsNaN(angleX) && !float.IsNaN(angleZ))
            {
                cube.transform.rotation = Quaternion.Euler(0, angleX, angleZ);
            }
        }
        if (deadMan && !finalMan)
        {
            Vector3 midpoint = (rightIndex + leftIndex) / 2;
            Vector3 diff = rightIndex - midpoint;
            Vector3 projectedVectorZ = Vector3.ProjectOnPlane(diff, Vector3.up);
            float angleZ = (diff.y / Math.Abs(diff.y)) * Vector3.Angle(diff, projectedVectorZ);

            Vector3 projectedVectorX = Vector3.ProjectOnPlane(diff, Vector3.back);
            float angleX = -(diff.z / Math.Abs(diff.z)) * Vector3.Angle(diff, projectedVectorX);

            lastZ += pubZ;
            lastX += pubX;
            Debug.Log(lastZ);
            Debug.Log(lastX);
            finalMan = true;
        }
    }
    public void GetGesture()
    {
        Debug.Log("ayy lmao");
    }
    // Update is called once per frame
    void Update()
    {
        RiggedHand leftRig = leftHand.GetComponent<RiggedHand>();
        RiggedHand rightRig = rightHand.GetComponent<RiggedHand>();
       // Debug.Log(type);
        BuildRectangle(leftRig, rightRig);

        if (Input.GetKey("g"))
        {
            EnablePhysics();
        }
        if (Input.GetKey("r"))
        {
            Reset();
        }
    }
}
