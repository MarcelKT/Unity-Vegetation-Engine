using Assets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class SceneObject {
    protected GameObject _object;
    protected GridElement _gridPosition;

	public SceneObject(GameObject gameObject, GridElement position) {
		_object = gameObject;
		_gridPosition = position;
		_gridPosition.Register(this);
	}

	public GameObject GameObject {
		get { return _object; }
	}

	public GridElement GridElement {
		get { return _gridPosition; }
	}

	public void Remove() {
		_gridPosition.Unregister(this);
		GameObject.Destroy(_object);
    }

    public Vector3 Position {
        get { return _object.transform.position; }
        set { _object.transform.position = value; }
    }

    public void Update() {
		if (_gridPosition.Contains(_object.transform.position))
			return;

		_gridPosition = GridManager.GetInstance().GetContainingGridElement(0, _object.transform.position);
	}
}

public class PlantDefinition {
    protected string _speciesName;                  // Name of the plant species
	protected int _speciesID;
    protected GameObject _object;					// The base model
    protected float _speciesRadius;					// How close the plant may spawn to others of its species
	protected int _sizeGroup;						// The size group of the plant, i.e. a group of similarly sized plants
	protected float _sizeGroupRadius;				// How close the plant may spawn to others of its size group
    protected int _rarityRange;						// Range used for randomised plant spawning, Random(0, _rarityRange)
    protected int _rarityThreshold;                 // Threshold that the random number must reach before spawning, Random(0, _rarityRange) > _rarityThreshold = spawn
	protected float _placementTimer;				// How often the plant may attempt spawning

	// The _speciesCooldown is reduced by _speciesCooldownCost every time this plant spawns.
	// If it falls to 0, then the plant must wait until the threshold is reached to start spawning again
	// Adds a bit of organised randomness to the spawning. Visually pleasing.
    protected float _speciesCooldownMax;
    protected float _speciesCooldownThreshold;
    protected float _speciesCooldown;
    protected float _speciesCooldownCost;
    protected bool _canPlace;						// An internally-managed value termining whether the plant may spawn.

    public PlantDefinition(string species,
            GameObject obj,
            float speciesRadius,
			float sizeGroupRadius,
            int sizeGroup,
            int rarityRange,
            int rarityThreshold,
            float placementTimer,
            float speciesCooldownMax,
            float speciesCooldownThreshold,
            float speciesCooldownCost) {
        _speciesName = species;
        _object = obj;
		_speciesRadius = speciesRadius;
		_sizeGroupRadius = sizeGroupRadius;
		_sizeGroup = sizeGroup;
        _rarityRange = rarityRange;
        _rarityThreshold = rarityThreshold;
        _placementTimer = placementTimer;
        _speciesCooldown = speciesCooldownMax;
        _speciesCooldownMax = speciesCooldownMax;
        _speciesCooldownThreshold = speciesCooldownThreshold;
        _speciesCooldownCost = speciesCooldownCost;
        _canPlace = true;
	}

	public static bool operator ==(PlantDefinition a, PlantDefinition b) {
		return (a._speciesID == b._speciesID);
	}

	public static bool operator !=(PlantDefinition a, PlantDefinition b) {
		return (a._speciesID != b._speciesID);
	}

	public void RecoverCooldown(float amount) {
        _speciesCooldown += amount;

        if (_speciesCooldown > _speciesCooldownMax)
            _speciesCooldown = _speciesCooldownMax;

        if (_speciesCooldown == _speciesCooldownMax)
            _canPlace = true;
    }

    public void DrainCooldown() {
        _speciesCooldown -= _speciesCooldownCost;

        if (_speciesCooldown < _speciesCooldownThreshold)
            _canPlace = false;
    }

	// Getters
	public string SpeciesName { get { return _speciesName; } }
	public int SpeciesID { get { return _speciesID; } set { _speciesID = value; } }
	public int SizeGroup { get { return _sizeGroup; } }
    public float PlacementAttemptDelay { get { return _placementTimer; } }
    public float SpeciesRadius { get { return _speciesRadius; } }
    public GameObject ObjectInstance { get { return _object; } }
    public int RarityRange { get { return _rarityRange; } }
    public int RarityThreshold { get { return _rarityThreshold; } }
    public float SizeGroupRadius { get { return _sizeGroupRadius; } }
    public float SpeciesCooldown { get { return _speciesCooldown; } }
    public float SpeciesCooldownMax { get { return _speciesCooldownMax; } }
    public float SpeciesCooldownThreshold { get { return _speciesCooldownThreshold; } }
    public float SpeciesCooldownCost { get { return _speciesCooldownCost; } }
    public bool CanPlace { get { return _canPlace; } }
}

