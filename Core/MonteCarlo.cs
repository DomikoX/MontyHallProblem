using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
    public class MonteCarlo
    {

        public int DoorsCount { get; set; }
        public long Replications { get; set; }
        public int FirstChoiseWinning { get; set; } = 0;
        public int ChangedChoiseWinning { get; set; } = 0;

        public delegate void NewResult(long replications, decimal p);

        public NewResult FirstChoiceResult;
        public NewResult ChangedChoiceResult;


        public MonteCarlo(int doorsCount, long replications = 10000000)
        {
            this.DoorsCount = doorsCount;
            this.Replications = replications;
        }

        public Task RunExperiment()
        {
            return Task.Run(() =>
            {
                var every = Replications >= 1000 ? Replications /100 : Replications /10;

                var seedRandom = new Random();

                var winningDoorGenerator = new Random(seedRandom.Next());
                var userChoiseGenerator = new Random(seedRandom.Next());
                var doorToOpenGenerator = new Random(seedRandom.Next());

                int winningChoise, userFirstChoise, userChangedChoise, openedDoors;

                for (long i = 1; i <= Replications; i++)
                {
                    winningChoise = winningDoorGenerator.Next(DoorsCount);
                    
                    userFirstChoise = userChoiseGenerator.Next(DoorsCount);

                    do
                    {
                        openedDoors = doorToOpenGenerator.Next(DoorsCount);
                    } while (openedDoors == winningChoise || openedDoors == userFirstChoise);

                    do
                    {
                        userChangedChoise = userChoiseGenerator.Next(DoorsCount);
                    } while (userChangedChoise == userFirstChoise || userChangedChoise == openedDoors);

                    if (userFirstChoise == winningChoise) FirstChoiseWinning++;
                    else if (userChangedChoise == winningChoise) ChangedChoiseWinning++;

                    if (i % (every) == 1 || i == Replications)
                    {
                        FirstChoiceResult?.Invoke(i, ((decimal)FirstChoiseWinning / i) * 100);
                        ChangedChoiceResult?.Invoke(i, ((decimal)ChangedChoiseWinning / i) * 100);
                    }
                }
            });

        }

    }
}
