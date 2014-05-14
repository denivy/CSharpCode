using System;
using System.Collections.Generic;
using System.IO;

class OilWell
{
    static void Main(String[] args)
    {
        System.IO.StreamReader file = new System.IO.StreamReader(@"C:\Users\Trey\Documents\Visual Studio 2013\Projects\HackerRank\C#\HackerRank\HackerRank\oilWell-7.txt");
        Console.SetIn(file);

        int width, height, x1, x2, y1, y2;

        Tuple<int, int> cords;
        List<Tuple<int, int>> wells = new List<Tuple<int, int>>();

        string[] line = Console.ReadLine().Split();
        height = int.Parse(line[0]);
        width = int.Parse(line[1]);
        int[,] plot = new int[height, width];

        for (int i = 0; i < height; i++)
        {
            string[] rowStr = Console.ReadLine().Split();
            for (int x = 0; x < width; x++)
            {
                plot[i, x] = int.Parse(rowStr[x]);
                if (plot[i, x] == 1)
                {
                    cords = new Tuple<int, int>(i + 1, x + 1);
                    wells.Add(cords);
                }//end if
            }//end for x
        }//end for  i
        try
        {
            CitiesData citiesData = new CitiesData(wells);
            int totalNumberBees = 100;
            int numberInactive = 20;
            int numberActive = 50;
            int numberScout = 30;
            int maxNumberVisits = 100;//
            int maxNumberCycles = 3460;
            Hive hive = new Hive(totalNumberBees, numberInactive, numberActive, numberScout, maxNumberVisits, maxNumberCycles, citiesData);
            hive.Solve();
            int Answer = 0;
            for (int i = 0; i < hive.bestMemoryMatrix.Length - 1; ++i)
            {
                var city1 = hive.bestMemoryMatrix[i];
                var city2 = hive.bestMemoryMatrix[i + 1];

                x1 = city1.Item1;
                y1 = city1.Item2;
                x2 = city2.Item1;
                y2 = city2.Item2;

                int deltaX = Math.Abs(x2 - x1);
                int deltaY = Math.Abs(y2 - y1);
                int thisLength = Math.Max(deltaX, deltaY);
                Answer += thisLength;
                //Console.WriteLine("city1={0}, city2={1}, deltaX={2}, deltaY={3}, thisLength={4}, Answer={5}", city1, city2, deltaX, deltaY, thisLength, Answer);
            }//end for
            Console.WriteLine("wells.Count=" + wells.Count);
            Console.WriteLine("answer=" + Answer);
        }//end try
        catch (Exception ex)
        {
            Console.WriteLine("Fatal: " + ex.Message);
            Console.ReadLine();
        }//end catch
        file.Close();
    }//end main
    class Hive
    {
        public class Bee
        {
            public int status; // 0 = inactive, 1 = active, 2 = scout
            public Tuple<int, int>[] memoryMatrix;
            public double measureOfQuality; // smaller values are better. total dist of path.
            public int numberOfVisits;

            public Bee(int status, Tuple<int, int>[] memoryMatrix, double measureOfQuality, int numberOfVisits)
            {
                this.status = status;
                this.memoryMatrix = new Tuple<int, int>[memoryMatrix.Length];
                Array.Copy(memoryMatrix, this.memoryMatrix, memoryMatrix.Length);
                this.measureOfQuality = measureOfQuality;
                this.numberOfVisits = numberOfVisits;
            }//end Bee constructor
        } // Bee

        static Random random = null; // multipurpose

        public CitiesData citiesData; // this is the problem-specific data we want to optimize

        public int totalNumberBees; // mostly for readability in the object constructor call
        public int numberInactive;
        public int numberActive;
        public int numberScout;
        public int maxNumberCycles; // one cycle represents an action by all bees in the hive
        public int maxNumberVisits; // max number of times bee will visit a given food source without finding a better neighbor
        public double probPersuasion = 0.90; // probability inactive bee is persuaded by better waggle solution
        public double probMistake = 0.01; // probability an active bee will reject a better neighbor food source OR accept worse neighbor food source