class Plant : SceneObject {
    float _objectSize;
    PlantDefinition _plant;

    public Plant(PlantDefinition plant,
                 float scale,
				 Vector3 position,
				 Quaternion angle,
				 Transform parent,
                 GridElement gridPosition)
			: base(GameObject.Instantiate(plant.ObjectInstance, position, angle, parent), 
				   gridPosition) {
		_plant = plant;
		_object.name = _plant.SpeciesName;
		_object.transform.localScale = new Vector3(scale, scale, scale);
		_objectSize = scale;
	}

	public static bool operator ==(Plant a, Plant b) {
		return (a._plant == b._plant);
	}

	public static bool operator !=(Plant a, Plant b) {
		return (a._plant != b._plant);
	}

	public static bool operator ==(Plant a, PlantDefinition b) {
		return (a._plant == b);
	}

	public static bool operator !=(Plant a, PlantDefinition b) {
		return (a._plant != b);
	}

	public PlantDefinition Species { get { return _plant; } }

	public List<SceneObject> GetPlantsInRange() {
		List<SceneObject> plantsInRange = new List<SceneObject>();
		float speciesRadius = _plant.SpeciesRadius;
		List<GridElement> path = new List<GridElement>();
		List<GridElement> elementsInRange = new List<GridElement>();

		_gridPosition.GridElementsInRange(_object.transform.position, _plant.SpeciesRadius, ref path, ref elementsInRange);
		Debug.Log("Objects In Range:: " + elementsInRange.Count);

		return plantsInRange;
	}
}

public struct PlantTypeClusterProfile {
    public int clusterIndex;
    public int plantIndex;
    public float minDistance;

    public PlantTypeClusterProfile(int cI, int pI, float mD) {
        clusterIndex = cI;
        plantIndex = pI;
        minDistance = mD;
    }

    public void Replace(int cI, int pI, float mD) {
        clusterIndex = cI;
        plantIndex = pI;
        minDistance = mD;
    }
}

public struct PlantTypeCluster {
    int plantType;
    int size;
    int maxClusterSize;

    public PlantTypeCluster(int pT, int s, int mCS) {
        plantType = pT;
        size = s;
        maxClusterSize = mCS;
    }

    public int PlantType {
        get { return plantType; }
    }

    public int ClusterSize {
        get { return size; }
        set { size = value; }
    }

    public void AddMember() {
        size++;
        Debug.Log("Size:: " + size);
    }

    public int MaxClusterSize {
        get { return maxClusterSize; }
    }
}

public class VegetationCollection : IEnumerable<PlantDefinition> {
    List<PlantDefinition> _plants;
	float _maxRadius;

    public VegetationCollection(List<PlantDefinition> plants) {
        _plants = plants;
		_maxRadius = 0.0f;

		for (int plantID = 0; plantID < plants.Count; plantID++) {
			_plants[plantID].SpeciesID = plantID;

			if (_plants[plantID].SpeciesRadius > _maxRadius)
				_maxRadius = _plants[plantID].SpeciesRadius;
			else if (_plants[plantID].SizeGroupRadius > _maxRadius)
				_maxRadius = _plants[plantID].SizeGroupRadius;
		}
	}

    public void AddPlant(PlantDefinition plant) {
        plant.SpeciesID = _plants.Count;
        _plants.Add(plant);

		if (plant.SpeciesRadius > _maxRadius)
			_maxRadius = plant.SpeciesRadius;
		else if (plant.SizeGroupRadius > _maxRadius)
			_maxRadius = plant.SizeGroupRadius;
	}

    public PlantDefinition GetPlant(int index) {
        return _plants[index];
    }

	public PlantDefinition this[int i] {
		get { return _plants[i]; }
		set { _plants[i] = value; }
	}

	public int Range {
        get { return _plants.Count; }
    }

	public float MaxRange {
		get { return _maxRadius; }
	}

	public IEnumerator<PlantDefinition> GetEnumerator() { return _plants.GetEnumerator(); }
	
	IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
}

