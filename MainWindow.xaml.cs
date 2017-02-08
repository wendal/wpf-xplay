using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using WpfXplay.bean;

namespace WpfXplay
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window, XCallback
    {

        XplayListener xs;

        Peanut peanut;

        public MainWindow()
        {
            InitializeComponent();
            xs = new XplayListener();
            xs.callback = this;
            Thread t = new Thread(xs.run);
            t.IsBackground = true;
            t.Start();

            peanut = new Peanut();
            Peanut.callback = this;
            //peanut.Start();
            PlayObj.scaling = peanut.getDoubleConf("scaling");
            if (PlayObj.scaling < 0.1)
            {
                PlayObj.scaling = 0.5;
                Peanut.conf["scaling"] = "0.5";
                peanut.saveConf();
            }
                

            Width = System.Windows.SystemParameters.PrimaryScreenWidth * PlayObj.scaling;
            Height = System.Windows.SystemParameters.PrimaryScreenHeight * PlayObj.scaling;
            if (PlayObj.scaling == 1)
            {
                this.Top = 0;
                this.Left = 0;
            }
            

            Matrix matrix;
            var source = PresentationSource.FromVisual(this);
            if (source != null)
            {
                matrix = source.CompositionTarget.TransformToDevice;
            }
            else
            {
                using (var src = new HwndSource(new HwndSourceParameters()))
                {
                    matrix = src.CompositionTarget.TransformToDevice;
                }
            }

            if (matrix != null)
            {
                PlayObj.dpiX = 96.0 * matrix.M11;
                PlayObj.dpiY = 96.0 * matrix.M22;
            }

            ContextMenu aMenu = new ContextMenu();
            MenuItem exitMenu = new MenuItem();
            exitMenu.Header = "退出";
            exitMenu.Click += (Object sender, RoutedEventArgs e) =>
            {
                Environment.Exit(0);
            };
            aMenu.Items.Add(exitMenu);
            if (PlayObj.scaling != 1)
            {
                MenuItem fullScreenMenu = new MenuItem();
                fullScreenMenu.Header = "全屏播放";
                fullScreenMenu.Click += (Object sender, RoutedEventArgs e) =>
                {
                    setScalingAndExit(1);
                };
                aMenu.Items.Add(fullScreenMenu);
            } else
            {
                MenuItem previewScreenMenu = new MenuItem();
                previewScreenMenu.Header = "半屏播放";
                previewScreenMenu.Click += (Object sender, RoutedEventArgs e) =>
                {
                    setScalingAndExit(0.5);
                };
                aMenu.Items.Add(previewScreenMenu);
            }
            

            TopCanvas.ContextMenu = aMenu;
            ContextMenu = aMenu;
            this.MouseLeftButtonDown += this.Window_MouseLeftButtonDown;
        }

        private void Window_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        public void HandlePlayObject(object obj)
        {
            PlayObj pobj = (PlayObj)obj;
            if (pobj._params == null)
                return;
            long start = pobj.start;
            if (start > 0)
            {
                long now = GetTimestamp(DateTime.UtcNow);
                int sleep = (int)(start - now);
                if (sleep > 5)
                {
                    Thread.Sleep(sleep - 5);
                }
            }

            Dispatcher.BeginInvoke(new Action(delegate ()
            {
                if ("stop" == pobj.type)
                {
                    StopLayout(pobj);
                }
                else if ("play" == pobj.type)
                {
                    StartLayout(pobj.getIntParam("zIndex"), pobj);
                }
                else
                {
                    Console.Out.WriteLine("what? type=" + pobj.type);
                }
            }));
            
        }

        public void StopLayout(PlayObj pobj)
        {
            object[] tmp = pobj._params["ids"] as object[];
            //Console.Out.WriteLine("tmp=" + tmp.GetType());
            foreach (string id in tmp)
            {
                int zIndex = int.Parse(id);

                UIElement ele = getByZIndex(zIndex);
                if (ele != null)
                {
                    TopCanvas.Children.Remove(ele);
                    TopCanvas.UnregisterName("LV_" + zIndex);
                }
            }
                    
        }

        public void StopLayout(int zIndex)
        {
            UIElement ele = getByZIndex(zIndex);
            if (ele != null)
            {
                TopCanvas.Children.Remove(ele);
                TopCanvas.UnregisterName("LV_" + zIndex);
            }
        }

        public void StartLayout(int zIndex, PlayObj pobj)
        {
            string libName = pobj.libName;
            UIElement ele = getByZIndex(zIndex);
            UIElement old = null;
            if (ele != null && ele is XplayUiEle && ((XplayUiEle)ele).getLibName() != pobj.libName)
            {
                old = ele;
                ele = null;
            }
            bool isNew = ele == null;
            if (ele == null)
            {
                switch (libName)
                {
                    case "text":
                        ele = new TextUiEle();
                        break;
                    case "video":
                        ele = new VideoUiEle();
                        break;
                    case "pic":
                        ele = new PicUiEle();
                        break;
                }
            }
            if (ele == null)
                return;
            XplayUiEle xe = (XplayUiEle)ele;
            xe.prepare(pobj);
            
            if (libName == "text")
            {
                Canvas.SetLeft(ele, pobj.getIntParam("left") * PlayObj.scaling * 96 / PlayObj.dpiX);
                Canvas.SetTop(ele, pobj.getIntParam("top") * PlayObj.scaling * 96 / PlayObj.dpiY - 20);
                //Console.Out.WriteLine(""+(pobj.getIntParam("top") * PlayObj.scaling * 96 / PlayObj.dpiY));
            }
            else
            {
                Canvas.SetLeft(ele, pobj.getIntParam("left") * PlayObj.scaling);
                Canvas.SetTop(ele, pobj.getIntParam("top") * PlayObj.scaling);
            }
            
            Canvas.SetZIndex(ele, 0 - zIndex);

            if (isNew)
            {
                TopCanvas.Children.Add(ele);
                if (old != null)
                {
                    TopCanvas.Children.Remove(old);
                    TopCanvas.UnregisterName("LV_" + zIndex);
                }
                TopCanvas.RegisterName("LV_" + zIndex, ele);
            }
            xe.Play();
        }

        public UIElement getByZIndex(int zIndex)
        {
            return (UIElement)TopCanvas.FindName("LV_" + zIndex);
        }

        public long GetTimestamp(DateTime dateTime)
        {
            DateTime dt1970 = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return (dateTime.Ticks - dt1970.Ticks) / 10000;
        }

        public void setScalingAndExit(double scaling)
        {
            PlayObj.scaling = scaling;
            Peanut.conf["scaling"] = "" + scaling;
            peanut.saveConf();
            MessageBox.Show("设置完成,程序将自动退出");
            Environment.Exit(0);
        }
    }

    public interface XCallback
    {
        void HandlePlayObject(object obj);
    }


    public interface XplayUiEle
    {
        void prepare(PlayObj pobj);
        void Play();
        void Stop();
        string getLibName();
    }
}
