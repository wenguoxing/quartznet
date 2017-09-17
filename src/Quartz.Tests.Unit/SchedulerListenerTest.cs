﻿using System.Collections.Specialized;
using System.Threading;
using System.Threading.Tasks;

using NUnit.Framework;

using Quartz.Impl;
using Quartz.Util;

namespace Quartz.Tests.Unit
{
    /// <summary>
    /// Tests for <see cref="ISchedulerListener"/>.
    /// </summary>
    /// <author>Zemian Deng</author>
    /// <author>Marko Lahma (.NET)</author>
    [TestFixture]
    public class SchedulerListenerTest
    {
        private static readonly ILog logger = LogProvider.GetLogger(typeof(SchedulerListenerTest));
        private static int jobExecutionCount;

        public class Qtz205Job : IJob
        {
            public Task Execute(IJobExecutionContext context)
            {
                jobExecutionCount++;
                logger.Info("Job executed. jobExecutionCount=" + jobExecutionCount);
                return Task.CompletedTask;
            }
        }

        public class Qtz205TriggerListener : ITriggerListener
        {
            public int FireCount { get; private set; }

            public string Name => "Qtz205TriggerListener";

            public Task TriggerFired(ITrigger trigger, IJobExecutionContext context, CancellationToken cancellationToken)
            {
                FireCount++;
                logger.Info("Trigger fired. count " + FireCount);
                return TaskUtil.CompletedTask;
            }

            public Task<bool> VetoJobExecution(ITrigger trigger, IJobExecutionContext context, CancellationToken cancellationToken)
            {
                if (FireCount >= 3)
                {
                    return Task.FromResult(true);
                }
                return Task.FromResult(false);
            }

            public Task TriggerMisfired(ITrigger trigger, CancellationToken cancellationToken)
            {
                return TaskUtil.CompletedTask;
            }

            public Task TriggerComplete(ITrigger trigger,
                                        IJobExecutionContext context,
                                        SchedulerInstruction triggerInstructionCode,
                                        CancellationToken cancellationToken)
            {
                return TaskUtil.CompletedTask;
            }
        }

        public class Qtz205ScheListener : ISchedulerListener
        {
            public int TriggerFinalizedCount { get; private set; }

            public Task JobScheduled(ITrigger trigger, CancellationToken cancellationToken)
            {
                return TaskUtil.CompletedTask;
            }

            public Task JobUnscheduled(TriggerKey triggerKey, CancellationToken cancellationToken)
            {
                return TaskUtil.CompletedTask;
            }

            public Task TriggerFinalized(ITrigger trigger, CancellationToken cancellationToken)
            {
                TriggerFinalizedCount ++;
                logger.Info("triggerFinalized " + trigger);
                return TaskUtil.CompletedTask;
            }

            public Task TriggerPaused(TriggerKey triggerKey, CancellationToken cancellationToken)
            {
                return TaskUtil.CompletedTask;
            }

            public Task TriggersPaused(string triggerGroup, CancellationToken cancellationToken)
            {
                return TaskUtil.CompletedTask;
            }

            public Task TriggerResumed(TriggerKey triggerKey, CancellationToken cancellationToken)
            {
                return TaskUtil.CompletedTask;
            }

            public Task TriggersResumed(string triggerGroup, CancellationToken cancellationToken)
            {
                return TaskUtil.CompletedTask;
            }

            public Task JobAdded(IJobDetail jobDetail, CancellationToken cancellationToken)
            {
                return TaskUtil.CompletedTask;
            }

            public Task JobDeleted(JobKey jobKey, CancellationToken cancellationToken)
            {
                return TaskUtil.CompletedTask;
            }

            public Task JobPaused(JobKey jobKey, CancellationToken cancellationToken)
            {
                return TaskUtil.CompletedTask;
            }

            public Task JobInterrupted(JobKey jobKey, CancellationToken cancellationToken = new CancellationToken())
            {
                return TaskUtil.CompletedTask;
            }

            public Task JobsPaused(string jobGroup, CancellationToken cancellationToken)
            {
                return TaskUtil.CompletedTask;
            }

            public Task JobResumed(JobKey jobKey, CancellationToken cancellationToken)
            {
                return TaskUtil.CompletedTask;
            }

            public Task JobsResumed(string jobGroup, CancellationToken cancellationToken)
            {
                return TaskUtil.CompletedTask;
            }

            public Task SchedulerError(string msg, SchedulerException cause, CancellationToken cancellationToken)
            {
                return TaskUtil.CompletedTask;
            }

            public Task SchedulerInStandbyMode(CancellationToken cancellationToken)
            {
                return TaskUtil.CompletedTask;
            }

            public Task SchedulerStarted(CancellationToken cancellationToken)
            {
                return TaskUtil.CompletedTask;
            }

            public Task SchedulerStarting(CancellationToken cancellationToken)
            {
                return TaskUtil.CompletedTask;
            }

            public Task SchedulerShutdown(CancellationToken cancellationToken)
            {
                return TaskUtil.CompletedTask;
            }

            public Task SchedulerShuttingdown(CancellationToken cancellationToken)
            {
                return TaskUtil.CompletedTask;
            }

            public Task SchedulingDataCleared(CancellationToken cancellationToken)
            {
                return TaskUtil.CompletedTask;
            }
        }

        [Test]
        public async Task TestTriggerFinalized()
        {
            Qtz205TriggerListener triggerListener = new Qtz205TriggerListener();
            Qtz205ScheListener schedulerListener = new Qtz205ScheListener();
            NameValueCollection props = new NameValueCollection();
            props["quartz.scheduler.idleWaitTime"] = "1500";
            props["quartz.threadPool.threadCount"] = "2";
            props["quartz.serializer.type"] = TestConstants.DefaultSerializerType;
            IScheduler scheduler = await new StdSchedulerFactory(props).GetScheduler();
            scheduler.ListenerManager.AddSchedulerListener(schedulerListener);
            scheduler.ListenerManager.AddTriggerListener(triggerListener);

            IJobDetail job = JobBuilder.Create<Qtz205Job>().WithIdentity("test").Build();
            ITrigger trigger = TriggerBuilder.Create()
                .WithIdentity("test")
                .WithSchedule(SimpleScheduleBuilder.RepeatSecondlyForTotalCount(3))
                .Build();

            await scheduler.ScheduleJob(job, trigger);
            await scheduler.Start();
            await Task.Delay(5000);

            await scheduler.Shutdown(true);

            Assert.AreEqual(2, jobExecutionCount);
            Assert.AreEqual(3, triggerListener.FireCount);
            Assert.AreEqual(1, schedulerListener.TriggerFinalizedCount);
        }
    }
}