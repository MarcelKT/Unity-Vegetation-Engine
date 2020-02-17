using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets {
    struct Dimensions {
        public int width, height, depth;

        public Dimensions(int x, int y, int z) {
            width = x;
            height = y;
            depth = z;
        }
    }

    // This class poses a database integrity issue - in order to function
    // > with the greatest minimisation of CPU performance, objects must hold a record of their GridElement
    // > and a GridElement must likewise hold a record of its children. If handled well (objects implement a "Remove" function
    // > that first unregisters them from the grid), the integrity issue should never the encountered.
    // Storing each reference only once is perfectly doable too (and easier to implement, in fact), but it
    // > will result in extra processing time. For a mechanics- and graphics-heavy game running on modern machines,
    // > memory can be expended if it means lowering CPU usage.
    class GridElement {
        List<GridElement> _neighbours;
        List<GridElement> _children;
        GridElement _parent;
        List<SceneObject> _objects;
		GameObject _object;
		BoxCollider _collider;
		float _rangeMax;
		float _rangeMin;
		Material material;
        
        Vector3 _halfSize;
        int _childWidth, _childHeight, _childDepth;
        int _testElement;
        Vector3 _position;

		public void PrintChildCount() {
			Debug.Log(_children.Count);
		}

        public GridElement(Vector3 pos, Vector3 size) {
            _neighbours = new List<GridElement>();
            _children = new List<GridElement>();
            _objects = new List<SceneObject>();
            _position = pos;
            _halfSize = size / 2.0f;
			_rangeMax = Vector3.Distance(_position, _position + _halfSize);
			_rangeMin = Mathf.Min(_halfSize.x, Mathf.Min(_halfSize.y, _halfSize.z));
			_testElement = 0;
            _childWidth = 0;
            _childHeight = 0;
            _childDepth = 0;
			_object = new GameObject("Grid Element");
			_object.AddComponent<BoxCollider>();
			_collider = _object.GetComponent<Collider>() as BoxCollider;
			_collider.size = size;
			_collider.center = pos;
			_parent = null;
		}

		public GridElement(Vector3 pos, Vector3 size, GridElement parent) : this(pos, size) {
			_parent = parent;
			_object.transform.SetParent(_parent._object.transform);
		}

		public GameObject Object {
			get { return _object; }
			set { _object = value; }
		}

		public bool InRange(Vector3 position, float range) {
			float distanceToCentre = Vector3.Distance(position, _position);

			// If position is within the grid element
			if (distanceToCentre <= _rangeMin)
				return true;

			// If the maximum range falls within the grid element's inner sphere
			if (range - _rangeMin <= distanceToCentre)
				return true;

            if (distanceToCentre - _rangeMax > range)
                return false;

			// Now we have to test for proper intesections...
			// Get the vector from position to the grid element, at maximum range
			// direciton = _position - position
			// unitDirection (|direction|) = Normalise(direction)
			// maxDirection = unitDirection * range
			// closestPosition = position + maxDirection
			Vector3 closestPosition = position + (Vector3.Normalize(_position - position) * range);

            return Contains(closestPosition);
		}

		public void FindContainingGridElement(Vector3 position, ref GridElement outChild) {
			if (!Contains(position))
				return;
			
			if (_children.Count > 0)
				foreach (GridElement child in _children)
					child.FindContainingGridElement(position, ref outChild);
			else
				outChild = this;
		}

		public void GridElementsInRange(Vector3 position, float range, ref List<GridElement> path, ref List<GridElement> elements) {
			if (path.Contains(this))
				return;
			
			path.Add(this);
            
            if (!InRange(position, range))
				return;

            
			foreach (GridElement neighbour in _neighbours) {
				neighbour.GridElementsInRange(position, range, ref path, ref elements);
			}

            
			if (_children.Count > 0)
				foreach (GridElement child in _children)
					child.GridElementsInRange(position, range, ref path, ref elements);
			else
				elements.Add(this);
        }

        public List<T> GetChildrenInRange<T>(Vector3 position, float range) where T : SceneObject {
            List<T> output = new List<T>();

            foreach (SceneObject child in _objects)
                if (child is T)
                    if (Vector3.Distance(position, child.Position) < range)
                        output.Add(child as T);

            return output;
        }

        public List<T> GetChildren<T>() where T : SceneObject {
			List<T> output = new List<T>();

			foreach (SceneObject child in _objects)
				if (child is T)
					output.Add(child as T);

			return output;
		}


		public void Register(SceneObject child) {
            _objects.Add(child);
        }

        public void Unregister(SceneObject child) {
            _objects.Remove(child);
		}

        protected void RemoveElement(GridElement element) {
            int index = _children.IndexOf(element);

            if (index > -1)
                _children[index] = null;
        }

        public Vector3 Position {
            get { return _position; }
        }

        public Vector3 Size {
            get { return _halfSize * 2.0f; }
        }

		public bool Contains(Vector3 pos) {
			return _collider.bounds.Contains(pos);
		}

        protected void AddNeighbour(GridElement neighbour) {
            if (neighbour != this && !_neighbours.Contains(neighbour))
                _neighbours.Add(neighbour);
        }

        public void ConnectTo(GridElement neighbour) {
            AddNeighbour(neighbour);
            neighbour.AddNeighbour(this);
        }

        public void ConnectTo(List<GridElement> neighbours) {
            foreach (GridElement neighbour in neighbours)
                ConnectTo(neighbour);
        }

        public void DisconnectFrom(GridElement neighbour) {
            if (_neighbours.Contains(neighbour)) {
                neighbour.DisconnectFrom(this);
                _neighbours.Remove(neighbour);
            }
        }

        public void DisconnectFromAll() {
            foreach (GridElement neighbour in _neighbours)
                DisconnectFrom(neighbour);
        }
        
        // Generate a 3D grid
        // The parent element will be filled, so a 70x70x70 cube with all the below parameters set to "10" will create
        // 10x10x10 child cubes, each 7x7x7 in size
        public void GenerateUniformGrid(Dimensions[] dimensions) {
            if (dimensions.Length == 0)
                return;

            _childWidth = dimensions[0].width;
            _childHeight = dimensions[0].height;
            _childDepth = dimensions[0].depth;
            _children.Clear();

            Vector3 origin = _position  - _halfSize;
            Vector3 childScale = new Vector3(_halfSize.z * 2.0f / _childWidth, _halfSize.y * 2.0f / _childHeight, _halfSize.z * 2.0f / _childDepth);

			// Fill list with the right amount of new members
			for (int x = 0; x < _childWidth; x++)
				for (int y = 0; y < _childHeight; y++)
					for (int z = 0; z < _childDepth; z++)
						_children.Add(new GridElement(new Vector3(origin.x + childScale.x / 2.0f + x * childScale.x,
																  origin.y + childScale.y / 2.0f + y * childScale.y,
																  origin.z + childScale.z / 2.0f + z * childScale.z),
																  childScale,
																  this));

            List<GridElement> neighbours = new List<GridElement>();
            for (int x = 0; x < _childWidth; x++) {
                for (int y = 0; y < _childHeight; y++) {
                    for (int z = 0; z < _childDepth; z++) {
                        // Generate Neighbours (-1 -> +1 in all directions)
                        for (int xn = -1; xn < 2; xn++) {
                            if (x == 0 && xn == -1) continue;
                            if (x == _childWidth - 1 && xn == 1) continue;

                            for (int yn = -1; yn < 2; yn++) {
                                if (y == 0 && yn == -1) continue;
                                if (y == _childHeight - 1 && yn == 1) continue;

                                for (int zn = -1; zn < 2; zn++) {
                                    if (z == 0 && zn == -1) continue;
                                    if (z == _childDepth - 1 && zn == 1) continue;
                                    if (x == 0 && y == 0 && z == 0) continue;

                                    neighbours.Add(_children[Maths.To1D(x + xn, y + yn, z + zn, _childHeight, _childDepth)]);
                                }
                            }
                        }
                        // End Generate Neighbours

                        int index = Maths.To1D(x, y, z, _childHeight, _childDepth);
                        // Connect to generated neighbours
                        _children[index].ConnectTo(neighbours);
                        // Clear for neighbour list for next iteration
                        neighbours.Clear();

                        if (dimensions.Length > 1) {
                            _children[index].GenerateUniformGrid(dimensions.SubArray(1, dimensions.Length - 1));
                        }
                    }
                }
            }

            // Connect edge children
            if (dimensions.Length > 2) {
                int grandChildWidth = dimensions[1].width;
                int grandChildHeight = dimensions[1].height;
                int grandChildDepth = dimensions[1].depth;
                int index;
                int xPos, xNeg, yPos, yNeg, zPos, zNeg;

                int cxMin = 0, cyMin = 0, czMin = 0;
                int cxMax = grandChildWidth - 1, cyMax = grandChildHeight - 1, czMax = grandChildDepth - 1;

                for (int x = 0; x < _childWidth; x++) {
                    for (int y = 0; y < _childHeight; y++) {
                        for (int z = 0; z < _childDepth; z++) {
                            index = Maths.To1D(x, y, z, _childHeight, _childDepth);

                            xPos = Maths.To1D(x + 1, y, z, _childHeight, _childDepth);
                            xNeg = Maths.To1D(x - 1, y, z, _childHeight, _childDepth);

                            for (int cy = 0; cy < grandChildHeight; cy++) {
                                for (int cz = 0; cz < grandChildDepth; cz++) {
                                    //if (x > 0)
                                    //    _children[index][Maths.To1D(cxMin, cy, cz, grandChildHeight, grandChildDepth)]
                                    //        .ConnectTo(_children[xNeg][Maths.To1D(cxMax, cy, cz, grandChildHeight, grandChildDepth)]);

                                    if (x < _childHeight - 1)
                                        _children[index]
                                            [Maths.To1D(cxMax, cy, cz, grandChildHeight, grandChildDepth)]
                                            .ConnectTo(_children[xPos][Maths.To1D(cxMin, cy, cz, grandChildHeight, grandChildDepth)]);
                                }
                            }

                            yPos = Maths.To1D(x, y + 1, z, _childHeight, _childDepth);
                            yNeg = Maths.To1D(x, y - 1, z, _childHeight, _childDepth);

                            for (int cx = 0; cx < grandChildWidth; cx++) {
                                for (int cz = 0; cz < grandChildDepth; cz++) {
                                    if (y < _childHeight - 1)
                                        _children[index]
                                            [Maths.To1D(cx, cyMax, cz, grandChildHeight, grandChildDepth)]
                                            .ConnectTo(_children[yPos][Maths.To1D(cx, cyMin, cz, grandChildHeight, grandChildDepth)]);
                                }
                            }

                            zPos = Maths.To1D(x, y, z + 1, _childHeight, _childDepth);
                            zNeg = Maths.To1D(x, y, z - 1, _childHeight, _childDepth);

                            for (int cx = 0; cx < grandChildWidth; cx++) {
                                for (int cy = 0; cy < grandChildHeight; cy++) {
                                    if (z < _childDepth - 1)
                                        _children[index]
                                            [Maths.To1D(cx, cy, czMax, grandChildHeight, grandChildDepth)]
                                            .ConnectTo(_children[zPos][Maths.To1D(cx, cy, czMin, grandChildHeight, grandChildDepth)]);
                                }
                            }


                        }
                    }
                }
            }
        }

        public List<GridElement> Elements {
            get { return _children; }
        }

        public int TestElement {
            get { return _testElement; }
            set { _testElement = value; }
        }

        public GridElement this[int i] {
            get { return _children[i]; }
            set { _children[i] = value; }
        }

        public SceneObject[] Objects {
            get { return _objects.ToArray(); }
        }
    }

    class GridManager {
        static GridManager _instance;
        List<GridElement> _grids;
        float _scale;

        private GridManager() {
            _grids = new List<GridElement>();
        }

        // The GridManager should be a singleton as it holds all the grids within it
        public static GridManager GetInstance() {
            if (_instance == null)
                _instance = new GridManager();
            return _instance;
		}

        public List<T> GetObjectsInRange<T>(SceneObject centreObject, float range) where T : SceneObject {
            List<T> objects = new List<T>();
            List<GridElement> path = new List<GridElement>();
            List<GridElement> elementsInRange = new List<GridElement>();

            centreObject.GridElement.GridElementsInRange(centreObject.Position, range, ref path, ref elementsInRange);

            foreach (GridElement element in elementsInRange)
                objects.AddRange(element.GetChildrenInRange<T>(centreObject.Position, range));

            return objects;
        }

		public List<T> GetObjectsInElements<T>(List<GridElement> elements) where T : SceneObject {
			List<T> objects = new List<T>();
			foreach (GridElement element in elements)
				objects.AddRange(element.GetChildren<T>());
                
			return objects;
		}

		public GridElement GetContainingGridElement(int gridID, Vector3 position) {
			GridElement element = null;
			
			if (_grids.Count > gridID)
				_grids[gridID].FindContainingGridElement(position, ref element);

			return element;
		}

		public void SetScale(float scale) {
            _scale = scale;
        }

        public void AddGrid(GridElement element) {
            _grids.Add(element);
        }

        public List<GridElement> Grids {
            get { return _grids; }
        }

        public GridElement this[int i] {
            get { return _grids[i]; }
            set { _grids[i] = value; }
        }
    }

    class Sensable {

    }

    class Sound : Sensable {

    }
    
    class Smell : Sensable {

    }
}
