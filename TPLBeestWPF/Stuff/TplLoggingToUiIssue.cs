using System;
using System.Collections.Generic;
using System.Text;
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
            var block1 = new TransformBlock<string, ListBoxThing>(input =>
            {
                Console.WriteLine($"1: {input}");
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
                Console.WriteLine($"2: {input}\t\t\tStarting {input} now (ui logging)");
                _logger.ThingChanged(input);
                return input;
            }, ExecutionDataflowBlockOptionsCreator.SynchronizeForUiThread(new ExecutionDataflowBlockOptions()
            {
                MaxDegreeOfParallelism = 1,
                BoundedCapacity = 1,
                EnsureOrdered = false
            }));


            var block3 = new TransformBlock<ListBoxThing, ListBoxThing>(async input =>
            {
                Console.WriteLine($"3 start: {input}");
                await Task.Delay(5000);
                Console.WriteLine($"3 end: {input}");
                return input;
            }, new ExecutionDataflowBlockOptions()
            {
                MaxDegreeOfParallelism = 2,
                BoundedCapacity = 2,
                EnsureOrdered = false
            });

            var block4 = new ActionBlock<ListBoxThing>(input =>
            {
                Console.WriteLine($"4: {input}");
                input.Color = Brushes.LightGreen;
                _logger.ThingChanged(input);
            }, ExecutionDataflowBlockOptionsCreator.SynchronizeForUiThread(new ExecutionDataflowBlockOptions()
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
