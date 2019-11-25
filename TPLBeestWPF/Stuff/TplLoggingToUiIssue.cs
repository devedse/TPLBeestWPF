﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using System.Windows.Media;

namespace TPLBeestWPF.Stuff
{
    public class TplLoggingToUiIssue
    {
        private readonly ILogger _logger;

        public TplLoggingToUiIssue(ILogger logger)
        {
            _logger = logger;
        }

        public IEnumerable<string> RecurseFiles()
        {
            for (int i = 0; i < 6; i++)
            {
                yield return i.ToString();
            }
        }


        public async Task Go()
        {
            var progress = new Progress<ListBoxThing>();
            progress.ProgressChanged += (sender, e) =>
            {
                _logger.ThingChanged(e);
            };

            bool shouldSync = false;



            var block1 = new TransformBlock<string, ListBoxThing>(input =>
            {
                Console.WriteLine($"({Thread.CurrentThread.ManagedThreadId}) 1: {input}");
                var newItem = new ListBoxThing()
                {
                    Path = input
                };
                return newItem;
            }, new ExecutionDataflowBlockOptions()
            {
                MaxDegreeOfParallelism = 4,
                BoundedCapacity = 10,
                EnsureOrdered = false
            });

            var block2 = new TransformBlock<ListBoxThing, ListBoxThing>(input =>
            {
                Console.WriteLine($"({Thread.CurrentThread.ManagedThreadId}) 2: {input}\t\t\tStarting {input} now (ui logging)");
                //_logger.ThingChanged(input);
                return input;
            }, ExecutionDataflowBlockOptionsCreator.SynchronizeForUiThread(shouldSync, new ExecutionDataflowBlockOptions()
            {
                MaxDegreeOfParallelism = 1,
                BoundedCapacity = 1,
                EnsureOrdered = false
            }));


            var block3 = new TransformBlock<ListBoxThing, ListBoxThing>(async input =>
            {
                Console.WriteLine($"({Thread.CurrentThread.ManagedThreadId}) 3 start: {input.Path}");
                ((IProgress<ListBoxThing>)progress).Report(input);

                await Task.Delay(5000);
                input.Color = Brushes.LightGreen;

                ((IProgress<ListBoxThing>)progress).Report(input);
                Console.WriteLine($"({Thread.CurrentThread.ManagedThreadId}) 3 end: {input.Path}");

                return input;
            }, new ExecutionDataflowBlockOptions()
            {
                MaxDegreeOfParallelism = 2,
                BoundedCapacity = 2,
                EnsureOrdered = false
            });

            var block4 = new ActionBlock<ListBoxThing>(input =>
            {
                Console.WriteLine($"({Thread.CurrentThread.ManagedThreadId}) 4: {input}");
                //_logger.ThingChanged(input);
            }, ExecutionDataflowBlockOptionsCreator.SynchronizeForUiThread(shouldSync, new ExecutionDataflowBlockOptions()
            {
                MaxDegreeOfParallelism = 1,
                BoundedCapacity = 1,
                EnsureOrdered = false
            }));


            block1.LinkTo(block2, new DataflowLinkOptions() { PropagateCompletion = true });
            block2.LinkTo(block3, new DataflowLinkOptions() { PropagateCompletion = true });
            block3.LinkTo(block4, new DataflowLinkOptions() { PropagateCompletion = true });


            var files = RecurseFiles();
            await Task.Run(async () =>
            {
                foreach (var file in files)
                {
                    Console.WriteLine($"Posting: {file}");
                    var result = await block1.SendAsync(file);

                    if (!result)
                    {
                        Console.WriteLine("Result is false!!!");
                    }
                }
            });

            Console.WriteLine("Completing");
            block1.Complete();
            await block4.Completion;
            Console.WriteLine("Done");
        }


    }
}
