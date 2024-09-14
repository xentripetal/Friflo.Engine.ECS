﻿// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using static System.Diagnostics.DebuggerBrowsableState;
using Browse = System.Diagnostics.DebuggerBrowsableAttribute;

// ReSharper disable CoVariantArrayConversion
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

/// <summary>
///     Enables <see cref="JobExecution.Parallel" /> query execution returning the specified components.
///     See <a href="https://friflo.gitbook.io/friflo.engine.ecs/examples/optimization#parallel-query-job">Example.</a>
/// </summary>
public sealed class QueryJob<T1> : QueryJob
    where T1 : struct, IComponent
{
    private static readonly int Multiple = ComponentType<T1>.ComponentMultiple;
    private readonly Action<Chunk<T1>, ChunkEntities> action; //  8

    [Browse(Never)]
    private readonly ArchetypeQuery<T1> query; //  8
    [Browse(Never)]
    private QueryJobTask[] jobTasks; //  8

    internal QueryJob(
        ArchetypeQuery<T1> query,
        Action<Chunk<T1>, ChunkEntities> action
    )
    {
        this.query = query;
        this.action = action;
        jobRunner = query.Store.JobRunner;
    }

    internal QueryChunks<T1> Chunks => new (query); // only for debugger
    internal QueryEntities Entities => query.Entities; // only for debugger

    public override int ParallelComponentMultiple => Multiple;
    public override string ToString() => query.GetQueryJobString();

    public override void Run()
    {
        foreach (var chunk in query.Chunks)
        {
            action(chunk.Chunk1, chunk.Entities);
        }
    }

    /// <summary>
    ///     Execute the query.
    ///     See <a href="https://friflo.gitbook.io/friflo.engine.ecs/examples/optimization#parallel-query-job">Example.</a>.
    ///     <br />
    ///     All chunks having at least <see cref="QueryJob.MinParallelChunkLength" /> *
    ///     <see cref="ParallelJobRunner.ThreadCount" />
    ///     components are executed <see cref="JobExecution.Parallel" />.
    /// </summary>
    public override void RunParallel()
    {
        if (jobRunner == null) throw JobRunnerIsNullException();
        var taskCount = jobRunner.workerCount + 1;

        foreach (var chunks in query.Chunks)
        {
            var chunkLength = chunks.Length;
            if (ExecuteSequential(taskCount, chunkLength))
            {
                action(chunks.Chunk1, chunks.Entities);
                continue;
            }
            var tasks = jobTasks;
            if (tasks == null || tasks.Length < taskCount)
            {
                tasks = jobTasks = new QueryJobTask[taskCount];
                for (var n = 0; n < taskCount; n++)
                {
                    tasks[n] = new QueryJobTask
                    {
                        action = action
                    };
                }
            }
            var sectionSize = GetSectionSize(chunkLength, taskCount, Multiple);
            var start = 0;
            for (var taskIndex = 0; taskIndex < taskCount; taskIndex++)
            {
                var length = GetSectionLength(chunkLength, start, sectionSize);
                if (length > 0)
                {
                    tasks[taskIndex].chunks = new Chunks<T1>(chunks, start, length, taskIndex);
                    start += sectionSize;
                    continue;
                }
                for (; taskIndex < taskCount; taskIndex++)
                {
                    tasks[taskIndex].chunks = new Chunks<T1>(chunks.Entities, taskIndex);
                }
                break;
            }
            jobRunner.ExecuteJob(this, tasks);
        }
    }


    private class QueryJobTask : JobTask
    {
        internal Action<Chunk<T1>, ChunkEntities> action;
        internal Chunks<T1> chunks;

        internal override void ExecuteTask() => action(chunks.Chunk1, chunks.Entities);
    }
}