public class Painter : MonoBehaviour {
	//List<List<int>> placedPlantClusters;
	//List<PlantTypeCluster> placedClusters;
	public Material transparent;
	public Material materialPlace;
    public Material materialRemove;
    public Material materialSlowRemove;
    public Material materialDefault;
    RaycastHit mouseRayHit;
    public Terrain worldTerrain;
    public VegetationCollection vegetation;
    public GameObject[] inPlantObjects;
    public int[] inPlantRadii;
    public int[] inPlantSizeRadii;
    public int[] inPlantSizes;
    public int[] inPlantRarityRanges;
    public int[] inPlantRarityThresholds;
    public int[] inPlantSpeciesCooldownMax;
    public int[] inPlantSpeciesCooldownThreshold;
    public int[] inPlantSpeciesCooldownCost;
    public float[] inPlantPlacementTimers;
    public int seed = 38578657;
    float[] placementTimersCurrent;
    //List<Plant> placedPlants;
    public int plantPlacementAttempts = 5;
    public GameObject vegetationObject;
    float placeRemoveInputLast = 0.0f;
    Perlin perlin;
    double[,] shrubClusterMap;
    double[,] treeClusterMap;
    bool slow;
    GridManager gridManager;
	SceneObject painterObject;

	List<int[]> painterTypes = new List<int[]>() {
        new int[4]{ 0, 1, 2, 3},
        new int[3]{ 4, 5, 6 },
        new int[3]{ 8, 9, 10 }
    };

    bool[] painterTypesCurrent = new bool[3] { false, false, false };

    List<int> painterTypesCurrentArray = new List<int>();

    public void setPainterType(int index, bool state) {
        if (state == painterTypesCurrent[index])
            return;

        if (state)
            for (int i = 0; i < painterTypes[index].Length; i++)
                painterTypesCurrentArray.Add(painterTypes[index][i]);
        else
            for (int i = 0; i < painterTypes[index].Length; i++)
                painterTypesCurrentArray.Remove(painterTypes[index][i]);

        painterTypesCurrent[index] = state;
    }

    public void togglePainterType(int index) {
        setPainterType(index, !painterTypesCurrent[index]);
    }

    // Use this for initialization
    void Start () {
		//placedClusters = new List<PlantTypeCluster>();
        List<Plant> plants = new List<Plant>();
        perlin = new Assets.Perlin();
        shrubClusterMap = new double[2500, 2500];
        treeClusterMap = new double[2500, 2500];
        
        for (int z = 0; z < 2500; z += 5)
            for (int x = 0; x < 2500; x += 5) {
				shrubClusterMap[x, z] = perlin.OctavePerlin(((double)x) / 2500, 0.0, ((double)z) / 2500, 4, 8.0);
				treeClusterMap[x, z] = perlin.OctavePerlin(((double)x) / 2500, 0.2, ((double)z) / 2500, 4, 8.0);

				for (int i = 0; i < 5; i++) {
					for (int j = 0; j < 5; j++) {
						shrubClusterMap[x + i, z + j] = shrubClusterMap[x, z];
						treeClusterMap[x + i, z + j] = treeClusterMap[x, z];
					}
				}
            }
        //																						Radius  SizeRadius  Size    RarityRange RarityThrs  PlaceTimer  CDMax   CDThrs  CDCost
        vegetation = new VegetationCollection(new List<PlantDefinition>());
        vegetation.AddPlant(new PlantDefinition("Fern 01", inPlantObjects[0],					3,		1,			2,		10,			3,			0.01f,		10,		1,		1));
        vegetation.AddPlant(new PlantDefinition("Pine 01", inPlantObjects[1],					10,		50,			10,		100,		95,			0.2f,		10,		1,		1));
        vegetation.AddPlant(new PlantDefinition("Pine 02", inPlantObjects[2],					10,		50,			10,		100,		90,			0.1f,		10,		1,		1));
        vegetation.AddPlant(new PlantDefinition("Palm Dual Curved 01", inPlantObjects[3],		20,		50,			10,		100,		90,			0.1f,		10,		1,		1));
        vegetation.AddPlant(new PlantDefinition("Palm Dual Straight 01", inPlantObjects[4],		20,		50,			10,		100,		90,			0.1f,		10,		1,		1));
        vegetation.AddPlant(new PlantDefinition("Palm Single Curved 01", inPlantObjects[5],		20,     50,			10,		100,		90,			0.1f,		10,		1,		1));
        vegetation.AddPlant(new PlantDefinition("Palm Single Straight 01", inPlantObjects[6],	20,		50,			10,		100,		90,			0.1f,		10,		1,		1));
        vegetation.AddPlant(new PlantDefinition("Palm Triple 01", inPlantObjects[7],			20,		50,			10,		100,		90,			0.1f,		10,		1,		1));
        vegetation.AddPlant(new PlantDefinition("Rose Bush 01", inPlantObjects[8],				5,		1,			4,		10,			6,			0.05f,		10,		1,		1));
        vegetation.AddPlant(new PlantDefinition("Leafy Bush 01", inPlantObjects[9],				3,		1,			3,		10,			3,			0.05f,		10,		1,		1));
        vegetation.AddPlant(new PlantDefinition("Leaf Plant 01", inPlantObjects[10],			1,		1,			2,		16,			2,			0.01f,		10,		1,		1));

		placementTimersCurrent = new float[inPlantObjects.Length];
		for (int i = 0; i < placementTimersCurrent.Length; i++)
			placementTimersCurrent[i] = 0.0f;

		Random.InitState(seed);

        gridManager = GridManager.GetInstance();
		
		Vector3 gridSize = new Vector3(2500.0f, 1200.0f, 2500.0f);
		GridElement mainGrid = new GridElement(new Vector3(gridSize.x / 2.0f, gridSize.y / 2.0f - 200.0f, gridSize.z / 2.0f), gridSize);
		mainGrid.GenerateUniformGrid(new Dimensions[3] { new Dimensions(2, 1, 2), new Dimensions(5, 4, 5), new Dimensions(4, 4, 4) });
		//mainGrid.SetMaterial(transparent);
		
		gridManager.AddGrid(mainGrid);
		
		painterObject = new SceneObject(gameObject, gridManager.GetContainingGridElement(0, transform.position));
	}