        public Bee[] bees;
        public Tuple<int, int>[] bestMemoryMatrix; // problem-specific
        public double bestMeasureOfQuality;
        public int[] indexesOfInactiveBees; // contains indexes into the bees array

        public Hive(int totalNumberBees, int numberInactive, int numberActive, int numberScout, int maxNumberVisits,
        int maxNumberCycles, CitiesData citiesData)
        {
            Int32 zero = 0;
            random = new Random(zero);

            this.totalNumberBees = totalNumberBees;
            this.numberInactive = numberInactive;
            this.numberActive = numberActive;
            this.numberScout = numberScout;
            this.maxNumberVisits = maxNumberVisits;
            this.maxNumberCycles = maxNumberCycles;
            this.citiesData = citiesData; // reference to CityData
            this.bees = new Bee[totalNumberBees];
            this.bestMemoryMatrix = GenerateRandomMemoryMatrix(); // alternative initializations are possible
            this.bestMeasureOfQuality = MeasureOfQuality(this.bestMemoryMatrix);
            this.indexesOfInactiveBees = new int[numberInactive]; // indexes of bees which are currently inactive

            for (int i = 0; i < totalNumberBees; ++i) // initialize each bee, and best solution
            {
                int currStatus; // depends on i. need status before we can initialize Bee
                if (i < numberInactive)
                {
                    currStatus = 0; // inactive
                    indexesOfInactiveBees[i] = i; // curr bee is inactive
                }//end if 
                else if (i < numberInactive + numberScout)
                {
                    currStatus = 2; // scout
                }//end else if
                else
                {
                    currStatus = 1; // active
                }//end else

                Tuple<int, int>[] randomMemoryMatrix = GenerateRandomMemoryMatrix();
                double mq = MeasureOfQuality(randomMemoryMatrix);
                int numberOfVisits = 0;

                bees[i] = new Bee(currStatus, randomMemoryMatrix, mq, numberOfVisits); // instantiate current bee

                // does this bee have best solution?
                if (bees[i].measureOfQuality < bestMeasureOfQuality) // curr bee is better (< because smaller is better)
                {
                    Array.Copy(bees[i].memoryMatrix, this.bestMemoryMatrix, bees[i].memoryMatrix.Length);
                    this.bestMeasureOfQuality = bees[i].measureOfQuality;
                }//end if
            } // each bee

        } // TravelingSalesmanHive ctor

        public Tuple<int, int>[] GenerateRandomMemoryMatrix()
        {
            Tuple<int, int>[] result = new Tuple<int, int>[this.citiesData.cities.Length];
            Array.Copy(this.citiesData.cities, result, this.citiesData.cities.Length);

            for (int i = 0; i < result.Length; i++)
            {
                int r = random.Next(i, result.Length);
                Tuple<int, int> temp = result[r];
                result[r] = result[i];
                result[i] = temp;
            }
            return result;
        }
        public Tuple<int, int>[] GenerateNeighborMemoryMatrix(Tuple<int, int>[] memoryMatrix)
        {
            Tuple<int, int>[] result = new Tuple<int, int>[memoryMatrix.Length];
            Array.Copy(memoryMatrix, result, memoryMatrix.Length);

            int ranIndex = random.Next(0, result.Length);
            int adjIndex;
            if (ranIndex == result.Length - 1)
                adjIndex = 0;
            else
                adjIndex = ranIndex + 1;

            Tuple<int, int> temp = result[ranIndex];
            result[ranIndex] = result[adjIndex];
            result[adjIndex] = temp;

            return result;
        }
        public double MeasureOfQuality(Tuple<int, int>[] memoryMatrix)
        {
            double answer = 0.0;
            for (int i = 0; i < memoryMatrix.Length - 1; ++i)
            {
                Tuple<int, int> c1 = memoryMatrix[0];
                Tuple<int, int> c2 = memoryMatrix[i + 1];
                double d = this.citiesData.Distance(c1, c2);
                answer += d;
            }
            return answer;
        }
        public void Solve() // find best Traveling Salesman Problem solution
        {
            int cycle = 0;

            while (cycle < this.maxNumberCycles)
            {
                for (int i = 0; i < this.totalNumberBees; ++i) // each bee
                {
                    if (this.bees[i].status == 1) // active bee
                        ProcessActiveBee(i);
                    else if (this.bees[i].status == 2) // scout bee
                        ProcessScoutBee(i);
                    else if (this.bees[i].status == 0) // inactive bee
                        ProcessInactiveBee(i);
                } // for each bee
                ++cycle;
            } // main while processing loop
        } // Solve()

