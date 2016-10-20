using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using Blend = Microsoft.DirectX.Direct3D.Blend;

namespace Map_Editor
{


    class DXManager
    {
        public static List<MLibrary.MImage> TextureList = new List<MLibrary.MImage>();
        /// <summary>
        /// 主设备
        /// </summary>
        public static Device Device;
        /// <summary>
        /// 精灵
        /// </summary>
        public static Sprite Sprite, TextSprite;
        /// <summary>
        /// 线
        /// </summary>
        public static Line Line;
        /// <summary>
        /// 当前表面 或者 现有表面
        /// </summary>
        public static Surface CurrentSurface;
        /// <summary>
        /// 主表面
        /// </summary>
        public static Surface MainSurface;
        /// <summary>
        /// 描述呈现参数。 
        /// </summary>
        public static PresentParameters Parameters;
        /// <summary>
        /// 设备是否丢失 true 丢失 、false 未丢失
        /// </summary>
        public static bool DeviceLost;
        /// <summary>
        /// 不透明度
        /// </summary>
        public static float Opacity = 1F;
        /// <summary>
        /// 是否混合
        /// </summary>
        public static bool Blending;

        /// <summary>
        /// 光源纹理集合
        /// </summary>
        public static List<Texture> Lights = new List<Texture>();

        /// <summary>
        /// 光源坐标集合
        /// </summary>
        public static Point[] LightSizes =
        {
            new Point(125,95),
            new Point(205,156),
            new Point(285,217),
            new Point(365,277),
            new Point(445,338),
            new Point(525,399),
            new Point(605,460),
            new Point(685,521),
            new Point(765,581),
            new Point(845,642),
            new Point(925,703)
        };