	bool canPlace(List<Plant> plantsInRange, Vector3 position, float radius, PlantDefinition plantType) {
		foreach (Plant plant in plantsInRange)
			if (plant == plantType)
				if (Vector3.Distance(position, plant.Position) < radius)
					return false;
		return true;
	}

	void removePlants(List<SceneObject> removalQueue) {
        foreach (SceneObject plant in removalQueue) {
			plant.Remove();
			//removalQueue[i].Sort();
			//removalQueue[i].Reverse();

			//for (int j = 0; j < removalQueue[i].Count; j++) {
			//Destroy(placedPlants[i][removalQueue[i][j]].GameObject);
			//placedPlants[i][j].Remove();
			//placedPlants[i].RemoveAt(removalQueue[i][j]);
			//}
		}
    }

    /*List<List<int>> GetPlantsInRange(Vector3 position, float radius) {
        List<List<int>> output = new List<List<int>>();
        int plantTypeCount = inPlantObjects.Length;

        for (int plantType = 0; plantType < plantTypeCount; plantType++) {
            output.Add(new List<int>());

            for (int plant = 0; plant < placedPlants[plantType].Count; plant++) {
				if (Vector3.Distance(placedPlants[plantType][plant].GameObject.transform.position, position) < radius) {
					output[plantType].Add(plant);
				}
			}
        }

        return output;
    }*/

    void PlaceVegetation(float radius, float dT, float density, bool replace = false) {
        int plantTypeCount = inPlantObjects.Length;
        List<SceneObject> removalQueue = new List<SceneObject>();
        
		List<Plant> plantsInRange = gridManager.GetObjectsInRange<Plant>(painterObject, radius);

        Vector2 positionInCircle;
        Vector3 position;
        Quaternion angle;
        Transform parent = vegetationObject.transform;
        float scale;

        foreach (PlantDefinition plant in vegetation) {
            if (!painterTypesCurrentArray.Contains(plant.SizeGroup))
                continue;

            for (int i = 0; i < (int)(density * density); i++) {
                if (Random.Range(0, plant.RarityRange) > plant.RarityThreshold) {
                    positionInCircle = Random.insideUnitCircle * radius;
                    position = transform.position + new Vector3(positionInCircle.x, 0.0f, positionInCircle.y);

                    if (position.x < 0 || position.x > 2500 || position.z < 0 || position.z > 2500)
                        continue;

                    scale = Random.Range(0.75f, 1.25f);

					//removePlantsInCircle(plantsInCircle, ref removalQueue, position, vegetation.GetPlant(plantType).SpeciesRadius, plantType);
					if (!canPlace(plantsInRange, position, plant.SpeciesRadius, plant))
						continue;

                    angle = Quaternion.Euler(new Vector3(0.0f, Random.rotation.eulerAngles.y, 0.0f));
					GridElement gridElement = gridManager.GetContainingGridElement(0, position);

					if (gridElement == null)
						Debug.Log("Grid Element Exists:: False");

					if (plant.SpeciesID == 0 || plant.SpeciesID == 9) {
                        if (shrubClusterMap[(int)position.x, (int)position.z] > 0.5) {
							plantsInRange.Add(new Plant(vegetation.GetPlant(0), scale, position, angle, parent, gridElement));
						}
						else {
							plantsInRange.Add(new Plant(vegetation.GetPlant(9), scale, position, angle, parent, gridElement));
						}
					}
                    else if (plant.SpeciesID == 8) {
                        if (shrubClusterMap[(int)position.x, (int)position.z] < 0.3) {
							plantsInRange.Add(new Plant(plant, scale, position, angle, parent, gridElement));
						}
					}
                    else if (plant.SpeciesID == 10) {
                        if (shrubClusterMap[(int)position.x, (int)position.z] > 0.4) {
							plantsInRange.Add(new Plant(plant, scale, position, angle, parent, gridElement));
						}
					}
                    else if (plant.SpeciesID > 0 && plant.SpeciesID < 8) {
                        if (treeClusterMap[(int)position.x, (int)position.z] > 0.5) {
                            if (plant.SpeciesID == 1 || plant.SpeciesID == 2) {
								plantsInRange.Add(new Plant(plant, scale, position, angle, parent, gridElement));
							}
						}
                        else {
                            if (plant.SpeciesID > 2 && plant.SpeciesID < 8) {
								plantsInRange.Add(new Plant(plant, scale, position, angle, parent, gridElement));
							}
						}
                    } else {
						plantsInRange.Add(new Plant(plant, scale, position, angle, parent, gridElement));
					}
				}

                placementTimersCurrent[plant.SpeciesID] = plant.PlacementAttemptDelay;
            }
        }

        removePlants(removalQueue);
    }

