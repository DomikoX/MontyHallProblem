using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Core
{
    /// <summary>
    /// Class that represent Monte Carlo simulation. 
    /// </summary>
    /// <typeparam name="PartialResultData"> Data that is returned on every replications (unless it it specified otherwise)</typeparam>
    /// <typeparam name="InputDataType">Data that is used for inicialization</typeparam>
    /// <typeparam name="FinalResultDataType"> Data that is returned after simulation ends</typeparam>
    public abstract class MonteCarlo<PartialResultData, InputDataType, FinalResultDataType>
    {
        /// <summary>
        /// Number of replikations to run
        /// </summary>
        public long Replications { get; set; }
        /// <summary>
        /// Set interval how often results from replications are invoked
        /// </summary>
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

        /// <summary>
        /// Here should be be all  data inicialized before experiment is started
        /// </summary>
        public abstract void Inicialization();

        /// <summary>
        /// This method represent single replications
        /// </summary>
        /// <param name="replication">replication number</param>
        /// <returns></returns>
        public abstract PartialResultData Experiment(long replication);
        /// <summary>
        /// method that is invoked after experiment runs
        /// </summary>
        /// <returns></returns>
        public abstract FinalResultDataType AfterExperiment();

        /// <summary>
        /// Run experiment, before that calls Inicialization() method and after all replikations call AfterExperiment() method and invoke OnFinish event
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// pause execution of current simulation
        /// </summary>
        public void Pause()
        {
            mutex.Reset();
        }

        /// <summary>
        /// Continue execution of current simulation
        /// </summary>
        public void Continue()
        {
            mutex.Set();
        }

        /// <summary>
        /// cancel current simulation
        /// </summary>
        public void Cancel()
        {
            this.cancel = true;
            this.Continue();
        }
    }
}
