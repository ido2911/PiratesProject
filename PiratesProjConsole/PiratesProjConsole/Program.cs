using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using C5;
using static System.Net.Mime.MediaTypeNames;

namespace PiratesProjConsole
{

    public class PirateShip
    {
        public int StartIsland { get; set; }
        public int Resources { get; set; }

        public PirateShip(int startIsland, int resources)
        {
            StartIsland = startIsland;
            Resources = resources;
        }

        public override string ToString()
        {
            return $"startIsland = {StartIsland}    resources = {Resources}";
        }
    }

    public class Island
    {
        public double pTresure { get; set; } // must be a number between 0 and 1
        public float Reward { get; set; } // the reward earned if you got the tresure
        public float Damage { get; set; } // the Damage from not getting any tresure
                                          // (losing additional resources)

        public Island(double pTresure, float reward, float damage)
        {
            this.pTresure = pTresure;
            Reward = reward;
            Damage = damage;
        }

        public void setAsStartIsland()
        {
            this.pTresure = 1;
            Reward = 0;
            Damage = 0;
        }

        public float ExpectedVal() // Expected val is the expected reward from
                                   // visiting an island
        {
            return Reward * (float)pTresure - Damage * (float)(1 -pTresure);
        }

        public override string ToString()
        {
            return $"pTresure = {pTresure}    reward = {Reward}    damage = {Damage}";
        }

    }

    internal class Program
    {
        //todo: add seperation and multy tasking algorthms
        private const int MAX_COST              = 41;
        private const double CONNECTION_CHANCE  = 0.5;
        private const int MIN_TRESURE           = 10;
        private const int MAX_TRESURE           = 10001;
        private const int MIN_DAMAGE            = 10;
        private const int MAX_DAMAGE            = 10001;
        private const int MIN_RESOURCES         = 100;
        private const int MAX_RESOURCES         = 1000;
        private const int MAX_SPLIT             = 4;
        private const double COST_CONST         = 0.0;
        private const double MIN_PT             = 0.05;