    void SetInvisible() {
        GetComponent<Renderer>().material.color = GetComponent<Renderer>().material.color * new Vector4(1.0f, 1.0f, 1.0f, 0.0f);
    }

    void SetVisible() {
        GetComponent<Renderer>().material.color = GetComponent<Renderer>().material.color * new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
    }
	
	void RemoveVegetation(float radius, bool slowRemove) {
        List<Plant> plantsInRange = gridManager.GetObjectsInRange<Plant>(painterObject, radius);

        foreach (Plant plant in plantsInRange) {
			if (!painterTypesCurrentArray.Contains(plant.Species.SizeGroup))
				continue;

			if (slowRemove)
				if (Random.Range(0, 15) != 0)
					continue;
            
			plant.Remove();
		}
	}

	// Update is called once per frame
	void Update () {
        float placeRemoveInput = Input.GetAxis("PlaceRemove");
        float slowFastInput = Input.GetAxis("SlowFast");

        if (slowFastInput != 0.0f) {
            if (slowFastInput < 0.0f)
                slow = true;
            else
                slow = false;
        }
		
        if (Input.GetKey(KeyCode.LeftAlt)) {
            float mouseScroll = Input.GetAxis("Mouse ScrollWheel");
            float newScale = 1.0f + mouseScroll;
            Vector3 oldScale = transform.localScale;

            transform.localScale = new Vector3(oldScale.x * newScale, oldScale.y, oldScale.z * newScale);
        }

        bool brushIsUsed = false;
        for (int i = 0; i < painterTypesCurrent.Length; i++)
            if (painterTypesCurrent[i])
                brushIsUsed = true;

        // If no brushes are being used, no need to burn CPU cycles
        if (!brushIsUsed) {
            SetInvisible();
            return;
        }
        else {
            SetVisible();
        }
        
        if (placeRemoveInput != placeRemoveInputLast) {
            if (placeRemoveInput > 0.0f)
                GetComponent<Renderer>().material = materialPlace;
            else if (placeRemoveInput < 0.0f) {
                if (slow)
                    GetComponent<Renderer>().material = materialSlowRemove;
                else
                    GetComponent<Renderer>().material = materialRemove;
            }
        }
        else {
            GetComponent<Renderer>().material = materialDefault;
        }

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (worldTerrain.GetComponent<Collider>().Raycast(ray, out mouseRayHit, Mathf.Infinity)) {
            transform.position = mouseRayHit.point;
			painterObject.Update();
            SetVisible();
        } else {
            SetInvisible();
        }

        for (int i = 0; i < inPlantObjects.Length; i++)
            if (placementTimersCurrent[i] > 0.0)
                placementTimersCurrent[i] -= Time.deltaTime;

        if (Input.GetMouseButton(0)) {
            if (placeRemoveInput > 0.0f)
                PlaceVegetation(transform.localScale.x / 2.0f, Time.deltaTime, transform.localScale.x / 30.0f);
            else if (placeRemoveInput < 0.0f)
                RemoveVegetation(transform.localScale.x / 2.0f, slow);
        }
    }
}
