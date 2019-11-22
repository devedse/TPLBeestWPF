using System;
using System.Collections.ObjectModel;
using System.Threading;

namespace TPLBeestWPF.Stuff
{
    public class UiLogger : ILogger
    {
        public ObservableCollection<ListBoxThing> Items { get; }

        public UiLogger(ObservableCollection<ListBoxThing> items)
        {
            Items = items;
        }

        public void ThingChanged(ListBoxThing thing)
        {
            Console.WriteLine($"({Thread.CurrentThread.ManagedThreadId}) Changed: {thing.Path}");

            if (Items.Contains(thing))
            {
                var pos = Items.IndexOf(thing);
                Items.Remove(thing);
                Items.Insert(pos, thing);
            }
            else
            {
                Items.Add(thing);
            }
        }
    }
}