        static float FindBestShipsPath(int[,] map, List<Island> islands, List<PirateShip> pirateShips)
        {
            int islandsNum = map.GetLength(0);
            float totalTresure = 0;
            Dictionary<int, List<int>> shipAreas = partion_ships();

            #region orgenize_ships

            Dictionary <int, List<int>> partion_ships()
            {

                Dictionary<int, List<int>> areas = new Dictionary<int, List<int>>();
                int shipsNum = pirateShips.Count;

                //innit each list
                for (int ship = 0; ship < shipsNum; ship++)
                    areas.Add(ship, new List<int>());

                // split according to resource

                int totalResources = 0;
                for (int ship = 0; ship < shipsNum; ship++)
                    totalResources += pirateShips[ship].Resources;

                int[] maxIslandsCount = new int[shipsNum];
                for (int ship = 0;ship < shipsNum; ship++)
                {
                    maxIslandsCount[ship] = (int)Math.Ceiling(islandsNum * ((float)pirateShips[ship].Resources/totalResources));
                }

                int[,] distances = new int[shipsNum, islandsNum];

                for (int i = 0; i < shipsNum; i++)
                {
                    int[] distance = dijkstra(pirateShips[i].StartIsland);
                    for (int j = 0; j < distance.Length; j++)
                    {
                        distances[i, j] = distance[j];
                    }
                }

                // it cant differenciate between C5 and system libreries so I have to do this
                // or rename one but I think it more understandable like this
                System.Collections.Generic.HashSet<int> firstIslands = new System.Collections.Generic.HashSet<int>();

                // to make sure the first Island will be in the routes we will add them first
                // we will use a hash set that will contain the islands to check if we added them in O(1)
                for(int i = 0;i<shipsNum;i++)
                {
                    areas[i].Add(pirateShips[i].StartIsland);
                    firstIslands.Add(pirateShips[i].StartIsland);
                }
                

                // compare which is the minimum distance from the island and add it
                // to the islands that pirate can travel to
                for(int island = 0; island< islandsNum; island++)
                {
                    if (firstIslands.Contains(island)) continue;

                    //find min island distance
                    int min = int.MaxValue;
                    int minShip = int.MaxValue;

                    // select the minimus distance and add it to the areas of the pirate
                    for (int ship = 0; ship< shipsNum; ship++)
                    {
                        
                        if ((min > distances[ship, island]) && 
                            (areas[ship].Count < maxIslandsCount[ship]))
                        {
                            min = distances[ship, island];
                            minShip = ship;
                        }
                    }

                    areas[minShip].Add(island);
                }

                return areas;

            }

            //partion ship by distance and weight
            int[] dijkstra(int firstIsland)
            {
                int numOfIsland = map.GetLength(0);
                int[] distances = new int[numOfIsland];
                bool[] visited = new bool[numOfIsland];

                for (int i = 0; i < distances.Length; i++)
                    distances[i] = int.MaxValue; //big value that shoudn't be reached

                distances[firstIsland] = 0;

                //Priority Queue has the distance and the node in it
                IPriorityQueue<Tuple<int, int>> islansPQ = new IntervalHeap<Tuple<int, int>>();
                islansPQ.Add(Tuple.Create(0, firstIsland));

                while (!islansPQ.IsEmpty)
                {

                    Tuple<int, int> current = islansPQ.DeleteMin();
                    int currDistance = current.Item1;
                    int currIsland = current.Item2;

                    if (visited[currIsland]) continue;

                    visited[currIsland] = true;

                    for (int neighbor = 0; neighbor < numOfIsland; neighbor++)
                    {
                        if (map[currIsland, neighbor] == int.MaxValue || visited[neighbor]) continue;

                        int newDistance = currDistance + map[currIsland, neighbor];

                        if (newDistance < distances[neighbor])
                        {
                            distances[neighbor] = newDistance;
                            islansPQ.Add(Tuple.Create<int, int>(newDistance, neighbor));
                        }
                    }
                }

                return distances;
            }


            float find_best_path(List<int> pirateIslands, PirateShip pirate, List<int> path)
            {

                int islandNum = map.GetLength(0);
                bool[] visited = new bool[islandsNum];
                // a list tp track the path I am going through
                List<int> dfsPath = new List<int>();
                visited[pirate.StartIsland] = true;
                float maxTresure = float.MinValue;

                #region inline

                bool island_is_worth_visiting(int neighbor, int currIsland)
                {
                    int cost = map[currIsland, neighbor];
                    Island neighborIsland = islands[neighbor];
                    double expected = neighborIsland.ExpectedVal() - COST_CONST * cost;

                    return (cost != int.MaxValue && cost > 0 && !visited[neighbor])
                            && neighborIsland.pTresure > MIN_PT
                            && ((neighborIsland.ExpectedVal() - COST_CONST*cost) >= 0)
                    ;
                }

                // will return the best path via call by refrence
                float dfs(float collectedTresure, int currIsland, int resources, List<int> currentPath, ref List<int> bestPath)
                {

                    visited[currIsland] = true;
                    Island island = islands[currIsland];

                    // add to current path
                    currentPath.Add(currIsland);

                    if (resources <= 0 || pirateIslands.Count == currentPath.Count)
                    {
                        if (collectedTresure > maxTresure)
                        {
                            maxTresure = collectedTresure;
                            
                            // copy the path to best path
                            bestPath.Clear();
                            bestPath.AddRange(currentPath);
                        }

                        currentPath.RemoveAt(currentPath.Count - 1);
                        visited[currIsland] = false;

                        return maxTresure;
                    }

                    bool continuedSearch = false;

                    foreach (int neighbor in pirateIslands)
                    {
                        //check if the island is worth visiting
                        if (island_is_worth_visiting(neighbor, currIsland))
                        {
                            continuedSearch = true;
                            //Continue DFS check
                            dfs(
                                collectedTresure + island.ExpectedVal(),
                                neighbor,
                                resources - map[currIsland, neighbor],
                                currentPath,
                                ref path
                            );

                        }
                    }

                    if(!continuedSearch)
                    {
                        if (collectedTresure > maxTresure)
                        {
                            maxTresure = collectedTresure;

                            // copy the path to best path
                            bestPath.Clear();
                            bestPath.AddRange(currentPath);
                        }
                    }
                    
                    //after check, go check a diffrent path and see if it better
                    currentPath.RemoveAt(currentPath.Count - 1);
                    visited[currIsland] = false;

                    return maxTresure;

                }
                #endregion


                return dfs(islands[pirate.StartIsland].ExpectedVal(), pirate.StartIsland, pirate.Resources,dfsPath ,ref path);
            }
            #endregion


            for (int i = 0; i < shipAreas.Count; i++)
            {
                Console.WriteLine(i + ": ");
                foreach (int island in shipAreas[i])
                {
                    Console.Write($"{island}, ");
                }
                Console.WriteLine();
            }

            List<int>[] paths = new List<int>[pirateShips.Count];
            // innit each path list
            for(int i = 0; i < pirateShips.Count; i++) paths[i] = new List<int>();

            // find the best path for each ship
            for (int ship = 0;ship <pirateShips.Count; ship++)
            {
                paths[ship].Add(pirateShips[ship].StartIsland);
                //**debug** get Estemated value
                 totalTresure += 
                find_best_path(shipAreas[ship], pirateShips[ship], paths[ship]);
            }

            //**debug** print best paths
            Console.WriteLine("best paths");
            for(int i = 0;i < pirateShips.Count();i++)
            {
                Console.WriteLine("ship " + i + ": ");
                foreach (int island in paths[i])
                {
                    Console.Write(island + ", ");
                }
                Console.WriteLine();   
            }

            Console.WriteLine("expected value: "+totalTresure);
            totalTresure = 0;

            //run through the path
            for(int ship = 0; ship < pirateShips.Count; ship++)
            {
                foreach (int island in paths[ship])
                {

                    Island currIsland = islands[island];
                    Random rand = new Random();
                    double chanceOfTresure = rand.NextDouble();


                    // simulates chance of tresure
                    if (chanceOfTresure > currIsland.pTresure)
                        totalTresure -= currIsland.Damage;
                    else
                        totalTresure += currIsland.Reward;

                }
            }

            
            return totalTresure;

        }