        private void ProcessInactiveBee(int i)
        {
            return; // not used in this implementation
        }

        private void ProcessActiveBee(int i)
        {
            Tuple<int, int>[] neighbor = GenerateNeighborMemoryMatrix(bees[i].memoryMatrix); // find a neighbor solution
            double neighborQuality = MeasureOfQuality(neighbor); // get its quality
            double prob = random.NextDouble(); // used to determine if bee makes a mistake; compare against probMistake which has some small value (~0.01)
            bool memoryWasUpdated = false; // used to determine if bee should perform a waggle dance when done
            bool numberOfVisitsOverLimit = false; // used to determine if bee will convert to inactive status

            if (neighborQuality < bees[i].measureOfQuality) // active bee found better neighbor (< because smaller values are better)
            {
                if (prob < probMistake) // bee makes mistake: rejects a better neighbor food source
                {
                    ++bees[i].numberOfVisits; // no change to memory but update number of visits
                    if (bees[i].numberOfVisits > maxNumberVisits) numberOfVisitsOverLimit = true;
                }
                else // bee does not make a mistake: accepts a better neighbor
                {
                    Array.Copy(neighbor, bees[i].memoryMatrix, neighbor.Length); // copy neighbor location into bee's memory
                    bees[i].measureOfQuality = neighborQuality; // update the quality
                    bees[i].numberOfVisits = 0; // reset counter
                    memoryWasUpdated = true; // so that this bee will do a waggle dance 
                }
            }
            else // active bee did not find a better neighbor
            {
                //Console.WriteLine("c");
                if (prob < probMistake) // bee makes mistake: accepts a worse neighbor food source
                {
                    Array.Copy(neighbor, bees[i].memoryMatrix, neighbor.Length); // copy neighbor location into bee's memory
                    bees[i].measureOfQuality = neighborQuality; // update the quality
                    bees[i].numberOfVisits = 0; // reset
                    memoryWasUpdated = true; // so that this bee will do a waggle dance 
                }
                else // no mistake: bee rejects worse food source
                {
                    ++bees[i].numberOfVisits;
                    if (bees[i].numberOfVisits > maxNumberVisits) numberOfVisitsOverLimit = true;
                }
            }

            // at this point we need to determine a.) if the number of visits has been exceeded in which case bee becomes inactive
            // or b.) memory was updated in which case check to see if new memory is a global best, and then bee does waggle dance
            // or c.) neither in which case nothing happens (bee just returns to hive).

            if (numberOfVisitsOverLimit == true)
            {
                bees[i].status = 0; // current active bee transitions to inactive
                bees[i].numberOfVisits = 0; // reset visits (and no change to this bees memory)
                int x = random.Next(numberInactive); // pick a random inactive bee. x is an index into a list, not a bee ID
                bees[indexesOfInactiveBees[x]].status = 1; // make it active
                indexesOfInactiveBees[x] = i; // record now-inactive bee 'i' in the inactive list
            }
            else if (memoryWasUpdated == true) // current bee returns and performs waggle dance
            {
                // first, determine if the new memory is a global best. note that if bee has accepted a worse food source this can't be true
                if (bees[i].measureOfQuality < this.bestMeasureOfQuality) // the modified bee's memory is a new global best (< because smaller is better)
                {
                    Array.Copy(bees[i].memoryMatrix, this.bestMemoryMatrix, bees[i].memoryMatrix.Length); // update global best memory
                    this.bestMeasureOfQuality = bees[i].measureOfQuality; // update global best quality
                }
                DoWaggleDance(i);
            }
            else // number visits is not over limit and memory was not updated so do nothing (return to hive but do not waggle)
            {
                return;
            }
        } // ProcessActiveBee()

