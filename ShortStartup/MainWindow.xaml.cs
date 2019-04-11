using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;

namespace ShortStartup
{
    public struct ProgramInfo
    {
        public string Group;
        public string Content;
        public bool IsEnable;
        public string Program;
        public string Args;
        public string WorkDirectory;
    }

    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        #region 内部变量
        private List<ProgramInfo> ProgramList;

        //需添加System.Windows.Forms引用
        private System.Windows.Forms.NotifyIcon notifyIcon = null;

        private HotKey _hotkey;
        #endregion

        #region 界面响应
        public MainWindow()
        {
            InitializeComponent();

            ProgramList = new List<ProgramInfo>();

            notifyIcon = new System.Windows.Forms.NotifyIcon();

            //控制是否在任务栏显示
            //ShowInTaskbar = false;

            //可以这种方法手工指定,也可以在xaml里面绑定closing
            //this.Closing += WindowClosing;
        }

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            if(!LoadConfig())
            {
                return;
            }

            if (!InitComponets())
            {
                return;
            }

            if (!InitTray())
            {
                return;
            }

            if (!RegisterKey())
            {
                return;
            }

            //MessageBoxTimeoutWinApi.MessageBoxTimeoutA((IntPtr)0,"程序加载成功!,两秒后此消息框自动关闭...","提示",0,0,3000);
        }

        private void WindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SaveConfig();

            notifyIcon.Dispose();
        }

        private void FormKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Tab)
            {
                if (tabGroup.SelectedIndex + 1 >= tabGroup.Items.Count)
                    tabGroup.SelectedIndex = 0;
                else
                    tabGroup.SelectedIndex++;
            }
            else if (e.Key == System.Windows.Input.Key.Back)
            {
                if (tabGroup.SelectedIndex <= 0)
                    tabGroup.SelectedIndex = tabGroup.Items.Count - 1;
                else
                    tabGroup.SelectedIndex--;
            }
            else if (e.Key == System.Windows.Input.Key.Escape)
            {
                WindowState = WindowState.Minimized;
            }
        }
        #endregion

        #region 内部函数
        private bool LoadConfig()
        {
            XDocument doc;
            try
            {
                doc = XDocument.Load("config.xml");
            }
            catch (Exception e)
            {
                MessageBox.Show("加载config.xml失败:" + e.Message);
                return false;
            }

            XElement root = doc.Root;

            //遍历group
            foreach (XElement group in root.Elements())
            {
                string groupName;

                try
                {
                    groupName = group.Attribute("name").Value;
                }
                catch (Exception e)
                {
                    MessageBox.Show("获取（" + group.Name + "）属性失败:" + e.Message);
                    return false;
                }

                //遍历button
                foreach(XElement item in group.Elements())
                {
                    ProgramInfo pi = new ProgramInfo();

                    try
                    {
                        pi.Group = groupName;
                        pi.IsEnable = item.Attribute("enable").Value.ToUpper() == "TRUE";
                        pi.Content = item.Attribute("content").Value.ToUpper();
                        pi.Program = item.Attribute("program").Value;
                        pi.Args = item.Attribute("args").Value;
                        pi.WorkDirectory = item.Attribute("workdirectory").Value;
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show("获取（" + item.Name + "）属性失败:" + e.Message);
                        return false;
                    }

                    ProgramList.Add(pi);
                }
            }

            return true;
        }

        private bool SaveConfig()
        {
            return true;
        }

        private bool InitComponets()
        {
            foreach (ProgramInfo pi in ProgramList)
            {
                if (!pi.IsEnable)
                {
                    continue;
                }

                //判断Group是否存在
                TabItem tiSelected = null;
                foreach (TabItem ti in tabGroup.Items)
                {
                    if (ti.Header.ToString() == pi.Group)
                    {
                        tiSelected = ti;
                    }
                }

                //不存在则增加一个TabItem(并且将其content设置为一个WrapPanel,用于放置按钮)
                if (tiSelected == null)
                {
                    tiSelected = new TabItem { Header = pi.Group };

                    WrapPanel wp = new WrapPanel { Orientation = Orientation.Horizontal };

                    tiSelected.Content = wp;

                    tabGroup.Items.Add(tiSelected);
                }

                //定义一个StackPanel(上面应用图标按钮/下面是文字)
                StackPanel sp = new StackPanel
                {
                    Width = 64,
                    Height = 96,
                    Margin = new Thickness(10,10,0,0),
                    Orientation = Orientation.Vertical
                };

                //初始化一个按钮
                Button btn = new Button
                {
                    Height = 64,
                    Width = 64,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Top,
                    Visibility = Visibility.Visible,
                    ToolTip = pi.Program + " " + pi.Args,
                    Focusable = false,//加了这一句之后,图标就不会闪来闪去了
                };

                //由于有些程序包含配置文件(比如SQLUpdate的config.ini),所以需要给程序一个"起始位置"
                btn.Click += (e, a) =>
                {
                    if ((pi.Program.IndexOf("C:\\") == 0 || pi.Program.IndexOf("D:\\") == 0 || pi.Program.IndexOf("E:\\") == 0 || pi.Program.IndexOf("F:\\") == 0) &&
                        !File.Exists(pi.Program))
                    {
                        System.Windows.MessageBox.Show("文件【" + pi.Program + "】不存在，请确认是否变更过程序！！！");
                        return;
                    }

                    string worksps = pi.WorkDirectory;

                    //如果WorkDirectory为空且pi.Program中包含路径信息,则从pi.Program中取
                    if (worksps.Length == 0 && pi.Program.IndexOf(":") >= 0 && pi.Program.IndexOf("\\") >= 0)
                    {
                        worksps = pi.Program.Substring(0, pi.Program.LastIndexOf('\\'));
                    }

                    Process.Start(new ProcessStartInfo(pi.Program, pi.Args){
                        UseShellExecute = false,
                        WorkingDirectory = worksps
                    });
                };

                ContextMenu contextMenu = new ContextMenu();

                MenuItem mi = new MenuItem { Header = "打开文件位置", };
                mi.Click += (e, a) =>
                {
                    if (System.IO.File.Exists(pi.Program))
                    {
                        Process.Start("explorer.exe", "/select," + pi.Program);
                    }
                    else if (System.IO.File.Exists(pi.Args))
                    {
                        Process.Start("explorer.exe", "/select," + pi.Args);
                    }
                    else if (System.IO.Directory.Exists(pi.Args))
                    {
                        Process.Start("explorer.exe", "/select," + pi.Args);
                    }
                };

                contextMenu.Items.Add(mi);

                btn.ContextMenu = contextMenu;

                //取应用图标
                try
                {
                    //选中文件中的图标总数
                    var iconTotalCount = ExtractIconWinApi.ExtractIconEx(pi.Program, -1, null, null, 0);

                    //用于接收获取到的大图标指针
                    IntPtr[] largeIcons = new IntPtr[iconTotalCount];

                    //用于接收获取到的小图标指针
                    IntPtr[] smallIcons = new IntPtr[iconTotalCount];

                    //成功获取到的图标个数
                    var successCount = ExtractIconWinApi.ExtractIconEx(pi.Program, 0, largeIcons, smallIcons, iconTotalCount);

                    if (successCount > 0)
                    {
                        //如果"System.Drawing.Icon.FromHandle(largeIcons[0])"报错:命名空间“System.Drawing”中不存在类型或命名空间名“Icon”,则：
                        //项目-应用-右键添加应用-System.Drawing

                        //1.通过FromHandle得到Icon
                        System.Drawing.Icon icon;
                        if (largeIcons[0] != null)
                        {
                            icon = System.Drawing.Icon.FromHandle(largeIcons[0]);
                        }
                        else
                        {
                            icon = System.Drawing.Icon.FromHandle(smallIcons[0]);
                        }

                        //2.通过ToBitmap得到Bitmap
                        System.Drawing.Bitmap bitmap = icon.ToBitmap();

                        //3.通过GetHbitmap得到Bitmap句柄
                        IntPtr bitmapHandle = bitmap.GetHbitmap();

                        //4.通过CreateBitmapSourceFromHBitmap得到ImageSource，进而达到ImageBrush
                        System.Windows.Media.ImageBrush imgBrush = new System.Windows.Media.ImageBrush();
                        imgBrush.ImageSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(bitmapHandle,
                            IntPtr.Zero,
                            Int32Rect.Empty,
                            System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());

                        //5.通过Backgroud赋值将图标展示在按钮上...
                        btn.Background = imgBrush;
                    }
                }
                catch (Exception e)
                {
                    MessageBox.Show("获取应用(" + pi.Program + ")图标失败：" + e.Message);
                }

                //初始化一个TextBlock
                TextBlock tb = new TextBlock
                {
                    Text = pi.Content,
                    Width = 64,
                    Height = 32,
                    FontSize = 12,
                    TextAlignment = TextAlignment.Center,
                    TextWrapping = TextWrapping.Wrap,
                    ToolTip =pi.Content,
                };

                sp.Children.Add(btn);
                sp.Children.Add(tb);

                //将Button和TextBlock放置在此TabItem上
                (tiSelected.Content as WrapPanel).Children.Add(sp);
            }
            return true;
        }

        private bool InitTray()
        {
            notifyIcon.Icon = new System.Drawing.Icon("ShortStartUp.ico");
            notifyIcon.Text = "快捷启动程序";
            notifyIcon.BalloonTipText = "快捷启动程序";
            notifyIcon.ShowBalloonTip(1000);
            notifyIcon.Visible = true;

            notifyIcon.MouseClick += new System.Windows.Forms.MouseEventHandler((s, e) => Visibility = (Visibility == Visibility.Hidden) ? Visibility.Visible : Visibility.Hidden);

            return true;
        }

        private bool RegisterKey()
        {
            try
            {
                //注册CTRL + ALT + Q热键
                _hotkey = new HotKey(System.Windows.Input.ModifierKeys.Control | System.Windows.Input.ModifierKeys.Alt, System.Windows.Forms.Keys.Q, this);

                _hotkey.HotKeyPressed += (k) => ResetWindow();
            }
            catch (Exception err)
            {
                MessageBox.Show("注册热键(CTRL + ALT + Q)失败：" + err.Message);
                return false;
            }

            return true;
        }

        private bool ResetWindow()
        {
            if (WindowState == WindowState.Minimized)
            {
                WindowState = WindowState.Normal;
            }

            Visibility = Visibility.Visible;
            Activate();
            return true;
        }
        #endregion
    }
}