        static void Main(string[] args)
        {
            int generatedPiratesNum = 4;
            int generatedIslandsNum = 20;

            //List of islands by index
            List<Island> islands = generate_islands(generatedIslandsNum);
            //**debug**
            /*new List<Island>
            {
            new Island(0.5,50,0),
            new Island(0.1,100,3),
            new Island(0.5,50,50),
            new Island(0.7,65,50),
            new Island(0.3,80,1),
            new Island(0.2,100,20),
            new Island(0.01,10000,100),
            new Island(0.8,1,10),
            new Island(0.9,70,50),
            new Island(0.45,80,81),
            new Island(0.5,50,0),
            new Island(0.1,100,3),
            new Island(0.5,50,50),
            new Island(0.7,65,50),
            new Island(0.3,80,1),
            new Island(0.2,100,20),
            new Island(0.01,10000,100),
            new Island(0.8,1,10),
            new Island(0.9,70,50),
            new Island(0.45,80,81)
            };*/

            //**debug**
            Console.WriteLine("generated islands: ");
            for (int i = 0; i < generatedIslandsNum; i++)
            {
                Console.WriteLine($"island {format_num(i)}: {islands[i]}");
            }

            List<PirateShip> pirateTeam = generate_pirates(generatedPiratesNum, generatedIslandsNum);
            //**debug**
            /*new List<PirateShip>
            {
            new PirateShip(0,100),
            new PirateShip(3,500),
            new PirateShip(4,500),
            new PirateShip(7,1000),
            };*/

            #region generate_data
            int[,] generate_map(int dimentions)
            {
                int[,] generatedMap = new int[dimentions, dimentions];
                Random rand = new Random();

                // will go under the main diagnal line
                for (int i = 1; i < dimentions; i++)
                {
                    bool hasConnection = false;
                    for(int j = 0; j< i; j++)
                    {
                        double conectionChance = rand.NextDouble();

                        // if chance > CONNACTION_CHANC (ex 0.5) it means there is a connction
                        if (conectionChance > CONNECTION_CHANCE)
                        {
                            //random cost
                            int randCost = rand.Next(1, MAX_COST);
                            generatedMap[i, j] = randCost;
                            generatedMap[j, i] = randCost;
                            hasConnection = true;
                        }
                        // no connection was made
                        else
                        {
                            // max value defines no connection
                            generatedMap[i, j] = int.MaxValue;
                            generatedMap[j, i] = int.MaxValue;
                        }
                    }

                    // in case no connection was made (low but possible)
                    if (!hasConnection)
                    {
                        //make a random connection
                        int randIsle = rand.Next(0, dimentions);
                        while(randIsle == i) randIsle = rand.Next(0, dimentions);
                        int randCost = rand.Next(1, MAX_COST);  
                        generatedMap[i, randIsle] = randCost;
                        generatedMap[randIsle, i] = randCost;
                    }
                }

                return generatedMap;

            }

            List<Island> generate_islands(int islandsNum)
            {
                List<Island> list = new List<Island>();

                Random rand = new Random();
                for (int i = 0; i < islandsNum; i++)
                {
                    double randPtresure = rand.NextDouble();
                    int randomReward = rand.Next(MIN_TRESURE,MAX_TRESURE);
                    int randomDamage = rand.Next(MIN_DAMAGE, MAX_DAMAGE);
                    list.Add(new Island(randPtresure, randomReward, randomDamage));
                }

                return list;

            }

            List<PirateShip> generate_pirates(int piratesNum, int islandsNum)
            {
                if (piratesNum > islandsNum/MAX_SPLIT)
                {
                    //todo make sure not a lot of pirates are made
                    return null;
                }


                List<PirateShip> list = new List<PirateShip>();
                bool[] arcupied = new bool[islandsNum];
                Random rand = new Random();

                for (int i = 0;i < piratesNum;i++)
                {
                    int randStartIsland = rand.Next(0, islandsNum);
                    while (arcupied[randStartIsland]) randStartIsland = rand.Next(0, islandsNum);
                    arcupied[randStartIsland] = true;
                    int randResources = rand.Next(MIN_RESOURCES, MAX_RESOURCES);
                    list.Add(new PirateShip(randStartIsland, randResources));
                }

                for(int ship =  0; ship < piratesNum;ship++)
                {
                    islands[list[ship].StartIsland].setAsStartIsland();
                }

                return list;
            }
            #endregion


            //**debug**
            Console.WriteLine("\ngenerated pirates: ");
            for (int i = 0; i < generatedPiratesNum; i++)
            {
                Console.WriteLine($"pirate {format_num(i)}: {pirateTeam[i]}");
            }

            string format_num(int num)
            {
                if (num < 10) return "0" + num;
                else return num.ToString();
            }

            //**debug**
            for(int k = 0; k < 200; k++)
            {
                Console.WriteLine();
                int[,] map = generate_map(generatedIslandsNum);
                for (int i = 0; i < map.GetLength(0); i++)
                {
                    for (int j = 0; j < map.GetLength(1); j++)
                    {
                        if (map[i, j] == int.MaxValue)
                        {
                            Console.Write("NA, ");
                            continue;
                        }
                        Console.Write(format_num(map[i, j]) + ", ");
                    }
                    Console.WriteLine();
                }

                float collectedTresure = FindBestShipsPath(map, islands, pirateTeam);
                Console.Write("tresure found: "+collectedTresure);
                if (collectedTresure < 0)
                {
                    Console.WriteLine(" unlucky day :(");
                }

                // **debug**
                Console.ReadKey();
                //Thread.Sleep(1000);
            }
        }
    }
}