        private static Control _control;
        public static void Create(Control control)
        {

            _control = control;
            Parameters = new PresentParameters
            {
                //检索或设置后台缓冲区的格式//	一种 32 位 RGB 像素格式，其中每种颜色使用 8 位。
                BackBufferFormat = Format.X8R8G8B8,
                //检索或设置呈现标志 //通知驱动程序，后台缓冲区包含视频数据
                PresentFlag = PresentFlag.LockableBackBuffer,
                //检索或设置交换链的后台缓冲区的高度
                BackBufferWidth = control.Width,
                //检索或设置交换链的后台缓冲区的宽度
                BackBufferHeight = control.Height,
                //检索或设置交换效果 ////当前屏幕绘制后它将自动从内存中删除
                SwapEffect = SwapEffect.Discard,
                //用于描述适配器刷新率与 Device 的 Present 运算完成速率之间的关系
                //One 驱动程序需要等待垂直回描周期（运行库将实施跟踪以防止脱节）。Present 运算受影响的频率不超过屏幕的刷新率；运行库在适配器的每个刷新周期内至多完成一次 Present 运算。对于窗口式和全屏交换链而言，此选项都始终可用。
                //Immediate 运行库将立即更新窗口工作区，并且，在适配器刷新周期内可能不止更新一次。Present 运算可能马上受到影响。对于窗口式和全屏交换链而言，此选项都始终可用
                PresentationInterval =  PresentInterval.One,
                //指示应用程序是否在窗口模式下运行。//如果应用程序运行时有窗口，则为 true，否则为 false
                Windowed = true
            };

            //检索特定于设备的信息。
            Caps devCaps = Manager.GetDeviceCaps(0, DeviceType.Hardware);

            // 指定设备类型。

            //---------------DeviceType------------
            //NullReference	4	一个空参考光栅器的版本。
            //Software	3	一个软件的设备。
            //Reference	2	微软Direct3D的特点是在软件中实现的；然而，参考光栅化程序尽可能使用特殊的CPU指令。
            //Hardware 1 一个硬件的设备
            DeviceType devType = DeviceType.Reference;
            //定义在创建设备时要使用的标志
            CreateFlags devFlags = CreateFlags.HardwareVertexProcessing; //指定硬件顶点处理
            //检索一个对象，该值指示主要和附属设备所支持的顶点着色器版本。 检索一个对象，该值指示主要和下属像素着色器版本。
            if (devCaps.VertexShaderVersion.Major >= 2 && devCaps.PixelShaderVersion.Major >= 2)
                //设备类型
                devType = DeviceType.Hardware;

            //NoWindowChanges	2048    微软Direct3D指示运行时不要以任何方式改变焦点的窗口。请谨慎使用！ 支持集中管理事件的负担(ALT标签等)瀑布上的应用，和适当的反应(切换显示模式等)应编码。
            //DisableDriverManagementEx	1024	 指定资源管理利用Direct3D代替设备。Direct3D调用资源不会失败的错误，如视频内存不足。
            //AdapterGroupDevice	512	
            //DisableDriverManagement	256	指定资源管理利用Direct3D代替设备。Direct3D调用资源不会失败的错误，如视频内存不足。
            //MixedVertexProcessing	128	指定混合顶点处理(包括软件和硬件)。
            //HardwareVertexProcessing	64	指定硬件顶点处理。
            //SoftwareVertexProcessing	32	指定软件顶点处理。
            //PureDevice	16	 如果设备不支持顶点处理，应用程序可以只使用后改变顶点。
            //MultiThreaded	4	指示应用程序请求的Direct3D的多线程安全。这会导致Direct3D更频繁地检查其全球关键节，这会降低性能。从Microsoft DirectX 9.0 SDK更新(2003夏天)，这总是指定枚举值，除非该参数四。ForceNoMultiThreadedFlag设置为true。
            //FpuPreserve	2	指示应用程序需要浮点单元(FPU)异常或双精度浮点异常启用。默认情况下，Direct3D使用单精度。 因为每次它被称为Direct3D设置FPU状态，设置此标志减少了Direct3D性能。


            //指示设备是否支持硬件转换和照明。
            if (devCaps.DeviceCaps.SupportsHardwareTransformAndLight)
                //指定硬件顶点处理。
                devFlags = CreateFlags.HardwareVertexProcessing;

            //指示设备是否支持光栅化、转换、照明和阴影在硬件。
            if (devCaps.DeviceCaps.SupportsPureDevice)
                //如果设备不支持顶点处理，应用程序可以只使用改变后的顶点。
                devFlags |= CreateFlags.PureDevice;

            //            参数
            //adapter
            //    一个标识对象表示哪个物理设备的序号。设备 0 表示默认设备。在此参数中可使用的最大值为物理设备总数减 1。

            //deviceType
            //    DeviceType 枚举类型的一个成员，它表示所需的设备类型。如果所需的设备类型不可用，则该方法将失败。

            //renderWindow
            //    Form 或任何其他 Control 派生类的句柄。此参数指示要绑定到设备的图面。
            //    指定的窗口必须是顶级窗口，Null 值不受支持。

            //behaviorFlags
            //    控制设备创建操作的一个或多个选项的组合。

            //presentationParameters
            //    一个 PresentParameters 对象，它描述要创建的设备的表示参数。
            Device = new Device(Manager.Adapters.Default.Adapter, devType, control, devFlags, Parameters);
            //在设备将要丢失时发生
            Device.DeviceLost += Device_DeviceLost;
            //设备调整大小时发生
            Device.DeviceResizing += Device_DeviceResizing;
            //重置设备之后发生
            Device.DeviceReset += Device_DeviceReset;
            //当调用 Dispose 方法时，或者当设备对象被终结并被垃圾回收器回收时。
            Device.Disposing += Device_Disposing;
            //Device.DeviceLost += (o, e) => DeviceLost = true;
            //Device.DeviceResizing += (o, e) => e.Cancel = true;
            //Device.DeviceReset += (o, e) => LoadTextures();
            //Device.Disposing += (o, e) => Clean();

            //允许使用Microsoft Windows图形设备接口(GDI)在全屏应用程序对话框。
            Device.SetDialogBoxesEnabled(true);

            LoadTextures();
        }

        #region Device 事件
        private static void Device_Disposing(object sender, EventArgs e)
        {
            Clean();
        }

        private static void Device_DeviceReset(object sender, EventArgs e)
        {
            LoadTextures();
        }

