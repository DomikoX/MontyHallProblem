using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Core
{
    public abstract class MonteCarlo<PartialResultData, InputDataType, FinalResultDataType>
    {
        public long Replications { get; set; }
        public long ResultInterval { get; set; }
        public InputDataType InputData { get; set; }


        private delegate bool ResultIntervalConditionDelegate(long replication);
        private ResultIntervalConditionDelegate ResultIntervalCondition;

        public delegate void PartialResult(long replication, PartialResultData result);
        public event PartialResult OnNewPartialResult;

        public delegate void Result(FinalResultDataType result);
        public event Result OnFinish;

        private ManualResetEvent mutex;
        private bool cancel;

        public MonteCarlo(InputDataType inputData, long replications = 1000000, long? resultInterval = null)
        {
            this.InputData = inputData;
            this.Replications = replications;
            this.ResultInterval = resultInterval ?? (Replications >= 1000 ? Replications / 500 : Replications / 10);
            this.mutex = new ManualResetEvent(true);
            this.cancel = false;
            if (resultInterval == 1)
            {
                ResultIntervalCondition = (rep) => true;
            }
            else
            {
                ResultIntervalCondition = (rep) => rep % (ResultInterval) == 1;
            }
        }

        public abstract void Inicialization();
        public abstract PartialResultData Experiment(long replication);
        public abstract FinalResultDataType AfterExperiment();

        public Task RunExperiment()
        {
            return Task.Run(() =>
            {
                Inicialization();
                PartialResultData experimentResult = default(PartialResultData);
                for (long i = 1; i <= Replications; i++)
                {
                    mutex.WaitOne();

                    if (cancel) return;

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

        public void Cancel()
        {
            this.cancel = true;
            this.Continue();
        }
    }
}