        private void ProcessScoutBee(int i)
        {
            Tuple<int, int>[] randomFoodSource = GenerateRandomMemoryMatrix(); // scout bee finds a random food source. . . 
            double randomFoodSourceQuality = MeasureOfQuality(randomFoodSource); // and examines its quality
            if (randomFoodSourceQuality < bees[i].measureOfQuality) // scout bee has found a better solution than its current one (< because smaller measure is better)
            {
                Array.Copy(randomFoodSource, bees[i].memoryMatrix, randomFoodSource.Length); // unlike active bees, scout bees do not make mistakes
                bees[i].measureOfQuality = randomFoodSourceQuality;
                // no change to scout bee's numberOfVisits or status

                // did this scout bee find a better overall/global solution?
                if (bees[i].measureOfQuality < bestMeasureOfQuality) // yes, better overall solution (< because smaller is better)
                {
                    Array.Copy(bees[i].memoryMatrix, this.bestMemoryMatrix, bees[i].memoryMatrix.Length); // copy scout bee's memory to global best
                    this.bestMeasureOfQuality = bees[i].measureOfQuality;
                } // better overall solution

                DoWaggleDance(i); // scout returns to hive and does waggle dance

            } // if scout bee found better solution
        } // ProcessScoutBee()

        private void DoWaggleDance(int i)
        {
            for (int ii = 0; ii < numberInactive; ++ii) // each inactive/watcher bee
            {
                int b = indexesOfInactiveBees[ii]; // index of an inactive bee
                if (bees[b].status != 0) throw new Exception("Catastrophic logic error when scout bee waggles dances");
                if (bees[b].numberOfVisits != 0) throw new Exception("Found an inactive bee with numberOfVisits != 0 in Scout bee waggle dance routine");
                if (bees[i].measureOfQuality < bees[b].measureOfQuality) // scout bee has a better solution than current inactive/watcher bee (< because smaller is better)
                {
                    double p = random.NextDouble(); // will current inactive bee be persuaded by scout's waggle dance?
                    if (this.probPersuasion > p) // this inactive bee is persuaded by the scout (usually because probPersuasion is large, ~0.90)
                    {
                        Array.Copy(bees[i].memoryMatrix, bees[b].memoryMatrix, bees[i].memoryMatrix.Length);
                        bees[b].measureOfQuality = bees[i].measureOfQuality;
                    } // inactive bee has been persuaded
                } // scout bee has better solution than watcher/inactive bee
            } // each inactive bee
        } // DoWaggleDance()

    } // class ShortestPathHive

    class CitiesData
    {
        //public char[] cities;
        public Tuple<int, int>[] cities;
        public CitiesData(List<Tuple<int, int>> wells)
        {
            this.cities = wells.ToArray();
        }
        public double Distance(Tuple<int, int> firstCity, Tuple<int, int> secondCity)
        {
            int x1 = firstCity.Item1;
            int y1 = firstCity.Item2;
            int x2 = secondCity.Item1;
            int y2 = secondCity.Item2;

            int deltaX = Math.Abs(x2 - x1);
            int deltaY = Math.Abs(y2 - y1);

            return Math.Max(deltaX, deltaY);
        }
    } // class CitiesData

}//end clas OilWell