        private static void Device_DeviceResizing(object sender, CancelEventArgs e)
        {
          
            if (_control.Size==new Size(0,0) )
            {
                e.Cancel = true;
            }
            else
            {
                e.Cancel = false;
            }
        }

        private static void Device_DeviceLost(object sender, EventArgs e)
        {
            DeviceLost = true;
        }
        #endregion
        /// <summary>
        /// 加载纹理
        /// </summary>
        private static unsafe void LoadTextures()
        {//unsafe 关键字表示不安全上下文,该上下文是任何涉及指针的操作所必需的。

            //Sprite 提供的方法和属性用于简化使用 Direct3D 绘制子画面的过程。
            //初始化 Sprite 类的新实例。 
            //            参数
            //device
            //    Device 的一个实例。
            Sprite = new Sprite(Device);
            TextSprite = new Sprite(Device);
            //Line 类在两个点之间绘制一条直线。
            Line = new Line(Device) { Width = 1F };
            //获取指定的后台缓冲区。 
            //参数
            //swapChain
            //无符号整数，它指定交换链。

            //backBuffer
            //要返回的后台缓冲区对象的索引。

            //backBufferType
            //    要返回的后台缓冲区的类型。只有 Mono 是有效值。
            //返回值 ：一个 Surface，它表示返回的后台缓冲区图面。
            MainSurface = Device.GetBackBuffer(0, 0, BackBufferType.Mono);
            CurrentSurface = MainSurface;

            //SetRenderTarget 为设备设置新的颜色缓冲区。 
            // 参数

            //renderTarget
            //    呈现目标的索引 Surface。

            //newZStencil
            //    新颜色缓冲区 Surface。如果将该参数设置为 空引用（在 Visual Basic 中为 Nothing），则会禁用相应 renderTarget 的颜色缓冲区。设备必须始终与一个颜色缓冲区相关联。
            //    新呈现目标图面至少必须指定 Usage。
            Device.SetRenderTarget(0, MainSurface);


            //if (RadarTexture == null || RadarTexture.Disposed)
            //{
            //    //参数

            //    //device
            //    //    类型：Microsoft.WindowsMobile.DirectX.Direct3D.Device
            //    //    要与 Texture 关联的 Device 对象。

            //    //width
            //    //    类型：System.Int32
            //    //    纹理顶级的宽度（以像素为单位）。后续级别的像素尺寸是前一级别像素尺寸的一半的截整值（每个级别的计算都是独立进行的）。每个尺寸都限制在至少一个像素大小。因此，如果被 2 除后再进行截断所得到的结果为 0，则改为采用 1。

            //    //height
            //    //    类型：System.Int32
            //    //    纹理顶级的高度（以像素为单位）。后续级别的像素尺寸是前一级别像素尺寸的一半的截整值（每个级别的计算都是独立进行的）。每个尺寸都限制在至少一个像素大小。因此，如果被 2 除后再进行截断所得到的结果为 0，则改为采用 1。

            //    //numLevels
            //    //    类型：System.Int32
            //    //    纹理中的级别数。如果此值为 0，对于支持 mipmap 形式纹理的硬件，Direct3D 会向下生成纹理的所有子级，直至生成的纹理子级的大小为 1 x 1 像素。检查 BaseTexture.LevelCount 属性以查看生成的级别数。

            //    //usage
            //    //    类型：Microsoft.WindowsMobile.DirectX.Direct3D.Usage
            //    //    Usage 可以是 0，表示没有用法值。但是，如果需要 usage，则请使用一个或多个 Usage 常数。建议使 usage 参数与 Device 的构造函数中的 CreateFlags 匹配。

            //    //format
            //    //    类型：Microsoft.WindowsMobile.DirectX.Direct3D.Format
            //    //    一个 Format 值，用于描述纹理中所有级别的格式。

            //    //pool
            //    //    类型：Microsoft.WindowsMobile.DirectX.Direct3D.Pool
            //    //    一个 Pool 值，用于描述纹理应被放入的内存类。
            //    RadarTexture = new Texture(Device, 2, 2, 1, Usage.None, Format.A8R8G8B8, Pool.Managed);

            //    //GraphicsStream 包含图形数据流。   
            //    //LockRectangle 锁定纹理资源中的矩形。
            //    //参数

            //    //level
            //    //    类型：System.Int32
            //    //    要锁定的纹理资源的 minmap 级别。

            //    //flags
            //    //    类型：Microsoft.WindowsMobile.DirectX.Direct3D.LockFlags
            //    //    零个或多个 LockFlags 值，用于描述要执行的锁定的类型。对于此方法，有效的标志为 Discard、NoDirtyUpdate 和 ReadOnly。

            //    //返回值
            //    //   GraphicsStream，用于描述锁定的区域。 

            //    //-------------------LockFlags----------------------
            //    //None	应用程序既可以从缓冲区读取也可以向缓冲区写入。
            //    //ReadOnly	应用程序不向缓冲区写入。利用此标志，可以使以非本机格式存储的资源在解锁时省去重新压缩的步骤。	NoDirtyUpdate	默认情况下，资源上的锁向该资源添加“脏”区域。此标志禁止对资源的脏状态进行任何更改。当应用程序在锁定操作期间更改了关于一组区域的附加信息时，该应用程序应使用此标志。
            //    //NoOverwrite	确保应用程序不改写顶点和索引缓冲区中的任何数据。使用具有此标志的顶点缓冲区时，驱动程序可以立即返回并继续呈现。如果未使用此标志，则驱动程序必须先完成呈现，然后才能从锁定状态返回。
            //    //Discard	应用程序使用只写操作改写锁定区域内的每个位置。当使用动态纹理、动态顶点缓冲区和动态索引缓冲区时，此选项为有效选项。 
            //    using (GraphicsStream stream = RadarTexture.LockRectangle(0, LockFlags.Discard))

            //    //Bitmap封装 GDI+ 位图，此位图由图形图像及其特性的像素数据组成。 Bitmap 是用于处理由像素数据定义的图像的对象。

            //    //用指定的大小、像素格式和像素数据初始化 Bitmap 类的新实例
            //    //参数

            //    //width
            //    //    新 Bitmap 的宽度（以像素为单位）。

            //    //height
            //    //    新 Bitmap 的高度（以像素为单位）。

            //    //stride
            //    //    指定相邻扫描行开始处之间字节偏移量的整数。这通常（但不一定）是以像素格式表示的字节数（例如，2 表示每像素 16 位）乘以位图的宽度。传递给此参数的值必须为 4 的倍数。

            //    //format
            //    //    新 Bitmap 的像素格式。这必须指定以 Format 开头的值。

            //    //scan0
            //    //    指向包含像素数据的字节数组的指针。
            //    using (Bitmap image = new Bitmap(2, 2, 8, PixelFormat.Format32bppArgb, (IntPtr)stream.InternalDataPointer))
            //    //从指定的 Image 创建新的 Graphics。
            //    using (Graphics graphics = Graphics.FromImage(image))
            //        //清除整个绘图面并以指定背景色填充。
            //        graphics.Clear(Color.White);
            //}
            //if (PoisonDotBackground == null || PoisonDotBackground.Disposed)
            //{
            //    PoisonDotBackground = new Texture(Device, 5, 5, 1, Usage.None, Format.A8R8G8B8, Pool.Managed);
            //    //锁定纹理资源中的矩形。
            //    using (GraphicsStream stream = PoisonDotBackground.LockRectangle(0, LockFlags.Discard))
            //    //用指定的大小、像素格式和像素数据初始化 Bitmap 类的新实例
            //    using (Bitmap image = new Bitmap(5, 5, 20, PixelFormat.Format32bppArgb, (IntPtr)stream.InternalDataPointer))
            //    //从指定的 Image 创建新的 Graphics。
            //    using (Graphics graphics = Graphics.FromImage(image))
            //        //清除整个绘图面并以指定背景色填充
            //        graphics.Clear(Color.White);
            //}
            //CreateLights();
        }
        /// <summary>
        /// 创建光源
        /// </summary>
        private unsafe static void CreateLights()
        {

            for (int i = Lights.Count - 1; i >= 0; i--)
                Lights[i].Dispose();

            Lights.Clear();

            for (int i = 1; i < LightSizes.Length; i++)
            {
                // int width = 125 + (57 *i);
                //int height = 110 + (57 * i);
                int width = LightSizes[i].X;
                int height = LightSizes[i].Y;
                Texture light = new Texture(Device, width, height, 1, Usage.None, Format.A8R8G8B8, Pool.Managed);
                ////LockRectangle 锁定纹理资源中的矩形
                using (GraphicsStream stream = light.LockRectangle(0, LockFlags.Discard))
                //用指定的大小、像素格式和像素数据初始化 Bitmap 类的新实例
                using (Bitmap image = new Bitmap(width, height, width * 4, PixelFormat.Format32bppArgb, (IntPtr)stream.InternalDataPointer))
                {
                    //从指定的 Image 创建新的 Graphics。
                    using (Graphics graphics = Graphics.FromImage(image))
                    {
                        //GraphicsPath 表示一系列相互连接的直线和曲线
                        using (GraphicsPath path = new GraphicsPath())
                        {
                            //AddEllipse 向当前路径添加一个椭圆。
                            // 参数

                            //x
                            //    Type: System.Int32
                            //    定义椭圆的边框的左上角的 X 坐标。

                            //y
                            //    Type: System.Int32
                            //    定义椭圆的边框的左上角的 Y 坐标。

                            //width
                            //    定义椭圆的边框的宽度。

                            //height
                            //    定义椭圆的边框的高度。
                            path.AddEllipse(new Rectangle(0, 0, width, height));
                            //PathGradientBrush 类 封装 Brush 对象，它通过渐变填充 GraphicsPath 对象的内部
                            //  使用指定的路径初始化 PathGradientBrush 类的新实例。
                            //  参数
                            //path
                            //    GraphicsPath，定义此 PathGradientBrush 填充的区域。
                            using (PathGradientBrush brush = new PathGradientBrush(path))
                            {
                                graphics.Clear(Color.FromArgb(0, 0, 0, 0));
                                //获取或设置与此 PathGradientBrush 填充的路径中的点相对应的颜色的数组。
                                brush.SurroundColors = new[] { Color.FromArgb(0, 255, 255, 255) };
                                //获取或设置路径渐变的中心处的颜色
                                brush.CenterColor = Color.FromArgb(255, 255, 255, 255);
                                //填充 GraphicsPath 的内部。
                                //参数

                                //brush
                                //    确定填充特性的 Brush。

                                //path
                                //    GraphicsPath，它表示要填充的路径。
                                graphics.FillPath(brush, path);
                                //保存此 Graphics 的当前状态，并用 GraphicsState 标识保存的状态。
                                graphics.Save();
                            }
                        }
                    }
                }
                //添加light
                Lights.Add(light);
                //为light添加 Disposing 事件 ，用来Lights.Remove(light);
                light.Disposing += (o, e) => Lights.Remove(light);
            }
        }
        /// <summary>
        /// 设置表面
        /// </summary>
        /// <param name="surface"></param>
        public static void SetSurface(Surface surface)
        {
            if (CurrentSurface == surface)
                return;
            //强制将所有批子画面都提交给设备。 
            Sprite.Flush();
            CurrentSurface = surface;
            //为设备设置新的颜色缓冲区。 
            Device.SetRenderTarget(0, surface);
        }
        /// <summary>
        /// 尝试重置
        /// </summary>
        public static void AttemptReset()
        {
            try
            {
                int result;
                //CheckCooperativeLevel 为窗口应用程序或全屏应用程序报告 Direct3D 设备的当前协作级别状态。 
                //参数

                //hResult
                //    窗口应用程序或全屏应用程序的设备的当前协作级别状态，用 ResultCode 值进行报告。Success 结果指示设备可操作并且调用应用程序可继续。DeviceLost 结果指示设备已丢失，但是此时无法重置；因此无法进行呈现。DeviceNotReset 结果指示设备已丢失，但是此时可以重置。

                //返回值
                //如果设备可操作并且调用应用程序可以继续，则为 true；如果设备已丢失或者需要重置，则为 false。 
                Device.CheckCooperativeLevel(out result);

                //    -------------ResultCode)result 返回值类型-------------
                //    AlreadyLocked	设备已锁定。 
                //    ConflictingRenderState	呈现状态不兼容。 
                //    ConflictingTextureFilter	当前的纹理筛选器不能一起使用。 
                //    ConflictingTexturePalette	当前的纹理调色板不能一起使用。 
                //    DeviceLost	设备已丢失，但此时无法对其进行重置。因此，不可能进行呈现。 
                //    DeviceNotReset	设备不支持所查询的技术。 
                //    DriverInternalError	发生驱动程序内部错误。 
                //	    DriverInvalidCall	驱动程序检测到无效的调用。 
                // 	DriverUnsupported	驱动程序不受支持。 
                // 	GenericFailure	常规失败。 
                // 	InvalidCall	方法调用无效；例如，方法的参数可能不是有效的指针。 
                // 	InvalidDevice	请求的设备类型无效。 
                // 	MemoryPoolEmpty	指定的内存池是空的。 
                // 	MoreData	传入例程的缓冲区所含元素数量不足，无法完成该操作。 
                // 	NotAvailable	请求的格式不可用。 
                // 	NotFound	搜索例程未能返回元素。 

                // 	Success	操作成功。 

                // 	TooManyOperations	应用程序请求的纹理筛选操作数量多于设备支持的纹理筛选操作数量。 
                // 	UnsupportedAlphaArgument	设备不支持 alpha 通道的指定纹理混合参数。 
                // 	UnsupportedAlphaOperation	设备不支持 alpha 通道的指定纹理混合操作。 
                // 	UnsupportedColorArgument	设备不支持颜色值的指定纹理混合参数。 
                //	    UnsupportedColorOperation	设备不支持颜色值的指定纹理混合操作。 
                // 	UnsupportedFactorValue	设备不支持指定的纹理因子值。 
                // 	UnsupportedTextureFilter	设备不支持指定的纹理筛选器。 
                //    WrongTextureFormat	像素格式的纹理图面无效。 
                switch ((ResultCode)result)
                {
                    //设备不支持所查询的技术。 
                    case ResultCode.DeviceNotReset:
                        //Device.Reset(Parameters)重置当前设备的表示参数。 
                        //参数

                        //presentationParameters
                        //    一个 PresentParameters 结构，它描述新的表示参数。此参数不能为空。

                        //备注
                        //切换到全屏模式时，Direct3D 会尝试找到一种与后台缓冲区格式匹配的桌面格式，以便后台缓冲区格式和前台缓冲区格式相同。这样就无需转换颜色了。
                        //如果对 Reset 的调用失败，除非设备处于“未重置”状态（由从 CheckCooperativeLevel 方法的 hResult 参数返回的 DeviceNotReset 来指示），否则设备会被置于“丢失”状态（由从对 CheckCooperativeLevel 的调用返回的值 false 指示）。
                        //调用 Reset 将导致所有纹理内存图面和状态信息丢失，并且还会导致托管纹理在视频内存中被刷新。在对设备调用 Reset 前，应用程序应释放任何与该设备相关联的显式呈现目标、深度模具图面、附加交换链、状态块以及默认资源。
                        //交换链可以是全屏的，也可以是窗口式的。如果新的交换链是全屏的，则会将适配器置于与新大小匹配的显示模式中。
                        //如果调用 Reset 的线程不是用来创建所要重置的设备的线程，则该调用将失败。
                        //调用 Device、Reset 和 SwapChain 时，可以将窗口模式后台缓冲区的格式指定为“未知”。这意味着应用程序在窗口模式下调用 Device 前不必查询当前的桌面格式。对于全屏模式，必须指定后台缓冲区格式。将 BackBufferCount 设置为 0 会创建一个后台缓冲区。
                        //尝试成组重置多个显示适配器时，可传入 PresentParameters 对象数组，该数组中的每个对象都对应于该适配器组中的一个显示适配器。
                        Device.Reset(Parameters);
                        break;
                    //设备已丢失，但此时无法对其进行重置。因此，不可能进行呈现。
                    case ResultCode.DeviceLost:
                        break;
                    //操作成功
                    case ResultCode.Success:
                        DeviceLost = false;
                        //获取指定的后台缓冲区。 
                        MainSurface = Device.GetBackBuffer(0, 0, BackBufferType.Mono);
                        //获取指定的后台缓冲区
                        CurrentSurface = Device.GetBackBuffer(0, 0, BackBufferType.Mono);
                        //为设备设置新的颜色缓冲区。
                        Device.SetRenderTarget(0, CurrentSurface);
                        break;
                }
            }
            catch
            {
            }
        }
        /// <summary>
        /// 尝试恢复
        /// </summary>
        public static void AttemptRecovery()
        {
            try
            {
                //将设备还原为调用 Begin 前的状态。
                //备注
                //此方法不能用于替代 Device 的 EndScene。

                Sprite.End();
                TextSprite.End();
            }
            catch
            {
            }

            try
            {
                //结束通过调用 BeginScene 方法开始的场景。 
                //备注
                //每个对 BeginScene 的调用后面最终都应有一个对 EndScene 的调用，然后才能用 Present 更新显示。
                //当 EndScene 成功时，该场景将进入由驱动程序进行呈现的队列中。该方法不是同步的，因此不能保证该方法返回时，场景已完成呈现。

                Device.EndScene();
            }
            catch
            {
            }

            try
            {
                ////获取指定的后台缓冲区
                MainSurface = Device.GetBackBuffer(0, 0, BackBufferType.Mono);
                CurrentSurface = MainSurface;
                ////为设备设置新的颜色缓冲区。
                Device.SetRenderTarget(0, MainSurface);
            }
            catch
            {
            }
        }
        /// <summary>
        /// 设置不透明度
        /// </summary>
        /// <param name="opacity"> 不透明度</param>
        public static void SetOpacity(float opacity)
        {
            if (Opacity == opacity)
                return;
            //强制将所有批子画面都提交给设备。 
            Sprite.Flush();
            //获取设备的呈现状态值。AlphaBlendEnable= true;
            Device.RenderState.AlphaBlendEnable = true;
            if (opacity >= 1 || opacity < 0)
            {

                //----------Blend  枚举 ------------
                //DestinationAlpha	混合因子为 (Ad, Ad, Ad, Ad)。 
                //	DestinationColor	混合因子为 (Rd, Gd, Bd, Ad)。 
                //	InvDestinationAlpha	混合因子为 (1 - Ad, 1 - Ad, 1 - Ad, 1 - Ad)。 
                //	InvDestinationColor	混合因子为 (1 - Rd, 1 - Gd, 1 - Bd, 1 - Ad)。 
                //	InvSourceAlpha	混合因子为 ( 1 - As, 1 - As, 1 - As, 1 - As)。 
                //	InvSourceColor	混合因子为 (Rs, Gs, Bs, As)。 
                //	One	混合因子为 (1,1,1,1)。 
                //	SourceAlpha	混合因子为 (As, As, As, As)。 
                //	SourceAlphaSat	混合因子为 (f, f, f, 1)；f = min(A, 1 - Ad)。 
                //	SourceColor	混合因子为 (Rs, Gs, Bs, As)。 
                //	Zero	混合因子为 (0, 0, 0, 0)。 

                //获取或设置颜色混合模式。 
                Device.RenderState.SourceBlend = Blend.SourceAlpha;
                //表示当前的混合模式或要设置的混合模式
                Device.RenderState.DestinationBlend = Blend.InvSourceAlpha;
                //获取或者设置Alpha混合模式
                Device.RenderState.AlphaSourceBlend = Blend.One;
                //使用现有颜色混合绘制操作
                Device.RenderState.BlendFactor = Color.FromArgb(255, 255, 255, 255);
            }
            else
            {
                Device.RenderState.SourceBlend = Blend.BlendFactor;
                Device.RenderState.DestinationBlend = Blend.InvBlendFactor;
                Device.RenderState.AlphaSourceBlend = Blend.SourceAlpha;
                Device.RenderState.BlendFactor = Color.FromArgb((byte)(255 * opacity), (byte)(255 * opacity),
                                                                (byte)(255 * opacity), (byte)(255 * opacity));
            }
            Opacity = opacity;
            //强制将所有批子画面都提交给设备。
            Sprite.Flush();
        }
        /// <summary>
        /// 设置混合
        /// </summary>
        /// <param name="value"> 是否</param>
        /// <param name="rate">率</param>
        public static void SetBlend(bool value, float rate = 1F)
        {
            if (value == Blending) return;
            Blending = value;
            //强制将所有批子画面都提交给设备。
            Sprite.Flush();
            // //将设备还原为调用 Begin 前的状态。
            Sprite.End();

            if (Blending)
            {
                //Sprite.Begin准备子画面的绘制 
                //参数

                //flags
                //来自 SpriteFlags 的零个或多个值的组合，这些值描述子画面呈现选项。

                // AlphaBlend	启用 AlphaTestEnable 设为 true（对于非零 alpha）的 alpha 混合。SourceAlpha 是源混合状态，InvSourceAlpha 是 RenderStateManager 调用中的目标混合状态。绘制文本时，Font 类要求设置此标志。 
                // Billboard	围绕每个子画面的中心旋转该子画面，以便使其面向查看者。必须首先调用 SetWorldViewLH 或 SetWorldViewRH。 
                // DoNotModifyRenderState	指定调用 Begin 时不更改设备呈现状态。  
                // DoNotSaveState	禁止在调用 Begin 和 End 时保存或还原设备状态。 
                // None	将值解析为 0。 
                // ObjectSpace	指定不修改世界、视图和投影转换。当前为设备设置的转换用于在成批绘制子画面时（即，调用 Begin 或 End 时）转换这些子画面。如果未指定此选项，则修改世界、视图和投影转换，以便以屏幕空间坐标绘制子画面。 
                // SortDepthBackToFront	绘制前，按深度从后到前的顺序对子画面进行排序。在绘制不同深度的透明子画面时，建议使用此选项。 
                // SortDepthFrontToBack	绘制前，按深度从前到后的顺序对子画面进行排序。在绘制不同深度的不透明子画面时，建议使用此选项。 
                // SortTexture	绘制前，按纹理子画面进行排序。在绘制统一深度的不重叠子画面时，建议使用此选项；例如，用 Font 绘制屏幕对齐的文本时。 

                Sprite.Begin(SpriteFlags.DoNotSaveState);//禁止在调用 Begin 和 End 时保存或还原设备状态。 
                Device.RenderState.AlphaBlendEnable = true;
                Device.RenderState.SourceBlend = Blend.BlendFactor;
                Device.RenderState.DestinationBlend = Blend.One;
                Device.RenderState.BlendFactor = Color.FromArgb((byte)(255 * rate), (byte)(255 * rate),
                                                                (byte)(255 * rate), (byte)(255 * rate));
            }
            else
                //Sprite.Begin准备子画面的绘制
                Sprite.Begin(SpriteFlags.AlphaBlend);//启用 AlphaTestEnable 设为 true（对于非零 alpha）的 alpha 混合。SourceAlpha 是源混合状态，InvSourceAlpha 是 RenderStateManager 调用中的目标混合状态。绘制文本时，Font 类要求设置此标志。 
            //SetRenderTarget 为设备设置新的颜色缓冲区。
            Device.SetRenderTarget(0, CurrentSurface);
        }
        /// <summary>
        /// 清除
        /// </summary>
        public static void Clean()
        {
            for (int i = TextureList.Count - 1; i >= 0; i--)
            {
                MLibrary.MImage m = TextureList[i];

                if (m == null)
                {
                    TextureList.RemoveAt(i);
                    continue;
                }

                //if (CMain.Time <= m.CleanTime) continue;


                TextureList.RemoveAt(i);
                //if (m.Image != null && !m.Image.Disposed)
                //    m.Image.Dispose();
            }
        }
    }
}
