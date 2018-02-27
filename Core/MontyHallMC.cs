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
        private Random userChoiseGenerator;
        private Random doorToOpenGenerator;

        private int winningChoise;
        private int userFirstChoise;
        private int userChangedChoise;
        private int openedDoors;

        public long FirstChoiseWinning { get; set; } = 0;
        public long ChangedChoiseWinning { get; set; } = 0;

        public MontyHallMC(MontyHallInputData inputData, long replications = 1000000, long? resultInterval = null) : base(inputData, replications, resultInterval)
        {
        }               

        public override void Inicialization()
        {
            var seedRandom = new Random();
            winningDoorGenerator = new Random(seedRandom.Next());
            userChoiseGenerator = new Random(seedRandom.Next());
            doorToOpenGenerator = new Random(seedRandom.Next());
        }

        public override MontyHallResult Experiment(long replication)
        {
            winningChoise = winningDoorGenerator.Next(InputData.DoorsCount);
            userFirstChoise = userChoiseGenerator.Next(InputData.DoorsCount);

            do
            {
                openedDoors = doorToOpenGenerator.Next(InputData.DoorsCount);
            } while (openedDoors == winningChoise || openedDoors == userFirstChoise);

            do
            {
                userChangedChoise = userChoiseGenerator.Next(InputData.DoorsCount);
            } while (userChangedChoise == userFirstChoise || userChangedChoise == openedDoors);

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
