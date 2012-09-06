using System;
using System.Transactions;
using Rebus.Logging;

namespace Rebus.RabbitMQ
{
    class TxMan : IEnlistmentNotification
    {
        static ILog log;

        static TxMan()
        {
            RebusLoggerFactory.Changed += f => log = f.GetCurrentClassLogger();
        }

        public event Action OnCommit = delegate { };
        public event Action OnRollback = delegate { };
        public event Action Cleanup = delegate { };
        
        public void Prepare(PreparingEnlistment preparingEnlistment)
        {
            preparingEnlistment.Prepared();
        }

        public void Commit(Enlistment enlistment)
        {
            try
            {
                log.Debug("Committing!");
                OnCommit();
                enlistment.Done();
            }
            catch (Exception e)
            {
                log.Error(e, "An error occurred while committing!");
                throw;
            }
            finally
            {
                RaiseCleanup();
            }
        }

        public void Rollback(Enlistment enlistment)
        {
            try
            {
                log.Debug("Rolling back!");
                OnRollback();
                enlistment.Done();
            }
            catch (Exception e)
            {
                log.Error(e, "An error occurred while rolling back!");
                throw;
            }
            finally
            {
                RaiseCleanup();
            }
        }

        void RaiseCleanup()
        {
            try
            {
                Cleanup();
            }
            finally
            {
                OnCommit = null;
                OnRollback = null;
                Cleanup = null;
            }
        }

        public void InDoubt(Enlistment enlistment)
        {
            log.Debug("In doubt!");
            enlistment.Done();
        }
    }
}