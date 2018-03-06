using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
    public class MontyHallMC : MonteCarlo<MontyHallResult,MontyHallInputData,object>
    {

        private Random winningDoorGenerator;
        private Random userFirstChoiseGenerator;
        private Random doorToOpenGeneratorGoodTip;
        private Random doorToOpenGeneratorBadTip;
        private Random userChangedChoiseGenerator;

        private int winningChoise;
        private int userFirstChoise;
        private int userChangedChoise;
        private int openedDoors;

        private List<int> doors;

        public MontyHallMC(MontyHallInputData inputData, long replications = 1000000, long? resultInterval = null) : base(inputData, replications, resultInterval)
        {
        }

        public long FirstChoiseWinning { get; set; } = 0;
        public long ChangedChoiseWinning { get; set; } = 0;

                      

        public override void Inicialization()
        {
            var seedRandom = new Random();
            winningDoorGenerator = new Random(seedRandom.Next());
            userFirstChoiseGenerator = new Random(seedRandom.Next());
            doorToOpenGeneratorGoodTip = new Random(seedRandom.Next());
            doorToOpenGeneratorBadTip = new Random(seedRandom.Next());
            userChangedChoiseGenerator = new Random(seedRandom.Next());

            doors = Enumerable.Range(0, InputData.DoorsCount).ToList();
        }

        public override MontyHallResult Experiment(long replication)
        {
            winningChoise = doors[winningDoorGenerator.Next(InputData.DoorsCount)];
            userFirstChoise = doors[userFirstChoiseGenerator.Next(InputData.DoorsCount)];

            doors.Remove(winningChoise);
            doors.Remove(userFirstChoise);
            
            openedDoors = doors[ winningChoise == userFirstChoise ? doorToOpenGeneratorGoodTip.Next(doors.Count) : doorToOpenGeneratorBadTip.Next(doors.Count) ];

            doors.Remove(openedDoors);
            doors.Add(winningChoise);

            userChangedChoise = doors[userChangedChoiseGenerator.Next(doors.Count)];

            //for next replication
            doors.Add(openedDoors);
            if (userFirstChoise != winningChoise) doors.Add(userFirstChoise);
            
            if (userFirstChoise == winningChoise) FirstChoiseWinning++;
            else if (userChangedChoise == winningChoise) ChangedChoiseWinning++;

            return new MontyHallResult()
            {
                FirstChoiseWinningPropability = ((double)FirstChoiseWinning / replication) * 100,
                ChangedChoiseWinningPropability = ((double)ChangedChoiseWinning / replication) * 100,
            };
        }

        public override object AfterExperiment()
        {
            return null;
        }
    }    

    public class MontyHallInputData
    {
        public int DoorsCount { get; set; }
    }

    public class MontyHallResult
    {
        public double FirstChoiseWinningPropability { get; set; }
        public double ChangedChoiseWinningPropability { get; set; }
    }


}
