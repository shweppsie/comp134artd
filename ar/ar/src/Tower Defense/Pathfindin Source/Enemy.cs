using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;

namespace TowerDefence3D
{

    public class Enemy
    {
        #region VARIABLE DECLARATIONS
        private Vector2 positionCurrent;
        private Vector2 positionStart;
        private Vector2 positionDestination;

        private enum MovingType
        {
            None,
            Linear,
            PathSmooth
        };

        private MovingType movingStatus = MovingType.None;

        private static float moveSpeed = 20.0f;
        private float time = 0.0f;
        private float totalTime = 0.0f;

        private float MoveDistance;

        //Position on map regarding to path
        private Point currentPosOnPath;
        private Point prewPosOnPath;
        private Point startPosOnPath;
        private Point endPosOnPath;

        //index in path nodes list
        private int NodePos;

        //Moving Direction
        private float direction = 0.0f;

        //Directions number
        private int DirFrom = 0;
        private int DirTo = 0;

        //Curves for smooth path movement
        public Curve curveX;
        public Curve curveY;

        //Width of one path tile
        private static int GridWidth;
        //GridWidth/2
        private static int GridWidthDiv2;

        //Array containing directions lookuos
        private static int[] Directions;
        //Array for relative position(regarding to directions) lookup
        private static Point[] LinearMoves;

        //Stuff for the enemy of td
        public const float MAX_HP = 10;
        public int hp;
        public bool alive;
        public bool finished;

        public Matrix matrix;
        #endregion

        #region CONSTRUCTOR
        public Enemy(Vector2 StartPos)
        {
            positionCurrent = StartPos;
        
            curveX = new Curve();
            curveY = new Curve();

            matrix = Matrix.CreateScale(4) * Matrix.CreateTranslation(positionCurrent.X, positionCurrent.Y, 4.0f);

            hp = 20;
            alive = true;
            finished = false;
        }
        #endregion

        #region PROPERTIES
        public Vector2 PositionCurrent
        {
            get { return positionCurrent; }
            set { positionCurrent = value; }
        }
        public float MoveSpeed
        {
            get { return moveSpeed; }
            set { moveSpeed = value; }
        }
        public float Direction
        {
            get { return direction; }
            set { direction = value; }
        }

        #endregion

        /// <summary>
        /// Init MovableObject class
        /// </summary>
        /// <param name="GridSize">Dimensions of provided grid</param>
        /// <param name="MoveWidth">Size of one grid tile</param>
        public static void Initalize(int GridSize, int MoveWidth)
        {
            GridWidth = MoveWidth;
            GridWidthDiv2 = MoveWidth / 2;

            //Lookaps for direction and node insertion 
            Directions = new int[8];
            Directions[0] = -GridSize;
            Directions[1] = -GridSize + 1;
            Directions[2] = 1;
            Directions[3] = GridSize + 1;
            Directions[4] = GridSize;
            Directions[5] = GridSize - 1;
            Directions[6] = -1;
            Directions[7] = -GridSize - 1;

            LinearMoves = new Point[8];
            LinearMoves[0] = new Point(GridWidthDiv2, GridWidth);
            LinearMoves[1] = new Point(0, GridWidth);
            LinearMoves[2] = new Point(0, GridWidthDiv2);
            LinearMoves[3] = new Point(0, 0);
            LinearMoves[4] = new Point(GridWidthDiv2, 0);
            LinearMoves[5] = new Point(GridWidth, 0);
            LinearMoves[6] = new Point(GridWidth, GridWidthDiv2);
            LinearMoves[7] = new Point(GridWidth, GridWidth);

        }

        /// <summary>
        /// Stop moving and reset times
        /// </summary>
        public void StopMoving()
        {
            movingStatus = MovingType.None;
            time = 0.0f;
            totalTime = 0.0f;
            if (currentPosOnPath == endPosOnPath)
            {
                finished = true;
            }
        }

        /// <summary>
        /// Perform a linear move
        /// </summary>
        /// <param name="startPos">Start position</param>
        /// <param name="endPos">End Position</param>
        public void LinearMove(Vector2 startPos, Vector2 endPos)
        {
            movingStatus = MovingType.Linear;
            time = 0.0f;
            positionStart = startPos;
            positionDestination = endPos;
            Vector2.Distance(ref positionStart, ref positionDestination, out MoveDistance);
            direction = (float)Math.Atan2(endPos.Y - startPos.Y,endPos.X - startPos.X);
        }

