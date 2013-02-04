using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using ReactiveUI;
using ReactiveUI.Xaml;
using ReflexUX;

namespace RxUI_QCon
{
    public interface ProgressViewModel
    {
        int ItemsFinished { get; set; }
        int Total { get; set; }
    }

    public class ProgressController : Reflex<ProgressViewModel>
    {
        public ProgressController()
            : base()
        {
            Proxy.ItemsFinished = 0;
            Proxy.Total = 0;
            var progress = Proxy.WhenAny(x => x.ItemsFinished, x => x.Total, (items, total) => items.Value / Math.Max(1.0, total.Value));
            var percent = React("Percent", progress);
            var eta = ((IReactiveCommand)CommandAsync.Start).Timestamp().CombineLatest(percent.Sample(TimeSpan.FromSeconds(0.1)).Timestamp(),
                (start, p) => TimeSpan.FromMilliseconds((1.0 - p.Value) * (p.Timestamp - start.Timestamp).TotalMilliseconds));
            React("ETA", eta);
        }

        public void Start(object p = null)
        {
            Task.Run(async () =>
            {
                var random = new Random();
                await App.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    Proxy.ItemsFinished = 0;
                    Proxy.Total = random.Next(800, 1000);
                }));

                for (int i = 0; i <= Proxy.Total; i++)
                {
                    await Task.Delay(random.Next(30, 100));
                    await App.Current.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        Proxy.ItemsFinished = i;
                    }));
                }
            }).Wait();
        }
    }
}
