using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelEditor : Singleton<LevelEditor> {

    public bool buildMode;

    public bool generateGround;
    public GameObject ground;
    public Vector3 groundSize;

    public GameObject hollowCube;
    public LayerMask layerMask;

    public GameObject cube;

    public bool selectAll;
    public List<GameObject> selected;

    public bool quickAction = true;
    Vector3 buildDirection;
    int buildLevel;

    public Mesh initialMesh;
    

    void Start()
    {
        initialMesh = hollowCube.GetComponent<MeshFilter>().mesh;
        if (generateGround)
        {
            GameObject groundParent = new GameObject("Ground");
            for (int x = 0; x < groundSize.x; x++)
            {
                for (int y = 0; y < groundSize.y; y++)
                {
                    Vector3 spawnLoc = transform.position + Vector3.left * groundSize.x / 2;
                    spawnLoc += Vector3.forward * groundSize.y / 2;
                    spawnLoc.x += x; spawnLoc.z -= y;
                    GameObject newGround = Instantiate(ground, spawnLoc, transform.rotation) as GameObject;
                    newGround.transform.SetParent(groundParent.transform);
                    newGround.layer = 11;
                    newGround.GetComponent<Renderer>().enabled = false;
                }
            }
            GameObject groundModel = Instantiate(ground, Vector3.zero, transform.rotation) as GameObject;
            groundModel.transform.localScale = new Vector3(groundSize.x, 1, groundSize.y);
            groundModel.GetComponent<BoxCollider>().enabled = false;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            buildMode = !buildMode;
        }

        PlayerViewPoint();

        if (buildMode)
        {
            Control();            
            SpawnCube();
            Selected();
        }
    }

    void Control()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            selectAll = !selectAll;
            if (!selectAll)
            {
                ClearSelected();                
            }
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            quickAction = !quickAction;
        }

        if (quickAction)
        {
            if (Input.GetKeyDown(KeyCode.Home))
            {
                buildLevel = 1;
            }

                if (Input.GetKeyDown(KeyCode.PageUp))
            {
                buildLevel++;
            }
            if (Input.GetKeyDown(KeyCode.PageDown))
            {                
                    buildLevel--;
            }
            if (buildLevel < 1) buildLevel = 1;

                if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                buildDirection = Vector3.up;
            }
            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                buildDirection = Vector3.down;
            }
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                if (buildDirection == Vector3.left)
                {
                    buildDirection = Vector3.back;
                }
                else
                {
                    buildDirection = Vector3.left;
                }
            }
            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                if (buildDirection == Vector3.right)
                {
                    buildDirection = Vector3.forward;
                }
                else
                {
                    buildDirection = Vector3.right;
                }
            }

            for (int i = 0; i < selected.Count; i++)
            {
                Transform[] trans = selected[i].GetComponentsInChildren<Transform>();
                trans[trans.Length - 1].position = selected[i].transform.position + (buildDirection * buildLevel);
            }
        }
    }    

    void Selected()
    {
        for (int i = 0; i < selected.Count; i++)
        {
            if (!selected[i]) selected.Remove(selected[i]);
            else
            {
                MeshRenderer[] rends = selected[i].GetComponentsInChildren<MeshRenderer>();
                rends[rends.Length - 1].enabled = true;
            }
        }
    }

    void ClearSelected()
    {
        for (int i = 0; i < selected.Count; i++)
        {
            MeshRenderer[] rends = selected[i].GetComponentsInChildren<MeshRenderer>();
            rends[rends.Length - 1].enabled = false;
        }    
        selected = new List<GameObject>();
    }

    void SpawnCube()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            if (quickAction)
            {
                List<GameObject> newSelected = new List<GameObject>();                
                for (int i = 0; i < selected.Count; i++)
                {
                    if (Input.GetKey(KeyCode.LeftShift))
                        newSelected.Add(selected[i]);
                        GameObject finalCube = null;
                    for (int y = 1; y < buildLevel+1; y++)
                    {
                        GameObject newCube = Instantiate(cube, selected[i].transform.position + (buildDirection * y), transform.rotation) as GameObject;
                        AssignCube(newCube);
                        finalCube = newCube;
                        if (Input.GetKey(KeyCode.LeftShift))
                        newSelected.Add(finalCube);
                    }
                    if (!Input.GetKey(KeyCode.LeftShift))
                        newSelected.Add(finalCube);
                }
                ClearSelected();
                selected = newSelected;
                //quickAction = false;
                buildDirection = Vector3.zero;
            }
        }

        if (Input.GetMouseButtonDown(0))
        {                     
            {
                if (hollowCube.activeSelf)
                {
                    //increase/decrease amount of cubes spawned


                    GameObject newCube = Instantiate(cube, hollowCube.transform.position, transform.rotation) as GameObject;
                    AssignCube(newCube);
                    if (selectAll)
                    {
                        selected.Add(newCube);
                    }
                }
            }
        }
    }

    void AssignCube(GameObject cube)
    {
        GameObject store = GameObject.Find("Store");
        cube.transform.SetParent(store.transform);
        cube.name = "Cube";
    }

    void PlayerViewPoint()
    {
        if (buildMode || Player.Instance.GetComponent<PlayerInventory>().inventoryMode) { 
        Vector3 direction = Manager.Instance.cam.transform.TransformDirection(Vector3.forward);
        RaycastHit hit;
            if (Physics.Raycast(Manager.Instance.cam.transform.position, direction, out hit, 10, Manager.Instance.playerVisible) && hit.collider.gameObject.layer == 11)
            {

                hollowCube.SetActive(true);

                Vector3 scale = hit.transform.localScale;

                Vector3 cubeSideDirection = Vector3.zero;
                cubeSideDirection = GetSideDirection(cubeSideDirection, hit.point.x, hit.transform.position.x, Vector3.right, scale.x / 2);
                cubeSideDirection = GetSideDirection(cubeSideDirection, hit.point.y, hit.transform.position.y, Vector3.up, scale.x / 2);
                cubeSideDirection = GetSideDirection(cubeSideDirection, hit.point.z, hit.transform.position.z, Vector3.forward, scale.z / 2);
                hollowCube.transform.position = cubeSideDirection + hit.transform.position;
                hollowCube.transform.position += hit.collider.gameObject.GetComponent<BoxCollider>().bounds.center - hit.transform.position;
                Manager.Instance.PlaceItem(hollowCube);
                Manager.Instance.ColliderToModelSize(hollowCube);
                if (Input.GetMouseButtonDown(1) && buildMode)
                {
                    Destroy(hit.transform.gameObject);
                }
            }
            else
            {
                hollowCube.SetActive(false);
            }


        }   
        else
        {
            hollowCube.SetActive(false);
        }     
    }

    public Vector3 GetSideDirection(Vector3 currentDirection, float hitPointAxis, float objectPosAxis, Vector3 direction, float scale)
    {
        if (currentDirection == Vector3.zero)
        {
            if (hitPointAxis >= objectPosAxis + scale)
            {
                return direction;
            }
            if (hitPointAxis <= objectPosAxis - scale)
            {
                return -direction;
            }
            return Vector3.zero;
        } else
        {
            return currentDirection;
        }
    }
}