        /// <summary>
        /// Perform move on found path
        /// </summary>
        /// <param name="Nodes">List of found path nodes</param>
        /// <param name="StartPosition">Point from where to start moving</param>
        /// <param name="EndPosition">Moving end point</param>
        public void PathMove(ref List<PathReturnNode> Nodes, Vector2 StartPosition, Vector2 EndPosition)
        {
            //Movement is on curve base - points are added in XNA Curve form given Path
            //Here, a direction based algorithm is used - that gives us more smooth movement, you could also 
            //drop that and just add new points to curve's just by node centers

            movingStatus = MovingType.PathSmooth;
            
            //Clear keys...
            curveX.Keys.Clear();
            curveY.Keys.Clear();

            NodePos = Nodes.Count - 1;

            //This needed to get directions
            startPosOnPath = new Point(Nodes[NodePos].PosX, Nodes[NodePos].PosY);
            prewPosOnPath = startPosOnPath;
            currentPosOnPath = startPosOnPath;
            endPosOnPath = new Point(Nodes[0].PosX, Nodes[0].PosY);

            DirFrom = GetDirection(NodePos, ref Nodes);

            CurveKey key;

            //Loop throught all nodes (They are in reverse order, so we move bacwards form last to first
            for (int n = NodePos - 1; n >= 0; n -= 1)
            {
                //Prew direction
                DirTo = DirFrom;
                //Get new direction
                DirFrom = GetDirection(n, ref Nodes);

                //Add nodes in curves, LinearMoves array and DirTo are used to get the rigt spot
                key = new CurveKey((NodePos - n) * GridWidth, currentPosOnPath.X * GridWidth + LinearMoves[DirTo].X);
                curveX.Keys.Add(key);
                key = new CurveKey((NodePos - n) * GridWidth, currentPosOnPath.Y * GridWidth + LinearMoves[DirTo].Y);
                curveY.Keys.Add(key);

                prewPosOnPath = currentPosOnPath;
                currentPosOnPath = new Point(Nodes[n].PosX, Nodes[n].PosY);
            }


            //First and last nodes are inserted and are not path node based
            //We should get distance form first to second node to insert in in right time
            float firstDist = GridWidth - Vector2.Distance(StartPosition, new Vector2(curveX.Keys[0].Value, curveY.Keys[0].Value));

            key = new CurveKey(firstDist, StartPosition.X);
            curveX.Keys.Add(key);
            key = new CurveKey(firstDist, StartPosition.Y);
            curveY.Keys.Add(key);

            //The same goes for last node
            float secondDist = Vector2.Distance(EndPosition, new Vector2(curveX.Keys[curveX.Keys.Count - 1].Value, curveY.Keys[curveY.Keys.Count - 1].Value)) - GridWidth;

            key = new CurveKey(Nodes.Count * GridWidth + secondDist, EndPosition.X);
            curveX.Keys.Add(key);
            key = new CurveKey(Nodes.Count * GridWidth + secondDist, EndPosition.Y);
            curveY.Keys.Add(key);

            if (NodePos > 2)
            {
                curveX.Keys.RemoveAt(1);
                curveX.Keys.RemoveAt(curveX.Keys.Count - 2);
                curveY.Keys.RemoveAt(1);
                curveY.Keys.RemoveAt(curveY.Keys.Count - 2);
            }

            //Compute tangents for both curves
            curveX.ComputeTangents(CurveTangent.Smooth);
            curveY.ComputeTangents(CurveTangent.Smooth);

            //Set start and end positions on curve, used in Update
            time = firstDist;
            totalTime = Nodes.Count * GridWidth + secondDist;
        }

        /// <summary>
        /// Performs moving logic
        /// </summary>
        /// <param name="ElapsedTime">Time elapsed form last frame</param>
        public void Update(float ElapsedTime)
        {            
            switch (movingStatus)
            {
                case MovingType.Linear:
                    {
                        positionCurrent = Linear(positionStart, positionDestination, time);
                        time += (moveSpeed * ElapsedTime) / MoveDistance;
                        if (time >= 1.0f)
                        {
                            StopMoving();
                            positionCurrent = positionDestination;
                        }
                    }
                    matrix = Matrix.CreateScale(4) * Matrix.CreateTranslation(positionCurrent.X, positionCurrent.Y, 25f);
                    break;
                case MovingType.PathSmooth:
                    {
                        time += (moveSpeed * ElapsedTime);
                        if (time < totalTime)
                        {
                            float newX = curveX.Evaluate(time);
                            float newY = curveY.Evaluate(time);

                            //Trought not used here, this provide moving direction (in radions)
                            direction = (float)Math.Atan2(newY - positionCurrent.Y,  newX-positionCurrent.X);

                            positionCurrent.X = newX;
                            positionCurrent.Y = newY;
                        }
                        else
                        {
                            StopMoving();
                        }
                    }
                    matrix = Matrix.CreateScale(4) * Matrix.CreateTranslation(positionCurrent.X, positionCurrent.Y, 5f);
                    break;
            }
        }

        //Get direction, where to add next path point
        private int GetDirection(int nodePosition, ref List<PathReturnNode> Nodes)
        {
            if (nodePosition > 0)
            {
                int difference = (int)Nodes[nodePosition].Pos - (int)Nodes[nodePosition - 1].Pos;
                int direction = 0;

                for (int i = 0; i < 8; i += 1)
                {
                    if (Directions[i] == difference)
                    {
                        direction = i;
                        break;
                    }
                }

                return direction;
            }
            else
            {
                return DirFrom;
            }
        }

        //Return 2D Postion on line form start and end point at given time
        private static Vector2 Linear(Vector2 startPos, Vector2 endPos, float time)
        {
            return startPos = startPos + (endPos - startPos) * time;
        }

    }
}
