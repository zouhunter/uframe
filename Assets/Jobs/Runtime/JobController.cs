using Unity.Jobs;
using System.Collections.Generic;

namespace UFrame.Jobs
{
    public class JobController : IJobController, System.IDisposable
    {
        protected Queue<IJobContent> m_jobContents;
        protected Queue<IJobContent> m_waitJobs;
        public JobController()
        {
            m_jobContents = new Queue<IJobContent>();
            m_waitJobs = new Queue<IJobContent>();
        }

        public void AppendJob(IJobContent jobContent)
        {
            m_jobContents.Enqueue(jobContent);
        }

        public void ScheduleJobs()
        {
            if (m_jobContents.Count > 0)
            {
                var jobContent = m_jobContents.Dequeue();
                jobContent.Schedule();
                m_waitJobs.Enqueue(jobContent);
            }
        }

        public void CompleteJobs()
        {
            if(m_waitJobs.Count >0)
            {
                var jobContent = m_waitJobs.Dequeue();
                jobContent.Complete();
            }
        }

        public void Dispose()
        {
            m_jobContents.Clear();
            if(m_waitJobs.Count > 0)
            {
                JobHandle.ScheduleBatchedJobs();
                while(m_waitJobs.Count > 0)
                {
                    var jobContent = m_waitJobs.Dequeue();
                    jobContent.Complete();
                }
                m_waitJobs.Clear();
            }
        }
    }
}