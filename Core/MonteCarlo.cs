using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Core
{
    public abstract class MonteCarlo<R, I, F>
    {
        public long Replications { get; set; }
        public long ResultInterval { get; set; }
        public I InputData { get; set; }

        private delegate bool ResultIntervalConditionDelegate(long replication);
        private ResultIntervalConditionDelegate ResultIntervalCondition;

        public delegate void PartialResult(long replication, R result);
        public event PartialResult OnNewPartialResult;

        public delegate void Result(F result);
        public event Result OnFinish;

        private ManualResetEvent mutex;

        public MonteCarlo(I inputData, long replications = 1000000, long? resultInterval = null)
        {
            this.InputData = inputData;
            this.Replications = replications;
            this.ResultInterval = resultInterval ?? (Replications >= 1000 ? Replications / 100 : Replications / 10);
            this.mutex = new ManualResetEvent(true);
            if( resultInterval == 1)
            {
                ResultIntervalCondition = (rep) => true;
            }else
            {
                ResultIntervalCondition = (rep) => rep % (ResultInterval) == 1;
            }
        }

        public abstract void Inicialization();
        public abstract R Experiment(long replication);
        public abstract F AfterExperiment();

        public Task RunExperiment()
        {
            return Task.Run(() =>
            {
                Inicialization();
                R experimentResult = default(R);
                for (long i = 1; i <= Replications; i++)
                {
                    mutex.WaitOne();

                    experimentResult = Experiment(i);

                    if (ResultIntervalCondition(i))
                    {
                        OnNewPartialResult?.Invoke(i, experimentResult);
                    }
                }
                OnNewPartialResult?.Invoke(Replications, experimentResult);
                OnFinish?.Invoke(AfterExperiment());
            });
        }

        public void Pause()
        {
            mutex.Reset();
        }

        public void Continue()
        {
            mutex.Set();
        }
    }
}
