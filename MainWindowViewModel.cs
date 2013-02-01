using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Newtonsoft.Json;
using ReactiveUI;
using ReactiveUI.Xaml;
using ReflexUX;

namespace RxUI_QCon
{
    public interface IMainWindowViewModel
    {
        int Red { get; set; }
        int Green { get; set; }
        int Blue { get; set; }
    }

    public class MainWindowController : Reflex<IMainWindowViewModel>
    {
        public MainWindowController()
            : base()
        {
            var whenAnyColorChanges = Proxy.WhenAny(x => x.Red, x => x.Green, x => x.Blue,
                    (r, g, b) => Tuple.Create(r.Value, g.Value, b.Value))
                .Select(intsToColor);

            var finalColor = this.React("FinalColor", whenAnyColorChanges
                .Where(x => x != null)
                .Select(x => new SolidColorBrush(x.Value)));

            Command.Ok = new ReactiveCommand(whenAnyColorChanges.Select(x => x != null));

            this.React("Images", finalColor
                .Throttle(TimeSpan.FromSeconds(0.7), RxApp.DeferredScheduler)
                .Select(x => imagesForColor(x.Color))
                .Switch()
                .ObserveOn(RxApp.DeferredScheduler)
                .Select(imageListToImages));
        }

        public void Ok(object p)
        {
            System.Windows.MessageBox.Show("Got it.");
        }

        Color? intsToColor(Tuple<int, int, int> colorsAsInts)
        {
            byte? r = inRange(colorsAsInts.Item1), g = inRange(colorsAsInts.Item2), b = inRange(colorsAsInts.Item3);

            if (r == null || g == null || b == null) return null;
            return Color.FromRgb(r.Value, g.Value, b.Value);
        }

        static byte? inRange(int value)
        {
            if (value < 0 || value > 255) {
                return null;
            }

            return (byte) value;
        }

        IObservable<ImageList> imagesForColor(Color sourceColor)
        {
            var queryParams = new[] {
                new { k = "method", v = "flickr_color_search" },
                new { k = "limit", v = "73" },
                new { k = "offset", v = "0" },
                new { k = "colors[0]", v = String.Format("{0:x2}{1:x2}{2:x2}", sourceColor.R, sourceColor.G, sourceColor.B) },
                new { k = "weights[0]", v = "1" },
            };

            var query = queryParams.Aggregate("",
                (acc, x) => String.Format("{0}&{1}={2}", acc, HttpUtility.UrlEncode(x.k), HttpUtility.UrlEncode(x.v)));

            query = query.Substring(1);

            var wc = new HttpClient();
            var url = "http://labs.tineye.com/rest/?" + query;
            wc.BaseAddress = new Uri(url);

            return wc.GetStringAsync("").ToObservable()
                .Select(JsonConvert.DeserializeObject<ImageList>);
        }

        List<BitmapImage> imageListToImages(ImageList imageList)
        {
            return imageList.result
                .Select(x => "http://img.tineye.com/flickr-images/?filepath=labs-flickr/" + x.filepath)
                .Select(x => {
                    var ret = new BitmapImage(new Uri(x));
                    return ret;
                }).ToList();
        }
    }
}