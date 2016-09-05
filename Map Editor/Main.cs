using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Runtime.InteropServices;
using System.Security;
using System.Threading;
using System.Windows.Forms;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using Font = System.Drawing.Font;

namespace Map_Editor
{
    public partial class Main : Form
    {
        public delegate void DelJump(int x, int y);

        public delegate void DelSetAnimationProperty(bool blend, byte frame, byte tick);

        public delegate void DelSetDoorProperty(bool door, byte index, byte offSet);

        public delegate void DelSetLightProperty(byte light);

        public delegate void DelSetMapSize(int w, int h);

        private const int CellWidth = 48;
        private const int CellHeight = 32;
        private const int Mir2BigTileBlock = 50;
        private const int Mir3BigTileBlock = 30;
        private const int smTileBlock = 60;
        public static Font font = new Font("Tahoma", 10, FontStyle.Bold);
        private static int zoomMIN;
        private static readonly int zoomMAX = zoomMIN = 20;

        public static readonly Stopwatch Timer = Stopwatch.StartNew();
        public static readonly DateTime StartTime = DateTime.Now;
        public static long Time, OldTime;
        private static long _fpsTime;
        private static int _fps;
        public static int FPS;
        public static long MoveTime;
        private static readonly Vector2[] vector2S = new Vector2[5];
        private static readonly Vector2[] line = new Vector2[2];

        private static readonly Random random = new Random();

        private static int AutoTileRange;
        private static int AutoTileChanges;
        private readonly Editor _editor = new Editor();
        private readonly Dictionary<int, int> _shandaMir2IndexList = new Dictionary<int, int>();
        private readonly Dictionary<int, int> _shandaMir3IndexList = new Dictionary<int, int>();
        private readonly Dictionary<int, int> _tilesIndexList = new Dictionary<int, int>();
        private readonly Dictionary<int, int> _wemadeMir2IndexList = new Dictionary<int, int>();
        private readonly Dictionary<int, int> _wemadeMir3IndexList = new Dictionary<int, int>();
        private readonly List<CellInfoData> bigTilePoints = new List<CellInfoData>();
        private readonly CellInfoControl cellInfoControl = new CellInfoControl();
        private readonly int[] Mir2BigTilesPreviewIndex = {5, 15, 6, 20, 0, 21, 7, 17, 8};
        private readonly int[] Mir3BigTilesPreviewIndex1 = {10, 20, 11, 25, 0, 26, 12, 22, 13};
        private readonly int[] Mir3BigTilesPreviewIndex2 = {18, 22, 17, 26, 5, 27, 16, 20, 15};
        private readonly List<CellInfoData> smTilePoints = new List<CellInfoData>();
        private readonly int[] smTilesPreviewIndex = {39, 11, 15, 35, 0, 19, 31, 25, 23};
        public int AnimationCount;
        private CellInfoData[] cellInfoDatas;
        private int cellX, cellY;
        private int drawY, drawX, libIndex, index;
        private Graphics graphics;

        private bool Grasping;
        private bool keyDown;

        private Layer layer = Layer.None;

        public CellInfo[,] M2CellInfo;
        private MapReader map;
        private string mapFileName;
        private Point mapPoint;

        private int mapWidth, mapHeight;
        public CellInfo[,] NewCellInfo;
        private CellInfoData[] objectDatas;

        private int OffSetX;
        private int OffSetY;

        private Point p1, p2;
        private int selectImageIndex;
        private MLibrary.MImage selectLibMImage;
        private ListItem selectListItem;
        private int selectTilesIndex = -1;

        private ListItem shangdaMir2ListItem;
        private ListItem shangdaMir3ListItem;

        private CellInfoData[] unTemp, reTemp;

        private ListItem wemadeMir2ListItem;
        private ListItem wemadeMir3ListItem;

        public Main()
        {
            InitializeComponent();

            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);

            Application.Idle += Application_Idle;
        }

        private void Application_Idle(object sender, EventArgs e)
        {
            try
            {
                while (AppStillIdle)
                {
                    UpdateTime();
                    UpdateEnviroment();
                    RenderEnviroment();
                }
            }
            catch (Exception ex)
            {
            }
        }

        private void UpdateTime()
        {
            Time = Timer.ElapsedMilliseconds;
        }

        private void UpdateEnviroment()
        {
            if (Time >= _fpsTime)
            {
                _fpsTime = Time + 1000;
                FPS = _fps;
                _fps = 0;
            }
            else
                _fps++;

            if (Time >= MoveTime)
            {
                MoveTime += 100;

                AnimationCount++;
            }

            Text = string.Format("FPS: {0}---Map:W {1}:H {2} ----W,S,A,D，观察地图<{3}>", FPS, mapWidth, mapHeight,
                mapFileName);
        }

        private void RenderEnviroment()
        {
            try
            {
                if (DXManager.DeviceLost)
                {
                    DXManager.AttemptReset();
                    Thread.Sleep(1);
                    return;
                }

                if (M2CellInfo == null)
                {
                }
                else
                {
                    DXManager.Device.Clear(ClearFlags.Target, Color.White, 0, 0);
                    DXManager.Device.BeginScene();
                    DXManager.Sprite.Begin(SpriteFlags.AlphaBlend);
                    DXManager.TextSprite.Begin(SpriteFlags.AlphaBlend);

                    OffSetX = MapPanel.Width/(CellWidth*zoomMIN/zoomMAX);
                    OffSetY = MapPanel.Height/(CellHeight*zoomMIN/zoomMAX);


                    //back
                    DrawBack(chkBack.Checked);
                    //midd
                    DrawMidd(chkMidd.Checked);
                    //front
                    DrawFront(chkFront.Checked);
                    //Draw Select Object
                    DrawObject(objectDatas);
                    //Draw Select TextureImage
                    DrawSelectTextureImage();
                    //Draw Limit
                    DrawLimit();
                    //门标记
                    DrawDoorTag(chkDoorSign.Checked);
                    //前景动画标记  
                    DrawFrontAnimationTag(chkFrontAnimationTag.Checked);
                    //背景动画标记
                    DrawMiddleAnimationTag(chkMiddleAnimationTag.Checked);
                    //亮光标记
                    DrawLightTag(chkLightTag.Checked);
                    //背景移动限制
                    DrawBackLimit(chkBackMask.Checked);
                    //前景移动限制
                    DrawFrontMask(chkFrontMask.Checked);
                    //前景标记
                    DrawFrontTag(chkFrontTag.Checked);
                    //中间层标记
                    DrawMiddleTag(chkMiddleTag.Checked);

                    DXManager.Sprite.End();
                    DXManager.TextSprite.End();

                    //网格
                    //4800 条短线版本 画格子
                    DrawGrids(chkDrawGrids.Checked);
                    //1200 条长线版本 画长线，交叉，变成格子
                    //DrawGrids2(chkDrawGrids.Checked);
                    //画选择的矩形
                    GraspingRectangle();

                    //DXManager.Sprite.End();
                    //DXManager.TextSprite.End();

                    DXManager.Device.EndScene();
                    DXManager.Device.Present();
                }
            }
            catch (DeviceLostException)
            {
            }
            catch (Exception ex)
            {
                DXManager.AttemptRecovery();
            }
        }

        [DllImport("kernel32.dll", EntryPoint = "SetProcessWorkingSetSize", ExactSpelling = true, CharSet =
            CharSet.Ansi, SetLastError = true)]
        private static extern int SetProcessWorkingSetSize(IntPtr process, int minimumWorkingSetSize,
            int maximumWorkingSetSize);

        public new void Dispose()
        {
            GC.Collect();
            GC.SuppressFinalize(this);

            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                SetProcessWorkingSetSize(Process.GetCurrentProcess().Handle, -1, -1);
            }
        }

        public void SetMapSize(int w, int h)
        {
            mapWidth = w;
            mapHeight = h;
            graphics = Graphics.FromHwnd(MapPanel.Handle);
        }

        private void Main_Load(object sender, EventArgs e)
        {
            Libraries.LoadGameLibraries();

            ReadShandaMir2LibToListBox();
            ReadWemadeMir2LibToListBox();
            ReadWemadeMir3LibToListBox();
            ReadShandaMir3LibToListBox();

            ReadObjectsToListBox();

            DXManager.Create(MapPanel);
        }


        private void Draw(int libIndex, int index, int drawX, int drawY)
        {
            Libraries.MapLibs[libIndex].CheckImage(index);
            var mi = Libraries.MapLibs[libIndex].Images[index];
            if (mi.Image == null || mi.ImageTexture == null) return;
            int w = mi.Width;
            int h = mi.Height;
            DXManager.Sprite.Draw2D(mi.ImageTexture, Rectangle.Empty, new SizeF(w*zoomMIN/zoomMAX, h*zoomMIN/zoomMAX), new PointF(drawX, drawY), Color.White);

            //DXManager.Sprite.Draw2D(mi.ImageTexture, new Rectangle(Point.Empty, new Size(w * zoomMIN / zoomMAX, h * zoomMIN / zoomMAX)), new Rectangle(Point.Empty, new Size(w * zoomMIN / zoomMAX, h * zoomMIN / zoomMAX)), new Point(drawX, drawY), Color.White);
        }

        public void DrawBlend(int libindex, int index, Point point, Color colour, bool offSet = false, float rate = 1f)
        {
            Libraries.MapLibs[libIndex].CheckImage(index);
            var mi = Libraries.MapLibs[libIndex].Images[index];
            if (mi.Image == null || mi.ImageTexture == null) return;
            int w = mi.Width;
            int h = mi.Height;

            if (offSet) point.Offset(mi.X*zoomMIN/zoomMAX, mi.Y*zoomMIN/zoomMAX);
            var oldBlend = DXManager.Blending;
            DXManager.SetBlend(true, rate);
            DXManager.Sprite.Draw2D(mi.ImageTexture, Rectangle.Empty, new SizeF(w*zoomMIN/zoomMAX, h*zoomMIN/zoomMAX), point, Color.White);

            //DXManager.Sprite.Draw2D(mi.ImageTexture, new Rectangle(Point.Empty, new Size(w * zoomMIN / zoomMAX, h * zoomMIN / zoomMAX)), new Rectangle(Point.Empty, new Size(w * zoomMIN / zoomMAX, h * zoomMIN / zoomMAX)), point, Color.White);
            DXManager.SetBlend(oldBlend);
        }

        private string GetLibName(int index)
        {
            if (index < 0 || index >= Libraries.ListItems.Length)
            {
                return string.Empty;
            }
            try
            {
                return Libraries.ListItems[index].Text;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        private void MapPanel_MouseMove(object sender, MouseEventArgs e)
        {
            //
            if (M2CellInfo == null) return;
            var p = MapPanel.PointToClient(MousePosition);
            cellX = p.X/(CellWidth*zoomMIN/zoomMAX) + mapPoint.X;
            cellY = p.Y/(CellHeight*zoomMIN/zoomMAX) + mapPoint.Y;

            //
            if (cellX >= mapWidth || cellY >= mapHeight || cellX < 0 || cellY < 0)
            {
                return;
            }

            //
            ShowCellInfo(chkShowCellInfo.Checked);
            //
            if (keyDown)
            {
                MapPanel_MouseClick(sender, e);
            }
            //
            if (M2CellInfo != null)
            {
                switch (layer)
                {
                    case Layer.GraspingMir2Front:
                        if (Grasping)
                        {
                            if (p1.IsEmpty)
                            {
                                return;
                            }
                            p2 = new Point(cellX, cellY);
                        }
                        break;
                    case Layer.GraspingInvertMir3FrontMiddle:
                        if (Grasping)
                        {
                            if (p1.IsEmpty)
                            {
                                return;
                            }
                            p2 = new Point(cellX, cellY);
                        }
                        break;
                }
            }
        }

        private void DrawLimit()
        {
            var drawX = (cellX - mapPoint.X)*(CellWidth*zoomMIN/zoomMAX);
            var drawY = (cellY - mapPoint.Y)*(CellHeight*zoomMIN/zoomMAX);
            switch (layer)
            {
                case Layer.BackLimit:
                    Draw(1, 58, drawX, drawY);
                    break;
                case Layer.FrontLimit:
                    Draw(1, 59, drawX, drawY);
                    break;
                case Layer.BackFrontLimit:
                    Draw(1, 58, drawX, drawY);
                    Draw(1, 59, drawX, drawY);
                    break;
            }
        }

        private void DrawSelectTextureImage()
        {
            if (selectLibMImage == null) return;
            if (selectLibMImage.ImageTexture == null) return;
            if (selectListItem == null) return;
            if (layer == Layer.MiddleImage || layer == Layer.FrontImage)
            {
                var libIndex = selectListItem.Value;
                var index = selectImageIndex;
                var drawX = (cellX - mapPoint.X)*(CellWidth*zoomMIN/zoomMAX);
                var drawY = (cellY - mapPoint.Y)*(CellHeight*zoomMIN/zoomMAX);
                var s = Libraries.MapLibs[libIndex].GetSize(index);
                if ((s.Width != CellWidth || s.Height != CellHeight) &&
                    (s.Width != CellWidth*2 || s.Height != CellHeight*2))
                {
                    drawY = (cellY - mapPoint.Y + 1)*(CellHeight*zoomMIN/zoomMAX);
                    Draw(libIndex, index, drawX, drawY - s.Height*zoomMIN/zoomMAX);
                }
                else
                {
                    drawY = (cellY - mapPoint.Y)*(CellHeight*zoomMIN/zoomMAX);
                    Draw(libIndex, index, drawX, drawY);
                }
            }
            else if (layer == Layer.BackImage)
            {
                var temp = CheckPointIsEven(new Point(cellX, cellY));
                cellX = temp.X;
                cellY = temp.Y;
                var libIndex = selectListItem.Value;
                var index = selectImageIndex;
                var drawX = (cellX - mapPoint.X)*(CellWidth*zoomMIN/zoomMAX);
                var drawY = (cellY - mapPoint.Y)*(CellHeight*zoomMIN/zoomMAX);
                var s = Libraries.MapLibs[libIndex].GetSize(index);
                if ((s.Width != CellWidth || s.Height != CellHeight) &&
                    (s.Width != CellWidth*2 || s.Height != CellHeight*2))
                {
                    drawY = (cellY - mapPoint.Y + 1)*(CellHeight*zoomMIN/zoomMAX);
                    Draw(libIndex, index, drawX, drawY - s.Height*zoomMIN/zoomMAX);
                }
                else
                {
                    drawY = (cellY - mapPoint.Y)*(CellHeight*zoomMIN/zoomMAX);
                    Draw(libIndex, index, drawX, drawY);
                }
            }
        }

        private void DrawObject(CellInfoData[] datas)
        {
            if (datas == null) return;
            if (layer != Layer.PlaceObjects) return;


            for (var i = 0; i < datas.Length; i++)
            {
                if (datas[i].X%2 != 0) continue;
                if (datas[i].Y%2 != 0) continue;
                if (datas[i].X + cellX >= mapWidth) continue;
                if (datas[i].Y + cellY >= mapWidth) continue;

                drawX = (datas[i].X + cellX - mapPoint.X)*(CellWidth*zoomMIN/zoomMAX);
                drawY = (datas[i].Y + cellY - mapPoint.Y)*(CellHeight*zoomMIN/zoomMAX);
                index = (datas[i].CellInfo.BackImage & 0x1FFFFFFF) - 1;
                libIndex = datas[i].CellInfo.BackIndex;
                if (libIndex < 0 || libIndex >= Libraries.MapLibs.Length) continue;
                if (index < 0 || index >= Libraries.MapLibs[libIndex].Images.Count) continue;
                Draw(libIndex, index, drawX, drawY);
            }


            for (var i = 0; i < datas.Length; i++)
            {
                byte animation;
                bool blend;
                if (datas[i].X + cellX >= mapWidth) continue;
                if (datas[i].Y + cellY >= mapWidth) continue;

                drawX = (datas[i].X + cellX - mapPoint.X)*(CellWidth*zoomMIN/zoomMAX);
                index = datas[i].CellInfo.MiddleImage - 1;
                libIndex = datas[i].CellInfo.MiddleIndex;
                if (libIndex < 0 || libIndex >= Libraries.MapLibs.Length) continue;
                if (index < 0 || index >= Libraries.MapLibs[libIndex].Images.Count) continue;

                animation = datas[i].CellInfo.MiddleAnimationFrame;
                blend = false;
                if ((animation > 0) && (animation < 255))
                {
                    if ((animation & 0x0f) > 0)
                    {
                        blend = true;
                        animation &= 0x0f;
                    }
                    if (animation > 0)
                    {
                        var animationTick = datas[i].CellInfo.MiddleAnimationTick;
                        index += AnimationCount%(animation + animation*animationTick)/(1 + animationTick);
                    }
                }

                var s = Libraries.MapLibs[libIndex].GetSize(index);
                if ((s.Width != CellWidth || s.Height != CellHeight) &&
                    (s.Width != CellWidth*2 || s.Height != CellHeight*2))
                {
                    drawY = (datas[i].Y + cellY - mapPoint.Y + 1)*(CellHeight*zoomMIN/zoomMAX);
                    Draw(libIndex, index, drawX, drawY - s.Height*zoomMIN/zoomMAX);
                }
                else
                {
                    drawY = (datas[i].Y + cellY - mapPoint.Y)*(CellHeight*zoomMIN/zoomMAX);
                    Draw(libIndex, index, drawX, drawY);
                }
                if ((datas[i].CellInfo.MiddleImage & 0x7FFF) - 1 >= 0)
                {
                    drawY = (datas[i].Y + cellY - mapPoint.Y)*(CellHeight*zoomMIN/zoomMAX);
                    Draw(1, 56, drawX, drawY);
                }
            }

            for (var i = 0; i < datas.Length; i++)
            {
                if (datas[i].X + cellX >= mapWidth) continue;
                if (datas[i].Y + cellY >= mapWidth) continue;

                drawX = (datas[i].X + cellX - mapPoint.X)*(CellWidth*zoomMIN/zoomMAX);
                drawY = (datas[i].Y + cellY - mapPoint.Y)*(CellHeight*zoomMIN/zoomMAX);
                index = (datas[i].CellInfo.FrontImage & 0x7FFF) - 1;
                libIndex = datas[i].CellInfo.FrontIndex;
                if (libIndex < 0 || libIndex >= Libraries.MapLibs.Length) continue;
                if (index < 0 || index >= Libraries.MapLibs[libIndex].Images.Count) continue;
                var s = Libraries.MapLibs[libIndex].GetSize(index);
                byte animation;
                bool blend;
                animation = datas[i].CellInfo.FrontAnimationFrame;
                if ((animation & 0x80) > 0)
                {
                    blend = true;
                    animation &= 0x7F;
                }
                else
                {
                    blend = false;
                }

                if (animation > 0)
                {
                    var animationTick = datas[i].CellInfo.FrontAnimationTick;
                    index += AnimationCount%(animation + animation*animationTick)/(1 + animationTick);
                }

                //不是 48*32 或96*64 的地砖 是大物体
                if ((s.Width != CellWidth || s.Height != CellHeight) &&
                    (s.Width != CellWidth*2 || s.Height != CellHeight*2))
                {
                    drawY = (datas[i].Y + cellY - mapPoint.Y + 1)*(CellHeight*zoomMIN/zoomMAX);
                    //如果有动画
                    if (animation > 0)
                    {
                        //如果需要混合
                        if (blend)
                        {
                            //新盛大地图
                            if ((libIndex > 99) & (libIndex < 199))
                            {
                                DrawBlend(libIndex, index, new Point(drawX, drawY - 3*CellHeight*zoomMIN/zoomMAX),
                                    Color.White, true);
                            }
                            //老地图灯柱 index >= 2723 && index <= 2732
                            else
                            {
                                DrawBlend(libIndex, index, new Point(drawX, drawY - s.Height*zoomMIN/zoomMAX),
                                    Color.White, index >= 2723 && index <= 2732);
                            }
                        }
                        //不需要混合
                        else
                        {
                            Draw(libIndex, index, drawX, drawY - s.Height*zoomMIN/zoomMAX);
                        }
                    }
                    //如果没动画 
                    else
                    {
                        Draw(libIndex, index, drawX, drawY - s.Height*zoomMIN/zoomMAX);
                    }
                }
                //是 48*32 或96*64 的地砖
                else
                {
                    drawY = (datas[i].Y + cellY - mapPoint.Y)*(CellHeight*zoomMIN/zoomMAX);
                    Draw(libIndex, index, drawX, drawY);
                }


                if ((datas[i].CellInfo.FrontImage & 0x7FFF) - 1 >= 0)
                {
                    drawY = (datas[i].Y + cellY - mapPoint.Y)*(CellHeight*zoomMIN/zoomMAX);
                    Draw(1, 56, drawX, drawY);
                }
            }
        }

        private void DrawGrids(bool blGrids)
        {
            if (blGrids)
            {
                if (FPS < 25) return;
                for (var y = mapPoint.Y; y <= mapPoint.Y + OffSetY + 2; y++)
                {
                    if (y >= mapHeight || y < 0) continue;
                    drawY = (y - mapPoint.Y)*(CellHeight*zoomMIN/zoomMAX);
                    for (var x = mapPoint.X; x <= mapPoint.X + OffSetX + 2; x++)
                    {
                        if (x >= mapWidth || x < 0) continue;
                        drawX = (x - mapPoint.X)*(CellWidth*zoomMIN/zoomMAX);
                        DrawCube(drawX, drawY);
                        //Draw(99, 1, drawX, drawY);
                    }
                }
            }
        }

        private void DrawGrids2(bool blGrids)
        {
            if (blGrids)
            {
                if (FPS < 25) return;
                for (var x = 0; x <= OffSetX; x++)
                {
                    if (x >= mapHeight || x < 0) continue;
                    for (var y = 0; y <= OffSetY; y++)
                    {
                        if (y >= mapWidth || y < 0) continue;
                        DrawHorizontalLine(x, y);
                        DrawVerticalLine(x, y);
                    }
                }
            }
        }

        private void DrawCube(int x, int y)
        {
            vector2S[0] = new Vector2(x, y);
            vector2S[1] = new Vector2(CellWidth*zoomMIN/zoomMAX, y);
            vector2S[2] = new Vector2(CellWidth*zoomMIN/zoomMAX, CellHeight*zoomMIN/zoomMAX);
            vector2S[3] = new Vector2(x, CellHeight*zoomMIN/zoomMAX);
            vector2S[4] = new Vector2(x, y);
            DXManager.Line.Width = 0.5F;
            DXManager.Line.Draw(vector2S, Color.Magenta);
        }

        private void DrawHorizontalLine(int x, int y)
        {
            line[0] = new Vector2(x*CellWidth*zoomMIN/zoomMAX, y*CellHeight*zoomMIN/zoomMAX);
            line[1] = new Vector2(MapPanel.Width, y*CellHeight*zoomMIN/zoomMAX);

            DXManager.Line.Width = 0.5F;
            DXManager.Line.Draw(line, Color.Magenta);
        }

        private void DrawVerticalLine(int x, int y)
        {
            line[0] = new Vector2(x*CellWidth*zoomMIN/zoomMAX, y*CellHeight*zoomMIN/zoomMAX);
            line[1] = new Vector2(x*CellWidth*zoomMIN/zoomMAX, MapPanel.Height);

            DXManager.Line.Width = 0.5F;
            DXManager.Line.Draw(line, Color.Magenta);
        }

        private void DrawCube(Point p1, Point p2)
        {
            //缩放时需要优先计算缩放系数加上括号
            var vector2S = new Vector2[5];
            vector2S[0] = new Vector2((p1.X - mapPoint.X)*(CellWidth*zoomMIN/zoomMAX),
                (p1.Y - mapPoint.Y)*(CellHeight*zoomMIN/zoomMAX));
            vector2S[1] = new Vector2((p2.X - mapPoint.X + 1)*(CellWidth*zoomMIN/zoomMAX),
                (p1.Y - mapPoint.Y)*(CellHeight*zoomMIN/zoomMAX));
            vector2S[2] = new Vector2((p2.X - mapPoint.X + 1)*(CellWidth*zoomMIN/zoomMAX),
                (p2.Y - mapPoint.Y + 1)*(CellHeight*zoomMIN/zoomMAX));
            vector2S[3] = new Vector2((p1.X - mapPoint.X)*(CellWidth*zoomMIN/zoomMAX),
                (p2.Y - mapPoint.Y + 1)*(CellHeight*zoomMIN/zoomMAX));
            vector2S[4] = new Vector2((p1.X - mapPoint.X)*(CellWidth*zoomMIN/zoomMAX),
                (p1.Y - mapPoint.Y)*(CellHeight*zoomMIN/zoomMAX));
            DXManager.Line.Width = 2F;
            DXManager.Line.Draw(vector2S, Color.Red);
        }

        private void GraspingRectangle()
        {
            if (p1.IsEmpty || p2.IsEmpty) return;
            if ((layer == Layer.GraspingMir2Front) || (layer == Layer.GraspingInvertMir3FrontMiddle))
            {
                DrawCube(p1, p2);
            }
        }

        private void DrawFrontMask(bool blFrontMask)
        {
            if (blFrontMask)
            {
                for (var y = mapPoint.Y - 1; y <= mapPoint.Y + OffSetY + 35; y++)
                {
                    if (y >= mapHeight || y < 0) continue;
                    drawY = (y - mapPoint.Y)*(CellHeight*zoomMIN/zoomMAX);
                    for (var x = mapPoint.X; x <= mapPoint.X + OffSetX + 35; x++)
                    {
                        if (x >= mapWidth || x < 0) continue;
                        drawX = (x - mapPoint.X)*(CellWidth*zoomMIN/zoomMAX);
                        if (Convert.ToBoolean(M2CellInfo[x, y].FrontImage & 0x8000))
                        {
                            Draw(1, 59, drawX, drawY);
                        }
                    }
                }
            }
        }

        private void DrawBackLimit(bool blBackMask)
        {
            if (blBackMask)
            {
                for (var y = mapPoint.Y - 1; y <= mapPoint.Y + OffSetY + 35; y++)
                {
                    if (y >= mapHeight || y < 0) continue;
                    drawY = (y - mapPoint.Y)*(CellHeight*zoomMIN/zoomMAX);
                    for (var x = mapPoint.X; x <= mapPoint.X + OffSetX + 35; x++)
                    {
                        if (x >= mapWidth || x < 0) continue;
                        drawX = (x - mapPoint.X)*(CellWidth*zoomMIN/zoomMAX);
                        if (Convert.ToBoolean(M2CellInfo[x, y].BackImage & 0x20000000))
                        {
                            Draw(1, 58, drawX, drawY);
                        }
                    }
                }
            }
        }

        private void DrawFront(bool blFront)
        {
            if (blFront)
            {
                byte animation;
                bool blend;

                for (var y = mapPoint.Y - 1; y <= mapPoint.Y + OffSetY + 35; y++)
                {
                    if (y >= mapHeight || y < 0) continue;
                    //drawY = (y + 1) * (cellHeight * zoomMIN / zoomMAX);
                    for (var x = mapPoint.X; x <= mapPoint.X + OffSetX + 35; x++)
                    {
                        if (x >= mapWidth || x < 0) continue;
                        drawX = (x - mapPoint.X)*(CellWidth*zoomMIN/zoomMAX);
                        index = (M2CellInfo[x, y].FrontImage & 0x7FFF) - 1;
                        libIndex = M2CellInfo[x, y].FrontIndex;
                        if (libIndex < 0 || libIndex >= Libraries.MapLibs.Length) continue;
                        if (index < 0 || index >= Libraries.MapLibs[libIndex].Images.Count) continue;

                        animation = M2CellInfo[x, y].FrontAnimationFrame;
                        if ((animation & 0x80) > 0)
                        {
                            blend = true;
                            animation &= 0x7F;
                        }
                        else
                        {
                            blend = false;
                        }

                        if (animation > 0)
                        {
                            var animationTick = M2CellInfo[x, y].FrontAnimationTick;
                            index += AnimationCount%(animation + animation*animationTick)/(1 + animationTick);
                        }

                        var doorOffset = M2CellInfo[x, y].DoorOffset;
                        var s = Libraries.MapLibs[libIndex].GetSize(index);
                        //不是 48*32 或96*64 的地砖 是大物体
                        if ((s.Width != CellWidth || s.Height != CellHeight) &&
                            (s.Width != CellWidth*2 || s.Height != CellHeight*2))
                        {
                            drawY = (y - mapPoint.Y + 1)*(CellHeight*zoomMIN/zoomMAX);
                            //如果有动画
                            if (animation > 0)
                            {
                                //如果需要混合
                                if (blend)
                                {
                                    //新盛大地图
                                    if ((libIndex > 99) & (libIndex < 199))
                                    {
                                        DrawBlend(libIndex, index,
                                            new Point(drawX, drawY - 3*CellHeight*zoomMIN/zoomMAX), Color.White, true);
                                    }
                                    //老地图灯柱 index >= 2723 && index <= 2732
                                    else
                                    {
                                        DrawBlend(libIndex, index, new Point(drawX, drawY - s.Height*zoomMIN/zoomMAX),
                                            Color.White, index >= 2723 && index <= 2732);
                                    }
                                }
                                //不需要混合
                                else
                                {
                                    Draw(libIndex, index, drawX, drawY - s.Height*zoomMIN/zoomMAX);
                                }
                            }
                            //如果没动画 
                            else
                            {
                                Draw(libIndex, index, drawX, drawY - s.Height*zoomMIN/zoomMAX);
                            }
                        }
                        //是 48*32 或96*64 的地砖
                        else
                        {
                            drawY = (y - mapPoint.Y)*(CellHeight*zoomMIN/zoomMAX);
                            Draw(libIndex, index, drawX, drawY);
                        }
                        //显示门打开
                        if (chkDoor.Checked && (doorOffset > 0))
                        {
                            drawY = (y - mapPoint.Y + 1)*(CellHeight*zoomMIN/zoomMAX);
                            Draw(libIndex, index + doorOffset, drawX, drawY - s.Height*zoomMIN/zoomMAX);
                        }
                    }
                }
            }
        }

        private void DrawFrontTag(bool blFront)
        {
            if (blFront)
            {
                for (var y = mapPoint.Y - 1; y <= mapPoint.Y + OffSetY + 35; y++)
                {
                    if (y >= mapHeight || y < 0) continue;
                    drawY = (y - mapPoint.Y)*(CellHeight*zoomMIN/zoomMAX);
                    for (var x = mapPoint.X; x <= mapPoint.X + OffSetX + 35; x++)
                    {
                        if (x >= mapWidth || x < 0) continue;
                        drawX = (x - mapPoint.X)*(CellWidth*zoomMIN/zoomMAX);
                        if ((M2CellInfo[x, y].FrontImage & 0x7FFF) - 1 >= 0)
                        {
                            Draw(1, 56, drawX, drawY);
                        }
                    }
                }
            }
        }

        private void DrawMiddleTag(bool blMiddle)
        {
            if (blMiddle)
            {
                for (var y = mapPoint.Y - 1; y <= mapPoint.Y + OffSetY + 35; y++)
                {
                    if (y >= mapHeight || y < 0) continue;
                    drawY = (y - mapPoint.Y)*(CellHeight*zoomMIN/zoomMAX);
                    for (var x = mapPoint.X; x <= mapPoint.X + OffSetX + 35; x++)
                    {
                        if (x >= mapWidth || x < 0) continue;
                        drawX = (x - mapPoint.X)*(CellWidth*zoomMIN/zoomMAX);
                        if ((M2CellInfo[x, y].MiddleImage & 0x7FFF) - 1 >= 0)
                        {
                            Draw(1, 56, drawX, drawY);
                        }
                    }
                }
            }
        }

        private void DrawMidd(bool blMidd)
        {
            if (blMidd)
            {
                byte animation;
                bool blend;
                for (var y = mapPoint.Y - 1; y <= mapPoint.Y + OffSetY + 35; y++)
                {
                    if (y >= mapHeight || y < 0) continue;
                    for (var x = mapPoint.X - 1; x <= mapPoint.X + OffSetX + 35; x++)
                    {
                        if (x >= mapWidth || x < 0) continue;
                        drawX = (x - mapPoint.X)*(CellWidth*zoomMIN/zoomMAX);
                        index = M2CellInfo[x, y].MiddleImage - 1;
                        libIndex = M2CellInfo[x, y].MiddleIndex;
                        if (libIndex < 0 || libIndex >= Libraries.MapLibs.Length) continue;
                        if (index < 0 || index >= Libraries.MapLibs[libIndex].Images.Count) continue;

                        animation = M2CellInfo[x, y].MiddleAnimationFrame;
                        blend = false;
                        if ((animation > 0) && (animation < 255))
                        {
                            if ((animation & 0x0f) > 0)
                            {
                                blend = true;
                                animation &= 0x0f;
                            }
                            if (animation > 0)
                            {
                                var animationTick = M2CellInfo[x, y].MiddleAnimationTick;
                                index += AnimationCount%(animation + animation*animationTick)/(1 + animationTick);
                            }
                        }

                        var s = Libraries.MapLibs[libIndex].GetSize(index);
                        if ((s.Width != CellWidth || s.Height != CellHeight) &&
                            (s.Width != CellWidth*2 || s.Height != CellHeight*2))
                        {
                            drawY = (y - mapPoint.Y + 1)*(CellHeight*zoomMIN/zoomMAX);
                            Draw(libIndex, index, drawX, drawY - s.Height*zoomMIN/zoomMAX);
                        }
                        else
                        {
                            drawY = (y - mapPoint.Y)*(CellHeight*zoomMIN/zoomMAX);
                            Draw(libIndex, index, drawX, drawY);
                        }
                    }
                }
            }
        }

        private void DrawBack(bool blBack)
        {
            if (blBack)
            {
                for (var y = mapPoint.Y - 1; y <= mapPoint.Y + OffSetY; y++)
                {
                    if (y%2 != 0) continue;
                    if (y >= mapHeight) continue;
                    drawY = (y - mapPoint.Y)*(CellHeight*zoomMIN/zoomMAX);
                    for (var x = mapPoint.X - 1; x <= mapPoint.X + OffSetX; x++)
                    {
                        if (x%2 != 0) continue;
                        if (x >= mapWidth) continue;
                        drawX = (x - mapPoint.X)*(CellWidth*zoomMIN/zoomMAX);
                        index = (M2CellInfo[x, y].BackImage & 0x1FFFFFFF) - 1;
                        libIndex = M2CellInfo[x, y].BackIndex;
                        if (libIndex < 0 || libIndex >= Libraries.MapLibs.Length) continue;
                        if (index < 0 || index >= Libraries.MapLibs[libIndex].Images.Count) continue;
                        Draw(libIndex, index, drawX, drawY);
                    }
                }
            }
        }

        private void ReadWemadeMir2LibToListBox()
        {
            for (var i = 0; i < 100; i++)
            {
                if (Libraries.ListItems[i] != null)
                {
                    WemadeMir2LibListBox.Items.Add(Libraries.ListItems[i]);
                }
            }
        }

        private void ReadShandaMir2LibToListBox()
        {
            for (var i = 100; i < 200; i++)
            {
                if (Libraries.ListItems[i] != null)
                {
                    ShandaMir2LibListBox.Items.Add(Libraries.ListItems[i]);
                }
            }
        }

        private void ReadWemadeMir3LibToListBox()
        {
            for (var i = 200; i < 300; i++)
            {
                if (Libraries.ListItems[i] != null)
                {
                    WemadeMir3LibListBox.Items.Add(Libraries.ListItems[i]);
                }
            }
        }

        private void ReadShandaMir3LibToListBox()
        {
            for (var i = 300; i < 400; i++)
            {
                if (Libraries.ListItems[i] != null)
                {
                    ShandaMir3LibListBox.Items.Add(Libraries.ListItems[i]);
                }
            }
        }

        private void Clear()
        {
            _shandaMir2IndexList.Clear();
            ShandaMir2ImageList.Images.Clear();
            _wemadeMir2IndexList.Clear();
            WemadeMir2ImageList.Images.Clear();
            _wemadeMir3IndexList.Clear();
            WemadeMir3ImageList.Images.Clear();
            _shandaMir3IndexList.Clear();
            ShandaMir3ImageList.Images.Clear();
            TilesImageList.Images.Clear();
            _tilesIndexList.Clear();
        }

        private void ClearImage()
        {
            for (var i = 0; i < Libraries.MapLibs.Length; i++)
            {
                for (var j = 0; j < Libraries.MapLibs[i].Images.Count; j++)
                {
                    Libraries.MapLibs[i].Images[j] = null;
                }
            }
        }

        public void EnlargeZoom()
        {
            graphics.Clear(Color.Black);
            zoomMIN += 3;
            if (zoomMIN >= zoomMAX)
            {
                zoomMIN = zoomMAX;
            }
        }

        public void NarrowZoom()
        {
            graphics.Clear(Color.Black);
            zoomMIN -= 3;
            if (zoomMIN <= 0)
            {
                zoomMIN = 1;
            }
        }

        private void btnNew_Click(object sender, EventArgs e)
        {
            var frm = new NewFileFrm(SetMapSize);
            if (frm.ShowDialog() == DialogResult.OK)
            {
                M2CellInfo = new CellInfo[mapWidth, mapHeight];
                for (var x = 0; x < mapWidth; x++)
                {
                    for (var y = 0; y < mapHeight; y++)
                    {
                        M2CellInfo[x, y] = new CellInfo();
                    }
                }
            }
            mapPoint = new Point(0, 0);
        }

        private void btnOpen_Click(object sender, EventArgs e)
        {
            ClearImage();
            var openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                var filePath = openFileDialog.FileName;
                map = new MapReader(filePath);
                M2CellInfo = map.MapCells;
                mapPoint = new Point(0, 0);
                SetMapSize(map.Width, map.Height);
                mapFileName = openFileDialog.FileName;
            }
        }

        private void btnDispose_Click(object sender, EventArgs e)
        {
            Dispose();
        }

        private void btnNarrow_Click(object sender, EventArgs e)
        {
            if (map != null)
            {
                NarrowZoom();
                SetMapSize(mapWidth, mapHeight);
            }
        }

        private void btnEnlarge_Click(object sender, EventArgs e)
        {
            if (map != null)
            {
                EnlargeZoom();
                SetMapSize(mapWidth, mapHeight);
            }
        }

        private void chkBack_Click(object sender, EventArgs e)
        {
            chkBack.Checked = !chkBack.Checked;
        }

        private void chkMidd_Click(object sender, EventArgs e)
        {
            chkMidd.Checked = !chkMidd.Checked;
        }

        private void chkFront_Click(object sender, EventArgs e)
        {
            chkFront.Checked = !chkFront.Checked;
        }

        private void chkBackMask_Click(object sender, EventArgs e)
        {
            chkBackMask.Checked = !chkBackMask.Checked;
        }

        private void chkFrontMask_Click(object sender, EventArgs e)
        {
            chkFrontMask.Checked = !chkFrontMask.Checked;
        }

        private void btnToImage_Click(object sender, EventArgs e)
        {
            //MessageBox.Show("Too lazy to write the .........");
            //var bit = new Bitmap(Width, Height); //实例化一个和窗体一样大的bitmap
            //var g = Graphics.FromImage(bit);
            //g.CompositingQuality = CompositingQuality.HighQuality; //质量设为最高
            //g.CopyFromScreen(Left, Top, 0, 0, new Size(Width, Height)); //保存整个窗体为图片
            ////g.CopyFromScreen(panel游戏区 .PointToScreen(Point.Empty), Point.Empty, panel游戏区.Size);//只保存某个控件（这里是panel游戏区）
            //bit.Save("weiboTemp.png"); //默认保存格式为PNG，保存成jpg格式质量不是很好
        }

        private void chkDoor_Click(object sender, EventArgs e)
        {
            chkDoor.Checked = !chkDoor.Checked;
        }

        private void DrawDoorTag(bool blDoorTag)
        {
            if (!blDoorTag)
            {
                return;
            }
            var szText = "";
            int DoorIndex;
            byte DoorOffset;
            bool EntityDoor;
            var font = new Font("Comic Sans MS", 10, FontStyle.Bold);
            var dxFont = new Microsoft.DirectX.Direct3D.Font(DXManager.Device, font);
            for (var y = mapPoint.Y; y <= mapPoint.Y + OffSetY; y++)
            {
                if (y >= mapHeight || y < 0) continue;
                drawY = (y - mapPoint.Y)*(CellHeight*zoomMIN/zoomMAX);
                for (var x = mapPoint.X; x <= mapPoint.X + OffSetX + 35; x++)
                {
                    if (x >= mapWidth || x < 0) continue;
                    drawX = (x - mapPoint.X)*(CellWidth*zoomMIN/zoomMAX);
                    DoorIndex = M2CellInfo[x, y].DoorIndex & 0x7F;
                    DoorOffset = M2CellInfo[x, y].DoorOffset;
                    EntityDoor = Convert.ToBoolean(M2CellInfo[x, y].DoorIndex & 0x80);

                    if (DoorIndex > 0)
                    {
                        if (EntityDoor)
                        {
                            szText = string.Format("Dx{0}/{1}", DoorIndex, DoorOffset);
                        }
                        else
                        {
                            szText = string.Format("D{0}/{1}", DoorIndex, DoorOffset);
                        }
                        dxFont.DrawText(DXManager.TextSprite, szText, drawX, drawY, Color.AliceBlue);
                    }
                }
            }
            font.Dispose();
            dxFont.Dispose();
        }

        private void chkDoorSign_Click(object sender, EventArgs e)
        {
            chkDoorSign.Checked = !chkDoorSign.Checked;
        }

        private void DrawFrontAnimationTag(bool blFrontAnimationTag)
        {
            if (!blFrontAnimationTag)
            {
                return;
            }
            var szText = "";
            int FrontAnimationFrame;
            byte FrontAnimationTick;
            bool CoreAnimation;
            var font = new Font("Comic Sans MS", 10, FontStyle.Bold);
            var dxFont = new Microsoft.DirectX.Direct3D.Font(DXManager.Device, font);
            for (var y = mapPoint.Y; y <= mapPoint.Y + OffSetY; y++)
            {
                if (y >= mapHeight || y < 0) continue;
                drawY = (y - mapPoint.Y)*(CellHeight*zoomMIN/zoomMAX);
                for (var x = mapPoint.X; x <= mapPoint.X + OffSetX + 35; x++)
                {
                    if (x >= mapWidth || x < 0) continue;
                    drawX = (x - mapPoint.X)*(CellWidth*zoomMIN/zoomMAX);
                    if (M2CellInfo[x, y].FrontAnimationFrame == 255)
                    {
                        continue;
                    }
                    FrontAnimationFrame = M2CellInfo[x, y].FrontAnimationFrame & 0x7F;
                    FrontAnimationTick = M2CellInfo[x, y].FrontAnimationTick;
                    CoreAnimation = Convert.ToBoolean(M2CellInfo[x, y].FrontAnimationFrame & 0x80);

                    if (FrontAnimationFrame > 0)
                    {
                        if (CoreAnimation)
                        {
                            szText = string.Format("FAb{0}/{1}", FrontAnimationFrame, FrontAnimationTick);
                        }
                        else
                        {
                            szText = string.Format("FA{0}/{1}", FrontAnimationFrame, FrontAnimationTick);
                        }
                        dxFont.DrawText(DXManager.TextSprite, szText, drawX, drawY, Color.AliceBlue);
                    }
                }
            }
            font.Dispose();
            dxFont.Dispose();
        }

        private void DrawMiddleAnimationTag(bool blMiddleAnimationTag)
        {
            if (!blMiddleAnimationTag)
            {
                return;
            }
            var szText = "";
            int MiddleAnimationFrame;
            byte MiddleAnimationTick;
            bool BlendAnimation;
            var font = new Font("Comic Sans MS", 10, FontStyle.Bold);
            var dxFont = new Microsoft.DirectX.Direct3D.Font(DXManager.Device, font);
            for (var y = mapPoint.Y; y <= mapPoint.Y + OffSetY; y++)
            {
                if (y >= mapHeight || y < 0) continue;
                drawY = (y - mapPoint.Y)*(CellHeight*zoomMIN/zoomMAX);
                for (var x = mapPoint.X; x <= mapPoint.X + OffSetX + 35; x++)
                {
                    if (x >= mapWidth || x < 0) continue;
                    drawX = (x - mapPoint.X)*(CellWidth*zoomMIN/zoomMAX);
                    if (M2CellInfo[x, y].MiddleAnimationFrame == 255)
                    {
                        continue;
                    }
                    MiddleAnimationFrame = M2CellInfo[x, y].MiddleAnimationFrame & 0x0F;
                    MiddleAnimationTick = M2CellInfo[x, y].MiddleAnimationTick;
                    BlendAnimation = Convert.ToBoolean(M2CellInfo[x, y].MiddleAnimationFrame & 0x08);

                    if (MiddleAnimationFrame > 0)
                    {
                        if (BlendAnimation)
                        {
                            szText = string.Format("MAb{0}/{1}", MiddleAnimationFrame, MiddleAnimationTick);
                        }
                        else
                        {
                            szText = string.Format("MA{0}/{1}", MiddleAnimationFrame, MiddleAnimationTick);
                        }
                        dxFont.DrawText(DXManager.TextSprite, szText, drawX, drawY, Color.Black);
                    }
                }
            }
            font.Dispose();
            dxFont.Dispose();
        }

        private void chkFrontAnimationTag_Click(object sender, EventArgs e)
        {
            chkFrontAnimationTag.Checked = !chkFrontAnimationTag.Checked;
        }

        private void DrawLightTag(bool blLightTag)
        {
            if (!blLightTag)
            {
                return;
            }
            var szText = "";
            int Light;
            var font = new Font("Comic Sans MS", 10, FontStyle.Bold);
            var dxFont = new Microsoft.DirectX.Direct3D.Font(DXManager.Device, font);
            for (var y = mapPoint.Y - 1; y <= mapPoint.Y + OffSetY + 35; y++)
            {
                if (y >= mapHeight || y < 0) continue;
                drawY = (y - mapPoint.Y)*(CellHeight*zoomMIN/zoomMAX);
                for (var x = mapPoint.X; x <= mapPoint.X + OffSetX + 35; x++)
                {
                    if (x >= mapWidth || x < 0) continue;
                    drawX = (x - mapPoint.X)*(CellWidth*zoomMIN/zoomMAX);
                    Light = M2CellInfo[x, y].Light;

                    if (Light > 0)
                    {
                        Draw(1, 57, drawX, drawY);
                        szText = string.Format("L{0}", Light);
                        dxFont.DrawText(DXManager.TextSprite, szText, drawX + 32*zoomMIN/zoomMAX, drawY, Color.AliceBlue);
                    }
                }
            }
            font.Dispose();
            dxFont.Dispose();
        }

        private void chkLightTag_Click(object sender, EventArgs e)
        {
            chkLightTag.Checked = !chkLightTag.Checked;
        }

        private void Save()
        {
            if (M2CellInfo == null)
            {
            }
            else
            {
                var saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "Custom  Map (*.Map)|*.Map";
                saveFileDialog.FileName = "Custom  Map";
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    var fileStream = new FileStream(saveFileDialog.FileName, FileMode.Create);
                    var binaryWriter = new BinaryWriter(fileStream);
                    short ver = 1;
                    char[] tag = {'C', '#'};
                    binaryWriter.Write(ver);
                    binaryWriter.Write(tag);

                    binaryWriter.Write(Convert.ToInt16(mapWidth));
                    binaryWriter.Write(Convert.ToInt16(mapHeight));
                    for (var x = 0; x < mapWidth; x++)
                    {
                        for (var y = 0; y < mapHeight; y++)
                        {
                            binaryWriter.Write(M2CellInfo[x, y].BackIndex);
                            binaryWriter.Write(M2CellInfo[x, y].BackImage);
                            binaryWriter.Write(M2CellInfo[x, y].MiddleIndex);
                            binaryWriter.Write(M2CellInfo[x, y].MiddleImage);
                            binaryWriter.Write(M2CellInfo[x, y].FrontIndex);
                            binaryWriter.Write(M2CellInfo[x, y].FrontImage);
                            binaryWriter.Write(M2CellInfo[x, y].DoorIndex);
                            binaryWriter.Write(M2CellInfo[x, y].DoorOffset);
                            binaryWriter.Write(M2CellInfo[x, y].FrontAnimationFrame);
                            binaryWriter.Write(M2CellInfo[x, y].FrontAnimationTick);
                            binaryWriter.Write(M2CellInfo[x, y].MiddleAnimationFrame);
                            binaryWriter.Write(M2CellInfo[x, y].MiddleAnimationTick);
                            binaryWriter.Write(M2CellInfo[x, y].TileAnimationImage);
                            binaryWriter.Write(M2CellInfo[x, y].TileAnimationOffset);
                            binaryWriter.Write(M2CellInfo[x, y].TileAnimationFrames);
                            binaryWriter.Write(M2CellInfo[x, y].Light);
                        }
                    }
                    binaryWriter.Flush();
                    binaryWriter.Dispose();
                    MessageBox.Show("Map Saved");
                }
            }
        }

        private void InvertMir3Layer()
        {
            if (M2CellInfo != null)
            {
                var saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "Custom  Map (*.Mir3)|*.Mir3";
                saveFileDialog.FileName = "Invert Mir3 Layer";
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    var fileStream = new FileStream(saveFileDialog.FileName, FileMode.Create);
                    var binaryWriter = new BinaryWriter(fileStream);
                    short ver = 1;
                    char[] tag = {'C', '#'};
                    string str;
                    binaryWriter.Write(ver);
                    binaryWriter.Write(tag);

                    binaryWriter.Write(Convert.ToInt16(mapWidth));
                    binaryWriter.Write(Convert.ToInt16(mapHeight));
                    for (var x = 0; x < mapWidth; x++)
                    {
                        for (var y = 0; y < mapHeight; y++)
                        {
                            binaryWriter.Write(M2CellInfo[x, y].BackIndex);
                            binaryWriter.Write(M2CellInfo[x, y].BackImage);

                            if ((M2CellInfo[x, y].MiddleImage != 0) && ((M2CellInfo[x, y].FrontImage & 0x7FFF) == 0))
                            {
                                str = GetLibName(M2CellInfo[x, y].MiddleIndex);
                                if (!(str.IndexOf("SmTiles", StringComparison.Ordinal) > -1))
                                {
                                    if ((M2CellInfo[x, y].MiddleAnimationFrame != 0) &&
                                        (M2CellInfo[x, y].MiddleAnimationFrame != 255) &&
                                        (M2CellInfo[x, y].FrontAnimationFrame == 0))
                                    {
                                        M2CellInfo[x, y].FrontAnimationFrame =
                                            (byte) (M2CellInfo[x, y].MiddleAnimationFrame & 0x0F);
                                        M2CellInfo[x, y].FrontAnimationTick = M2CellInfo[x, y].MiddleAnimationTick;
                                        M2CellInfo[x, y].MiddleAnimationFrame = 0;
                                        M2CellInfo[x, y].MiddleAnimationTick = 0;
                                    }
                                    M2CellInfo[x, y].FrontImage = M2CellInfo[x, y].MiddleImage;
                                    M2CellInfo[x, y].FrontIndex = M2CellInfo[x, y].MiddleIndex;
                                    M2CellInfo[x, y].MiddleImage = 0;
                                    M2CellInfo[x, y].MiddleIndex = 0;
                                }
                            }
                            else if ((M2CellInfo[x, y].MiddleImage != 0) && ((M2CellInfo[x, y].FrontImage & 0x7FFF) != 0))
                            {
                                str = GetLibName(M2CellInfo[x, y].MiddleIndex);
                                if (!(str.IndexOf("SmTiles", StringComparison.Ordinal) > -1))
                                {
                                    if ((M2CellInfo[x, y].MiddleAnimationFrame == 255) ||
                                        (M2CellInfo[x, y].MiddleAnimationFrame == 0))
                                    {
                                        if (M2CellInfo[x, y].FrontAnimationFrame == 0)
                                        {
                                            var temp = M2CellInfo[x, y].MiddleImage;
                                            M2CellInfo[x, y].MiddleImage =
                                                (short) (M2CellInfo[x, y].FrontImage & 0x7FFF);
                                            M2CellInfo[x, y].FrontImage = temp;
                                            temp = M2CellInfo[x, y].MiddleIndex;
                                            M2CellInfo[x, y].MiddleIndex = M2CellInfo[x, y].FrontIndex;
                                            M2CellInfo[x, y].FrontIndex = temp;
                                        }
                                    }
                                }
                            }


                            binaryWriter.Write(M2CellInfo[x, y].MiddleIndex);
                            binaryWriter.Write(M2CellInfo[x, y].MiddleImage);
                            binaryWriter.Write(M2CellInfo[x, y].FrontIndex);
                            binaryWriter.Write(M2CellInfo[x, y].FrontImage);
                            binaryWriter.Write(M2CellInfo[x, y].DoorIndex);
                            binaryWriter.Write(M2CellInfo[x, y].DoorOffset);
                            binaryWriter.Write(M2CellInfo[x, y].FrontAnimationFrame);
                            binaryWriter.Write(M2CellInfo[x, y].FrontAnimationTick);
                            binaryWriter.Write(M2CellInfo[x, y].MiddleAnimationFrame);
                            binaryWriter.Write(M2CellInfo[x, y].MiddleAnimationTick);
                            binaryWriter.Write(M2CellInfo[x, y].TileAnimationImage);
                            binaryWriter.Write(M2CellInfo[x, y].TileAnimationOffset);
                            binaryWriter.Write(M2CellInfo[x, y].TileAnimationFrames);
                            binaryWriter.Write(M2CellInfo[x, y].Light);
                        }
                    }
                    binaryWriter.Flush();
                    binaryWriter.Dispose();
                    fileStream.Dispose();
                    MessageBox.Show("保存成功");
                }
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            Save();
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            if (M2CellInfo != null)
            {
                for (var x = 0; x < mapWidth; x++)
                {
                    for (var y = 0; y < mapHeight; y++)
                    {
                        M2CellInfo[x, y] = new CellInfo();
                    }
                }
            }
        }

        private void ShandaMir2LibListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            Clear();
            shangdaMir2ListItem = (ListItem) ShandaMir2LibListBox.SelectedItem;
            selectListItem = shangdaMir2ListItem;
            selectListItem.Version = (byte) MirVerSion.ShandaMir2;
            ShandaMir2LibListView.VirtualListSize = Libraries.MapLibs[shangdaMir2ListItem.Value].Images.Count;
            TileslistView.VirtualListSize = Libraries.MapLibs[selectListItem.Value].Images.Count/Mir2BigTileBlock;
        }

        private void ShandaMir2LiblistView_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e)
        {
            int index;

            if (_shandaMir2IndexList.TryGetValue(e.ItemIndex, out index))
            {
                e.Item = new ListViewItem {ImageIndex = index, Text = e.ItemIndex.ToString()};
                return;
            }

            _shandaMir2IndexList.Add(e.ItemIndex, ShandaMir2ImageList.Images.Count);
            Libraries.MapLibs[shangdaMir2ListItem.Value].CheckImage(e.ItemIndex);
            ShandaMir2ImageList.Images.Add(Libraries.MapLibs[shangdaMir2ListItem.Value].GetPreview(e.ItemIndex));
            e.Item = new ListViewItem {ImageIndex = index, Text = e.ItemIndex.ToString()};
            Libraries.MapLibs[shangdaMir2ListItem.Value].Images[e.ItemIndex] = null;
        }

        private void WemadeMir2LibListView_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e)
        {
            int index;

            if (_wemadeMir2IndexList.TryGetValue(e.ItemIndex, out index))
            {
                e.Item = new ListViewItem {ImageIndex = index, Text = e.ItemIndex.ToString()};
                return;
            }

            _wemadeMir2IndexList.Add(e.ItemIndex, WemadeMir2ImageList.Images.Count);
            Libraries.MapLibs[wemadeMir2ListItem.Value].CheckImage(e.ItemIndex);
            WemadeMir2ImageList.Images.Add(Libraries.MapLibs[wemadeMir2ListItem.Value].GetPreview(e.ItemIndex));
            e.Item = new ListViewItem {ImageIndex = index, Text = e.ItemIndex.ToString()};
            Libraries.MapLibs[wemadeMir2ListItem.Value].Images[e.ItemIndex] = null;
        }

        private void WemadeMir2LibListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            Clear();
            wemadeMir2ListItem = (ListItem) WemadeMir2LibListBox.SelectedItem;
            selectListItem = wemadeMir2ListItem;
            selectListItem.Version = (byte) MirVerSion.WemadeMir2;
            WemadeMir2LibListView.VirtualListSize = Libraries.MapLibs[wemadeMir2ListItem.Value].Images.Count;
            TileslistView.VirtualListSize = Libraries.MapLibs[wemadeMir2ListItem.Value].Images.Count/Mir2BigTileBlock;
        }

        private void WemadeMir3LibListView_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e)
        {
            int index;

            if (_wemadeMir3IndexList.TryGetValue(e.ItemIndex, out index))
            {
                e.Item = new ListViewItem {ImageIndex = index, Text = e.ItemIndex.ToString()};
                return;
            }

            _wemadeMir3IndexList.Add(e.ItemIndex, WemadeMir3ImageList.Images.Count);
            Libraries.MapLibs[wemadeMir3ListItem.Value].CheckImage(e.ItemIndex);
            WemadeMir3ImageList.Images.Add(Libraries.MapLibs[wemadeMir3ListItem.Value].GetPreview(e.ItemIndex));
            e.Item = new ListViewItem {ImageIndex = index, Text = e.ItemIndex.ToString()};
            Libraries.MapLibs[wemadeMir3ListItem.Value].Images[e.ItemIndex] = null;
        }

        private void WemadeMir3LibListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            Clear();
            wemadeMir3ListItem = (ListItem) WemadeMir3LibListBox.SelectedItem;
            selectListItem = wemadeMir3ListItem;
            selectListItem.Version = (byte) MirVerSion.WemadeMir3;
            WemadeMir3LibListView.VirtualListSize = Libraries.MapLibs[wemadeMir3ListItem.Value].Images.Count;
            if (selectListItem.Text.IndexOf("Tiles30", StringComparison.Ordinal) > -1)
            {
                if (Libraries.MapLibs[selectListItem.Value].Images.Count%10 != 0)
                {
                    TileslistView.VirtualListSize = (Libraries.MapLibs[wemadeMir3ListItem.Value].Images.Count + 1)/
                                                    Mir3BigTileBlock*2;
                }
                else
                {
                    TileslistView.VirtualListSize = Libraries.MapLibs[wemadeMir3ListItem.Value].Images.Count/
                                                    Mir3BigTileBlock*2;
                }
            }
            else if (selectListItem.Text.IndexOf("smtiles", StringComparison.Ordinal) > -1)
            {
                TileslistView.VirtualListSize = Libraries.MapLibs[wemadeMir3ListItem.Value].Images.Count/
                                                Mir3BigTileBlock;
            }
        }

        private void ShandaMir3LibListView_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e)
        {
            int index;

            if (_shandaMir3IndexList.TryGetValue(e.ItemIndex, out index))
            {
                e.Item = new ListViewItem {ImageIndex = index, Text = e.ItemIndex.ToString()};
                return;
            }

            _shandaMir3IndexList.Add(e.ItemIndex, ShandaMir3ImageList.Images.Count);
            Libraries.MapLibs[shangdaMir3ListItem.Value].CheckImage(e.ItemIndex);
            ShandaMir3ImageList.Images.Add(Libraries.MapLibs[shangdaMir3ListItem.Value].GetPreview(e.ItemIndex));
            e.Item = new ListViewItem {ImageIndex = index, Text = e.ItemIndex.ToString()};
            Libraries.MapLibs[shangdaMir3ListItem.Value].Images[e.ItemIndex] = null;
        }

        private void ShandaMir3LibListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            Clear();
            shangdaMir3ListItem = (ListItem) ShandaMir3LibListBox.SelectedItem;
            selectListItem = shangdaMir3ListItem;
            selectListItem.Version = (byte) MirVerSion.ShandaMir3;
            ShandaMir3LibListView.VirtualListSize = Libraries.MapLibs[shangdaMir3ListItem.Value].Images.Count;

            if (selectListItem.Text.IndexOf("Tiles30", StringComparison.Ordinal) > -1)
            {
                if (Libraries.MapLibs[selectListItem.Value].Images.Count%10 != 0)
                {
                    TileslistView.VirtualListSize = (Libraries.MapLibs[shangdaMir3ListItem.Value].Images.Count + 1)/
                                                    Mir3BigTileBlock*2;
                }
                else
                {
                    TileslistView.VirtualListSize = Libraries.MapLibs[shangdaMir3ListItem.Value].Images.Count/
                                                    Mir3BigTileBlock*2;
                }
            }
            else if (selectListItem.Text.IndexOf("smtiles", StringComparison.Ordinal) > -1)
            {
                TileslistView.VirtualListSize = Libraries.MapLibs[shangdaMir3ListItem.Value].Images.Count/
                                                Mir3BigTileBlock;
            }
        }

        private void chkMiddleAnimationTag_Click(object sender, EventArgs e)
        {
            chkMiddleAnimationTag.Checked = !chkMiddleAnimationTag.Checked;
        }

        private void cmbEditorLayer_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (cmbEditorLayer.SelectedIndex)
            {
                case 0:
                    layer = Layer.None;
                    break;
                case 1:
                    layer = Layer.BackImage;
                    break;
                case 2:
                    layer = Layer.MiddleImage;
                    break;
                case 3:
                    layer = Layer.FrontImage;
                    break;
                case 4:
                    layer = Layer.BackLimit;
                    break;
                case 5:
                    layer = Layer.FrontLimit;
                    break;
                case 6:
                    layer = Layer.BackFrontLimit;
                    break;
                case 7:
                    layer = Layer.GraspingMir2Front;
                    break;
                case 8:
                    layer = Layer.GraspingInvertMir3FrontMiddle;
                    break;
                case 9:
                    layer = Layer.PlaceObjects;
                    break;
                case 10:
                    layer = Layer.ClearAll;
                    break;
                case 11:
                    layer = Layer.ClearBack;
                    break;
                case 12:
                    layer = Layer.ClearMidd;
                    break;
                case 13:
                    layer = Layer.ClearFront;
                    break;
                case 14:
                    layer = Layer.ClearBackFrontLimit;
                    break;
                case 15:
                    layer = Layer.ClearBackLimit;
                    break;
                case 16:
                    layer = Layer.ClearFrontLimit;
                    break;
                case 17:
                    layer = Layer.BrushMir2BigTiles;
                    break;
                case 18:
                    layer = Layer.BrushSmTiles;
                    break;
                case 19:
                    layer = Layer.BrushMir3BigTiles;
                    break;
            }
        }

        private void WemadeMir2LibListView_SelectedIndexChanged(object sender, EventArgs e)
        {
            selectListItem = wemadeMir2ListItem;
            //在此处设断点，发现点击不同的Item后，此事件居然执行了2次 //第一次是取消当前Item选中状态，导致整个ListView的SelectedIndices变为0
            //第二次才将新选中的Item设置为选中状态，SelectedIndices变为1
            //如果不加listview.SelectedIndices.Count>0判断，将导致获取listview.Items[]索引超界的异常
            if (WemadeMir2LibListView.SelectedIndices.Count > 0)
            {
                selectLibMImage =
                    Libraries.MapLibs[wemadeMir2ListItem.Value].GetMImage(WemadeMir2LibListView.SelectedIndices[0]);
                if (selectLibMImage != null)
                {
                    selectImageIndex = WemadeMir2LibListView.SelectedIndices[0];

                    picWemdeMir2.Image = selectLibMImage.Image;
                    LabWemadeMir2Width.Text = "Width : " + selectLibMImage.Width;
                    LabWemadeMir2Height.Text = "Height : " + selectLibMImage.Height;
                    labeWemadeMir2OffSetX.Text = "OffSetX : " + selectLibMImage.X;
                    labWemadeMir2OffSetY.Text = "OffSetY : " + selectLibMImage.Y;
                    Libraries.MapLibs[wemadeMir2ListItem.Value].Images[WemadeMir2LibListView.SelectedIndices[0]] = null;
                }
            }
        }

        private bool CheckImageSizeIsBigTile(int libIndex, int index)
        {
            var s = Libraries.MapLibs[libIndex].GetSize(index);
            if ((s.Width != 2*CellWidth) && (s.Height != 2*CellHeight))
            {
                return false;
            }
            return true;
        }

        private Point CheckPointIsEven(Point point)
        {
            if ((point.X%2 == 0) && (point.Y%2 == 0))
            {
                return point;
            }
            if (point.X%2 != 0)
            {
                point.X = point.X - 1;
            }
            if (point.Y%2 != 0)
            {
                point.Y = point.Y - 1;
            }
            return point;
        }

        private void MapPanel_MouseClick(object sender, MouseEventArgs e)
        {
            Point[] points;
            Point temp;
            if (M2CellInfo == null) return;
            switch (layer)
            {
                case Layer.BackImage:
                    if (selectListItem == null) return;
                    if (CheckImageSizeIsBigTile(selectListItem.Value, selectImageIndex))
                    {
                        temp = CheckPointIsEven(new Point(cellX, cellY));
                        cellX = temp.X;
                        cellY = temp.Y;
                        points = new[] {new Point(cellX, cellY)};
                        AddCellInfoPoints(points);
                        M2CellInfo[cellX, cellY].BackIndex = Convert.ToInt16(selectListItem.Value);
                        M2CellInfo[cellX, cellY].BackImage = selectImageIndex + 1;
                    }
                    break;
                case Layer.MiddleImage:
                    if (selectListItem == null) return;
                    points = new[] {new Point(cellX, cellY)};
                    AddCellInfoPoints(points);
                    M2CellInfo[cellX, cellY].MiddleIndex = Convert.ToInt16(selectListItem.Value);
                    M2CellInfo[cellX, cellY].MiddleImage = Convert.ToInt16(selectImageIndex + 1);

                    break;
                case Layer.FrontImage:
                    if (selectListItem == null) return;
                    points = new[] {new Point(cellX, cellY)};
                    AddCellInfoPoints(points);
                    M2CellInfo[cellX, cellY].FrontIndex = Convert.ToInt16(selectListItem.Value);
                    M2CellInfo[cellX, cellY].FrontImage = Convert.ToInt16(selectImageIndex + 1);
                    break;
                case Layer.BackLimit:
                    points = new[] {new Point(cellX, cellY)};
                    AddCellInfoPoints(points);
                    SetBackLimit();
                    break;
                case Layer.FrontLimit:
                    points = new[] {new Point(cellX, cellY)};
                    AddCellInfoPoints(points);
                    SetFrontLimit();
                    break;
                case Layer.BackFrontLimit:
                    points = new[] {new Point(cellX, cellY)};
                    AddCellInfoPoints(points);
                    SetBackLimit();
                    SetFrontLimit();
                    break;
                case Layer.PlaceObjects:
                    points = GetObjectDatasPoints(objectDatas);
                    if (AddCellInfoPoints(points) && (objectDatas != null))
                    {
                        ModifyM2CellInfo(objectDatas);
                    }
                    break;
                case Layer.ClearAll:
                    points = new[] {new Point(cellX, cellY)};
                    AddCellInfoPoints(points);
                    ClearAll();
                    break;
                case Layer.ClearBack:
                    points = new[] {new Point(cellX, cellY)};
                    AddCellInfoPoints(points);
                    ClearBack();
                    break;
                case Layer.ClearMidd:
                    points = new[] {new Point(cellX, cellY)};
                    AddCellInfoPoints(points);
                    ClearMidd();
                    break;
                case Layer.ClearFront:
                    points = new[] {new Point(cellX, cellY)};
                    AddCellInfoPoints(points);
                    ClearFront();
                    break;
                case Layer.ClearBackFrontLimit:
                    points = new[] {new Point(cellX, cellY)};
                    AddCellInfoPoints(points);
                    ClearBackLimit();
                    ClearFrontLimit();
                    break;
                case Layer.ClearBackLimit:
                    points = new[] {new Point(cellX, cellY)};
                    AddCellInfoPoints(points);
                    ClearBackLimit();
                    break;
                case Layer.ClearFrontLimit:
                    points = new[] {new Point(cellX, cellY)};
                    AddCellInfoPoints(points);
                    ClearFrontLimit();
                    break;
                case Layer.BrushMir2BigTiles:
                    temp = CheckPointIsEven(new Point(cellX, cellY));
                    cellX = temp.X;
                    cellY = temp.Y;
                    CreateMir2BigTiles();
                    AddCellInfoPoints(bigTilePoints.ToArray());
                    break;
                case Layer.BrushSmTiles:
                    CreateSmTiles();
                    AddCellInfoPoints(smTilePoints.ToArray());
                    break;
                case Layer.BrushMir3BigTiles:
                    temp = CheckPointIsEven(new Point(cellX, cellY));
                    cellX = temp.X;
                    cellY = temp.Y;
                    CreateMir3BigTiles();
                    AddCellInfoPoints(bigTilePoints.ToArray());
                    break;
            }
        }

        private Point[] GetObjectDatasPoints(CellInfoData[] datas)
        {
            if (datas == null) return null;
            var list = new List<Point>();
            for (var i = 0; i < datas.Length; i++)
            {
                list.Add(new Point(datas[i].X + cellX, datas[i].Y + cellY));
            }
            return list.ToArray();
        }

        private void ModifyM2CellInfo(CellInfoData[] datas)
        {
            for (var i = 0; i < objectDatas.Length; i++)
            {
                if ((datas[i].CellInfo.BackImage & 0x20000000) != 0)
                {
                    if (((datas[i].CellInfo.BackImage & 0x1FFFF) - 1) == -1 || datas[i].CellInfo.BackIndex == -1)
                    {
                        M2CellInfo[cellX + datas[i].X, cellY + datas[i].Y].BackImage |= 0x20000000;

                    }
                    else
                    {
                        M2CellInfo[cellX + datas[i].X, cellY + datas[i].Y].BackImage = datas[i].CellInfo.BackImage;
                        M2CellInfo[cellX + datas[i].X, cellY + datas[i].Y].BackIndex = datas[i].CellInfo.BackIndex;
                    }
                }
                else
                {
                    if (((datas[i].CellInfo.BackImage & 0x1FFFF) - 1) == -1 || datas[i].CellInfo.BackIndex == -1)
                    {
                        //do nothing
                    }
                    else
                    {
                        M2CellInfo[cellX + datas[i].X, cellY + datas[i].Y].BackImage = datas[i].CellInfo.BackImage;
                        M2CellInfo[cellX + datas[i].X, cellY + datas[i].Y].BackIndex = datas[i].CellInfo.BackIndex;
                    }
                }

                //middle
                if (objectDatas[i].CellInfo.MiddleImage != 0)
                {
                    if ((M2CellInfo[cellX + datas[i].X, cellY + datas[i].Y].MiddleAnimationFrame & 0x7F) != 0)
                    {
                        M2CellInfo[cellX + datas[i].X, cellY + datas[i].Y].MiddleAnimationFrame = 0;
                        M2CellInfo[cellX + datas[i].X, cellY + datas[i].Y].MiddleAnimationTick = 0;
                    }
                    M2CellInfo[cellX + datas[i].X, cellY + datas[i].Y].MiddleImage = datas[i].CellInfo.MiddleImage;
                    M2CellInfo[cellX + datas[i].X, cellY + datas[i].Y].MiddleIndex = datas[i].CellInfo.MiddleIndex;
                }
                if (objectDatas[i].CellInfo.MiddleIndex != 0)
                {
                    M2CellInfo[cellX + datas[i].X, cellY + datas[i].Y].MiddleIndex =
                        datas[i].CellInfo.MiddleIndex;
                }
                //front
                if (objectDatas[i].CellInfo.FrontImage != 0)
                {
                    if ((M2CellInfo[cellX + datas[i].X, cellY + datas[i].Y].FrontAnimationFrame & 0x7F) != 0)
                    {
                        M2CellInfo[cellX + datas[i].X, cellY + datas[i].Y].FrontAnimationFrame = 0;
                        M2CellInfo[cellX + datas[i].X, cellY + datas[i].Y].FrontAnimationTick = 0;
                    }
                    M2CellInfo[cellX + datas[i].X, cellY + datas[i].Y].FrontImage = datas[i].CellInfo.FrontImage;
                    M2CellInfo[cellX + datas[i].X, cellY + datas[i].Y].FrontIndex = datas[i].CellInfo.FrontIndex;
                }
                if (objectDatas[i].CellInfo.FrontIndex != 0)
                {
                    M2CellInfo[cellX + datas[i].X, cellY + datas[i].Y].FrontIndex = datas[i].CellInfo.FrontIndex;
                }
                //Door
                if (datas[i].CellInfo.DoorIndex != 0)
                {
                    M2CellInfo[cellX + datas[i].X, cellY + datas[i].Y].DoorIndex = datas[i].CellInfo.DoorIndex;
                }
                if (datas[i].CellInfo.DoorOffset != 0)
                {
                    M2CellInfo[cellX + datas[i].X, cellY + datas[i].Y].DoorOffset = datas[i].CellInfo.DoorOffset;
                }
                //Animation
                if (datas[i].CellInfo.MiddleAnimationFrame != 0)
                {
                    M2CellInfo[cellX + datas[i].X, cellY + datas[i].Y].MiddleAnimationFrame = datas[i].CellInfo.MiddleAnimationFrame;
                }
                if (datas[i].CellInfo.MiddleAnimationTick != 0)
                {
                    M2CellInfo[cellX + datas[i].X, cellY + datas[i].Y].MiddleAnimationTick = datas[i].CellInfo.MiddleAnimationTick;
                }
                if (datas[i].CellInfo.FrontAnimationFrame != 0)
                {
                    M2CellInfo[cellX + datas[i].X, cellY + datas[i].Y].FrontAnimationFrame = datas[i].CellInfo.FrontAnimationFrame;
                }
                if (datas[i].CellInfo.FrontAnimationTick != 0)
                {
                    M2CellInfo[cellX + datas[i].X, cellY + datas[i].Y].FrontAnimationTick = datas[i].CellInfo.FrontAnimationTick;
                }
                //light
                if (datas[i].CellInfo.Light != 0)
                {
                    M2CellInfo[cellX + datas[i].X, cellY + datas[i].Y].Light = datas[i].CellInfo.Light;
                }
            }
        }

        private bool CheckCellInfoIsZero(CellInfo cellInfo)
        {
            if ((cellInfo.BackImage == 0) &&
                (cellInfo.BackIndex == 0) &&
                (cellInfo.MiddleImage == 0) &&
                (cellInfo.MiddleIndex == 0) &&
                (cellInfo.FrontImage == 0) &&
                (cellInfo.FrontIndex == 0) &&
                (cellInfo.DoorIndex == 0) &&
                (cellInfo.DoorOffset == 0) &&
                (cellInfo.FrontAnimationFrame == 0) &&
                (cellInfo.FrontAnimationTick == 0) &&
                (cellInfo.MiddleAnimationFrame == 0) &&
                (cellInfo.MiddleAnimationTick == 0) &&
                (cellInfo.TileAnimationImage == 0) &&
                (cellInfo.TileAnimationOffset == 0) &&
                (cellInfo.TileAnimationFrames == 0) &&
                (cellInfo.Light == 0))
            {
                return true;
            }
            return false;
        }

        private void ShandaMir2LibListView_SelectedIndexChanged(object sender, EventArgs e)
        {
            selectListItem = shangdaMir2ListItem;
            if (ShandaMir2LibListView.SelectedIndices.Count > 0)
            {
                selectLibMImage =
                    Libraries.MapLibs[shangdaMir2ListItem.Value].GetMImage(ShandaMir2LibListView.SelectedIndices[0]);
                if (selectLibMImage != null)
                {
                    selectImageIndex = ShandaMir2LibListView.SelectedIndices[0];

                    picShandaMir2.Image = selectLibMImage.Image;
                    labShandaMir2Width.Text = "Width : " + selectLibMImage.Width;
                    labShandaMir2Height.Text = "Height : " + selectLibMImage.Height;
                    labShandaMir2OffSetX.Text = "OffSetX : " + selectLibMImage.X;
                    labshandaMir2OffSetY.Text = "OffSetY : " + selectLibMImage.Y;
                    Libraries.MapLibs[shangdaMir2ListItem.Value].Images[ShandaMir2LibListView.SelectedIndices[0]] = null;
                }
            }
        }

        private void WemadeMir3LibListView_SelectedIndexChanged(object sender, EventArgs e)
        {
            selectListItem = wemadeMir3ListItem;
            if (WemadeMir3LibListView.SelectedIndices.Count > 0)
            {
                selectLibMImage =
                    Libraries.MapLibs[wemadeMir3ListItem.Value].GetMImage(WemadeMir3LibListView.SelectedIndices[0]);
                if (selectLibMImage != null)
                {
                    selectImageIndex = WemadeMir3LibListView.SelectedIndices[0];

                    picWemdeMir3.Image = selectLibMImage.Image;
                    LabWemadeMir3Width.Text = "Width : " + selectLibMImage.Width;
                    LabWemadeMir3Height.Text = "Height : " + selectLibMImage.Height;
                    labeWemadeMir3OffSetX.Text = "OffSetX : " + selectLibMImage.X;
                    labWemadeMir3OffSetY.Text = "OffSetY : " + selectLibMImage.Y;
                    Libraries.MapLibs[wemadeMir3ListItem.Value].Images[WemadeMir3LibListView.SelectedIndices[0]] = null;
                }
            }
        }

        private void ShandaMir3LibListView_SelectedIndexChanged(object sender, EventArgs e)
        {
            selectListItem = shangdaMir3ListItem;
            if (ShandaMir3LibListView.SelectedIndices.Count > 0)
            {
                selectLibMImage =
                    Libraries.MapLibs[shangdaMir3ListItem.Value].GetMImage(ShandaMir3LibListView.SelectedIndices[0]);
                if (selectLibMImage != null)
                {
                    selectImageIndex = ShandaMir3LibListView.SelectedIndices[0];
                    picShandaMir3.Image = selectLibMImage.Image;
                    labShandaMir3Width.Text = "Width : " + selectLibMImage.Width;
                    labShandaMir3Height.Text = "Height : " + selectLibMImage.Height;
                    labShandaMir3OffSetX.Text = "OffSetX : " + selectLibMImage.X;
                    labshandaMir3OffSetY.Text = "OffSetY : " + selectLibMImage.Y;
                    Libraries.MapLibs[shangdaMir3ListItem.Value].Images[ShandaMir3LibListView.SelectedIndices[0]] = null;
                }
            }
        }

        private void chkDrawGrids_Click(object sender, EventArgs e)
        {
            chkDrawGrids.Checked = !chkDrawGrids.Checked;
        }

        private bool AddCellInfoPoints(Point[] points)
        {
            if (points == null) return false;
            if (M2CellInfo != null)
            {
                cellInfoDatas = new CellInfoData[points.Length];
                for (var i = 0; i < cellInfoDatas.Length; i++)
                {
                    if (points[i].X >= mapWidth || points[i].Y >= mapHeight || points[i].X < 0 || points[i].Y < 0)
                    {
                        MessageBox.Show("放置位置不对，或地图过小！");
                        return false;
                    }
                    cellInfoDatas[i] = new CellInfoData(points[i].X, points[i].Y,
                        M2CellInfo[points[i].X, points[i].Y]);
                }
                _editor.UnDo = cellInfoDatas;
                return true;
            }
            return false;
        }

        private bool AddCellInfoPoints(CellInfoData[] datas)
        {
            if (datas.Length <= 0)
            {
                return false;
            }
            _editor.UnDo = datas;
            return true;
        }

        private void UnDo()
        {
            unTemp = _editor.UnDo;
            if (unTemp == null) return;
            reTemp = new CellInfoData[unTemp.Length];
            for (var i = 0; i < unTemp.Length; i++)
            {
                var x = unTemp[i].X;
                var y = unTemp[i].Y;
                reTemp[i] = new CellInfoData(unTemp[i].X, unTemp[i].Y, M2CellInfo[unTemp[i].X, unTemp[i].Y]);
                M2CellInfo[x, y] = unTemp[i].CellInfo;
            }
            _editor.ReDo = reTemp;
        }

        private void ReDo()
        {
            reTemp = _editor.ReDo;
            if (reTemp == null) return;
            unTemp = new CellInfoData[reTemp.Length];
            for (var i = 0; i < reTemp.Length; i++)
            {
                var x = reTemp[i].X;
                var y = reTemp[i].Y;
                unTemp[i] = new CellInfoData(reTemp[i].X, reTemp[i].Y, M2CellInfo[reTemp[i].X, reTemp[i].Y]);
                M2CellInfo[x, y] = reTemp[i].CellInfo;
            }
            _editor.UnDo = unTemp;
        }

        private void btnUnDo_Click(object sender, EventArgs e)
        {
            UnDo();
        }

        private void btnReDo_Click(object sender, EventArgs e)
        {
            ReDo();
        }

        private void btnSaveToObjects_Click(object sender, EventArgs e)
        {
            if (M2CellInfo == null) return;
            SaveObjectsFile();
            ObjectslistBox.Items.Clear();
            ReadObjectsToListBox();
        }

        private void SaveObjectsFile()
        {
            var saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Object (*.X)|*.X";
            saveFileDialog.FileName = "Object";
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                var file = new FileStream(saveFileDialog.FileName, FileMode.Create, FileAccess.ReadWrite);
                var binaryWriter = new BinaryWriter(file);
                var flag = false;
                int tempX = 0, tempY = 0;
                var list = new List<CellInfoData>();
                for (var y = 0; y < mapHeight; y++)
                {
                    for (var x = 0; x < mapWidth; x++)
                    {
                        if (!flag)
                        {
                            if (CheckCellInfoIsZero(M2CellInfo[x, y]))
                            {
                            }
                            else
                            {
                                flag = true;
                                tempX = x;
                                tempY = y;
                                list.Add(new CellInfoData(x - tempX, y - tempY, M2CellInfo[x, y]));
                            }
                        }
                        else
                        {
                            if (CheckCellInfoIsZero(M2CellInfo[x, y]))
                            {
                            }
                            else
                            {
                                list.Add(new CellInfoData(x - tempX, y - tempY, M2CellInfo[x, y]));
                            }
                        }
                    }
                }

                binaryWriter.Write(list.Count);
                for (var i = 0; i < list.Count; i++)
                {
                    binaryWriter.Write(list[i].X);
                    binaryWriter.Write(list[i].Y);
                    binaryWriter.Write(list[i].CellInfo.BackIndex);
                    binaryWriter.Write(list[i].CellInfo.BackImage);
                    binaryWriter.Write(list[i].CellInfo.MiddleIndex);
                    binaryWriter.Write(list[i].CellInfo.MiddleImage);
                    binaryWriter.Write(list[i].CellInfo.FrontIndex);
                    binaryWriter.Write(list[i].CellInfo.FrontImage);
                    binaryWriter.Write(list[i].CellInfo.DoorIndex);
                    binaryWriter.Write(list[i].CellInfo.DoorOffset);
                    binaryWriter.Write(list[i].CellInfo.FrontAnimationFrame);
                    binaryWriter.Write(list[i].CellInfo.FrontAnimationTick);
                    binaryWriter.Write(list[i].CellInfo.MiddleAnimationFrame);
                    binaryWriter.Write(list[i].CellInfo.MiddleAnimationTick);
                    binaryWriter.Write(list[i].CellInfo.TileAnimationImage);
                    binaryWriter.Write(list[i].CellInfo.TileAnimationOffset);
                    binaryWriter.Write(list[i].CellInfo.TileAnimationFrames);
                    binaryWriter.Write(list[i].CellInfo.Light);
                }
                binaryWriter.Flush();
                binaryWriter.Dispose();
                file.Dispose();
            }
        }

        private void MapPanel_MouseDown(object sender, MouseEventArgs e)
        {
            if (M2CellInfo != null)
            {
                switch (layer)
                {
                    case Layer.GraspingMir2Front:
                        p1 = new Point(cellX, cellY);
                        p2 = Point.Empty;
                        Grasping = true;
                        break;
                    case Layer.GraspingInvertMir3FrontMiddle:
                        p1 = new Point(cellX, cellY);
                        p2 = Point.Empty;
                        Grasping = true;
                        break;
                }
            }
        }

        private void MapPanel_MouseUp(object sender, MouseEventArgs e)
        {
            if (M2CellInfo != null)
            {
                switch (layer)
                {
                    case Layer.GraspingMir2Front:
                        GraspingData();
                        Grasping = false;
                        break;
                    case Layer.GraspingInvertMir3FrontMiddle:
                        GraspingData();
                        Grasping = false;
                        break;
                }
            }
        }

        private void GraspingData()
        {
            if (p1.IsEmpty || p2.IsEmpty) return;
            if ((layer == Layer.GraspingMir2Front) || (layer == Layer.GraspingInvertMir3FrontMiddle))
            {
                if (M2CellInfo != null)
                {
                    var w = Math.Abs(p2.X - p1.X + 1);
                    var h = Math.Abs(p2.Y - p1.Y + 1);
                    objectDatas = new CellInfoData[w*h];
                    var z = 0;
                    for (int x = p1.X, i = 0; x <= p2.X; x++, i++)
                    {
                        for (int y = p1.Y, j = 0; y <= p2.Y; y++, j++)
                        {
                            objectDatas[z] = new CellInfoData();
                            objectDatas[z].CellInfo = new CellInfo();
                            objectDatas[z].X = x - p1.X - 1;
                            objectDatas[z].Y = y - p2.Y - 1;
                            objectDatas[z].CellInfo.BackImage = M2CellInfo[x, y].BackImage & 0x20000000;
                            objectDatas[z].CellInfo.BackIndex = 0;
                            if (layer == Layer.GraspingMir2Front)
                            {
                                objectDatas[z].CellInfo.MiddleImage = 0;
                                objectDatas[z].CellInfo.MiddleIndex = 0;
                            }
                            else if (layer == Layer.GraspingInvertMir3FrontMiddle)
                            {
                                objectDatas[z].CellInfo.MiddleImage = M2CellInfo[x, y].MiddleImage;
                                objectDatas[z].CellInfo.MiddleIndex = M2CellInfo[x, y].MiddleIndex;
                            }

                            objectDatas[z].CellInfo.MiddleAnimationFrame = 0;
                            objectDatas[z].CellInfo.MiddleAnimationTick = 0;
                            objectDatas[z].CellInfo.FrontImage = M2CellInfo[x, y].FrontImage;
                            objectDatas[z].CellInfo.FrontIndex = M2CellInfo[x, y].FrontIndex;
                            objectDatas[z].CellInfo.FrontAnimationFrame = M2CellInfo[x, y].FrontAnimationFrame;
                            objectDatas[z].CellInfo.FrontAnimationTick = M2CellInfo[x, y].FrontAnimationTick;
                            objectDatas[z].CellInfo.DoorIndex = M2CellInfo[x, y].DoorIndex;
                            objectDatas[z].CellInfo.DoorOffset = M2CellInfo[x, y].DoorOffset;
                            objectDatas[z].CellInfo.TileAnimationImage = M2CellInfo[x, y].TileAnimationImage;
                            objectDatas[z].CellInfo.TileAnimationFrames = M2CellInfo[x, y].TileAnimationFrames;
                            objectDatas[z].CellInfo.TileAnimationOffset = M2CellInfo[x, y].TileAnimationOffset;
                            objectDatas[z].CellInfo.Light = M2CellInfo[x, y].Light;
                            objectDatas[z].CellInfo.FishingCell = M2CellInfo[x, y].FishingCell;
                            z++;
                        }
                    }
                }
            }
        }

        private void ReadObjectsToListBox()
        {
            if (!Directory.Exists(Libraries.ObjectsPath))
            {
                Directory.CreateDirectory(Libraries.ObjectsPath);
            }

            foreach (string file in Directory.EnumerateFileSystemEntries(Libraries.ObjectsPath, "*.X", SearchOption.AllDirectories))
            {
                ObjectslistBox.Items.Add(file.Replace(Libraries.ObjectsPath, "").Replace(".X", ""));
            }
        }

        private CellInfoData[] ReadObjectsFile(string objectFile)
        {
            byte[] Bytes;
            var offset = 0;
            var count = 0;
            CellInfoData[] objectDatas = null;
            if (File.Exists(objectFile))
            {
                Bytes = File.ReadAllBytes(objectFile);
                count = BitConverter.ToInt32(Bytes, offset);
                offset += 4;
                objectDatas = new CellInfoData[count];
                for (var i = 0; i < count; i++)
                {
                    objectDatas[i] = new CellInfoData();
                    objectDatas[i].CellInfo = new CellInfo();
                    objectDatas[i].X = BitConverter.ToInt32(Bytes, offset);
                    offset += 4;
                    objectDatas[i].Y = BitConverter.ToInt32(Bytes, offset);
                    offset += 4;
                    objectDatas[i].CellInfo.BackIndex = BitConverter.ToInt16(Bytes, offset);
                    offset += 2;
                    objectDatas[i].CellInfo.BackImage = BitConverter.ToInt32(Bytes, offset);
                    offset += 4;
                    objectDatas[i].CellInfo.MiddleIndex = BitConverter.ToInt16(Bytes, offset);
                    offset += 2;
                    objectDatas[i].CellInfo.MiddleImage = BitConverter.ToInt16(Bytes, offset);
                    offset += 2;
                    objectDatas[i].CellInfo.FrontIndex = BitConverter.ToInt16(Bytes, offset);
                    offset += 2;
                    objectDatas[i].CellInfo.FrontImage = BitConverter.ToInt16(Bytes, offset);
                    offset += 2;
                    objectDatas[i].CellInfo.DoorIndex = Bytes[offset++];
                    objectDatas[i].CellInfo.DoorOffset = Bytes[offset++];
                    objectDatas[i].CellInfo.FrontAnimationFrame = Bytes[offset++];
                    objectDatas[i].CellInfo.FrontAnimationTick = Bytes[offset++];
                    objectDatas[i].CellInfo.MiddleAnimationFrame = Bytes[offset++];
                    objectDatas[i].CellInfo.MiddleAnimationTick = Bytes[offset++];
                    objectDatas[i].CellInfo.TileAnimationImage = BitConverter.ToInt16(Bytes, offset);
                    offset += 2;
                    objectDatas[i].CellInfo.TileAnimationOffset = BitConverter.ToInt16(Bytes, offset);
                    offset += 2;
                    objectDatas[i].CellInfo.TileAnimationFrames = Bytes[offset++];
                    objectDatas[i].CellInfo.Light = Bytes[offset++];

                    if (objectDatas[i].CellInfo.Light == 100 || objectDatas[i].CellInfo.Light == 101)
                        objectDatas[i].CellInfo.FishingCell = true;
                }
            }
            return objectDatas;
        }

        private Bitmap GetObjectPreview(int w, int h, CellInfoData[] datas)
        {
            if (datas == null)
            {
                return null;
            }
            var preview = new Bitmap(w*CellWidth, h*CellHeight);
            var graphics = Graphics.FromImage(preview);
            graphics.InterpolationMode = InterpolationMode.Low;

            for (var i = 0; i < datas.Length; i++)
            {
                if (datas[i].Y%2 != 0) continue;
                drawY = (datas[i].Y + h/4)*CellHeight;
                if (datas[i].X%2 != 0) continue;
                drawX = (datas[i].X + w/4)*CellWidth;

                index = (datas[i].CellInfo.BackImage & 0x1FFFFFFF) - 1;
                libIndex = datas[i].CellInfo.BackIndex;

                if (libIndex < 0 || libIndex >= Libraries.MapLibs.Length) continue;
                if (index < 0 || index >= Libraries.MapLibs[libIndex].Images.Count) continue;

                Libraries.MapLibs[libIndex].CheckImage(index);
                var mi = Libraries.MapLibs[libIndex].Images[index];

                if (mi.Image == null) continue;

                var destRect = new Rectangle(drawX, drawY, mi.Width, mi.Height);
                var srcRect = new Rectangle(0, 0, mi.Width, mi.Height);
                graphics.DrawImage(mi.Image, destRect, srcRect, GraphicsUnit.Pixel);
                Libraries.MapLibs[libIndex].Images[index] = null;
            }

            for (var i = 0; i < datas.Length; i++)
            {
                drawX = (datas[i].X + w/4)*CellWidth;
                index = datas[i].CellInfo.MiddleImage - 1;
                libIndex = datas[i].CellInfo.MiddleIndex;
                if (libIndex < 0 || libIndex >= Libraries.MapLibs.Length) continue;
                if (index < 0 || index >= Libraries.MapLibs[libIndex].Images.Count) continue;

                var s = Libraries.MapLibs[libIndex].GetSize(index);
                if ((s.Width != CellWidth || s.Height != CellHeight) &&
                    (s.Width != CellWidth*2 || s.Height != CellHeight*2))
                {
                    drawY = (datas[i].Y + 1 + h/4)*CellHeight - s.Height;
                }
                else
                {
                    drawY = (datas[i].Y + h/4)*CellHeight;
                }
                Libraries.MapLibs[libIndex].CheckImage(index);
                var mi = Libraries.MapLibs[libIndex].Images[index];
                if (mi.Image == null) continue;
                var destRect = new Rectangle(drawX, drawY, mi.Width, mi.Height);
                var srcRect = new Rectangle(0, 0, mi.Width, mi.Height);
                graphics.DrawImage(mi.Image, destRect, srcRect, GraphicsUnit.Pixel);
                Libraries.MapLibs[libIndex].Images[index] = null;
            }

            for (var i = 0; i < datas.Length; i++)
            {
                drawX = (datas[i].X + w/4)*CellWidth;
                index = (datas[i].CellInfo.FrontImage & 0x7FFF) - 1;
                libIndex = datas[i].CellInfo.FrontIndex;
                if (libIndex < 0 || libIndex >= Libraries.MapLibs.Length) continue;
                if (index < 0 || index >= Libraries.MapLibs[libIndex].Images.Count) continue;

                var s = Libraries.MapLibs[libIndex].GetSize(index);
                if ((s.Width != CellWidth || s.Height != CellHeight) &&
                    (s.Width != CellWidth*2 || s.Height != CellHeight*2))
                {
                    drawY = (datas[i].Y + 1 + h/4)*CellHeight - s.Height;
                }
                else
                {
                    drawY = (datas[i].Y + h/4)*CellHeight;
                }
                Libraries.MapLibs[libIndex].CheckImage(index);
                var mi = Libraries.MapLibs[libIndex].Images[index];
                if (mi.Image == null) continue;
                var destRect = new Rectangle(drawX, drawY, mi.Width, mi.Height);
                var srcRect = new Rectangle(0, 0, mi.Width, mi.Height);
                graphics.DrawImage(mi.Image, destRect, srcRect, GraphicsUnit.Pixel);
                Libraries.MapLibs[libIndex].Images[index] = null;
            }

            graphics.Save();
            graphics.Dispose();
            return preview;
        }

        private void chkFrontTag_Click(object sender, EventArgs e)
        {
            chkFrontTag.Checked = !chkFrontTag.Checked;
        }

        private void btnDeleteObjects_Click(object sender, EventArgs e)
        {
            var name = ObjectslistBox.SelectedItem + ".X";
            var objectFile = Application.StartupPath + "\\Objects\\" + name;
            if (File.Exists(objectFile))
            {
                File.Delete(objectFile);
                ObjectslistBox.Items.Clear();
                ReadObjectsToListBox();
            }
        }

        private void ClearAll()
        {
            M2CellInfo[cellX, cellY] = new CellInfo();
        }

        private void ClearBack()
        {
            M2CellInfo[cellX, cellY].BackIndex = 0;
            M2CellInfo[cellX, cellY].BackImage = 0;
        }

        private void ClearMidd()
        {
            M2CellInfo[cellX, cellY].MiddleImage = 0;
            M2CellInfo[cellX, cellY].MiddleIndex = 0;
            M2CellInfo[cellX, cellY].MiddleAnimationFrame = 0;
            M2CellInfo[cellX, cellY].MiddleAnimationTick = 0;
        }

        private void ClearFront()
        {
            M2CellInfo[cellX, cellY].FrontImage = 0;
            M2CellInfo[cellX, cellY].FrontIndex = 0;
            M2CellInfo[cellX, cellY].FrontAnimationFrame = 0;
            M2CellInfo[cellX, cellY].FrontAnimationTick = 0;
        }

        private void ClearBackLimit()
        {
            M2CellInfo[cellX, cellY].BackImage = M2CellInfo[cellX, cellY].BackImage & 0x1fffffff;
        }

        private void ClearFrontLimit()
        {
            M2CellInfo[cellX, cellY].FrontImage = (short) (M2CellInfo[cellX, cellY].FrontImage & 0x7fff);
        }

        private void 撤销ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UnDo();
        }

        private void 返回ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ReDo();
        }

        private void btnSetDoor_Click(object sender, EventArgs e)
        {
            if (M2CellInfo != null)
            {
                Form setDoorForm = new FrmSetDoor(SetDoorProperty);
                if (setDoorForm.ShowDialog() == DialogResult.OK)
                {
                }
            }
        }

        private void SetDoorProperty(bool blCoreDoor, byte index, byte offSet)
        {
            Point[] points = {new Point(cellX, cellY)};
            AddCellInfoPoints(points);

            M2CellInfo[cellX, cellY].DoorIndex = (byte) (M2CellInfo[cellX, cellY].DoorIndex | index);
            M2CellInfo[cellX, cellY].DoorOffset = offSet;
            if (blCoreDoor)
            {
                M2CellInfo[cellX, cellY].DoorIndex = (byte) (M2CellInfo[cellX, cellY].DoorIndex | 0x80);
            }
            else
            {
                M2CellInfo[cellX, cellY].DoorIndex = (byte) (M2CellInfo[cellX, cellY].DoorIndex & 0x7F);
            }
        }

        private void btnSetAnimation_Click(object sender, EventArgs e)
        {
            if (M2CellInfo != null)
            {
                Form setAnimation = new FrmSetAnimation(SetAnimationProperty);
                if (setAnimation.ShowDialog() == DialogResult.OK)
                {
                }
            }
        }

        private void SetAnimationProperty(bool blend, byte frame, byte tick)
        {
            Point[] points = {new Point(cellX, cellY)};
            AddCellInfoPoints(points);
            //(byte)(M2CellInfo[cellX, cellY].FrontAnimationFrame | frame)
            M2CellInfo[cellX, cellY].FrontAnimationFrame = frame;
            M2CellInfo[cellX, cellY].FrontAnimationTick = tick;
            if (blend)
            {
                M2CellInfo[cellX, cellY].FrontAnimationFrame =
                    (byte) (M2CellInfo[cellX, cellY].FrontAnimationFrame | 0x80);
            }
            else
            {
                M2CellInfo[cellX, cellY].FrontAnimationFrame =
                    (byte) (M2CellInfo[cellX, cellY].FrontAnimationFrame & 0x7F);
            }
        }

        private void btnSetLight_Click(object sender, EventArgs e)
        {
            if (M2CellInfo != null)
            {
                Form setLight = new FrmSetLight(SetLightProperty);
                if (setLight.ShowDialog() == DialogResult.OK)
                {
                }
            }
        }

        private void SetLightProperty(byte light)
        {
            Point[] points = {new Point(cellX, cellY)};
            AddCellInfoPoints(points);
            M2CellInfo[cellX, cellY].Light = light;
        }

        private void SetBackLimit()
        {
            if (M2CellInfo != null)
            {
                M2CellInfo[cellX, cellY].BackImage = M2CellInfo[cellX, cellY].BackImage | 0x20000000;
            }
        }

        private void SetFrontLimit()
        {
            if (M2CellInfo != null)
            {
                M2CellInfo[cellX, cellY].FrontImage =
                    (short) (M2CellInfo[cellX, cellY].FrontImage | 0x8000);
            }
        }

        private void btnJump_Click(object sender, EventArgs e)
        {
            if (M2CellInfo != null)
            {
                Form jump = new FrmJump(Jump);
                if (jump.ShowDialog() == DialogResult.OK)
                {
                }
            }
        }

        private void Jump(int x, int y)
        {
            if (x - OffSetX/2 >= mapWidth || y - OffSetY/2 >= mapHeight)
            {
                MessageBox.Show("X,Y is error point");
                return;
            }
            if (x - OffSetX/2 < 0 || y - OffSetY/2 < 0)
            {
                mapPoint.X = x;
                mapPoint.Y = y;
                return;
            }
            mapPoint.X = x - OffSetX/2;
            mapPoint.Y = y - OffSetX/2;
        }

        private void btnAbout_Click(object sender, EventArgs e)
        {
            Form frmAbout = new FrmAbout();
            frmAbout.ShowDialog();
        }

        private void Main_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.D:
                    mapPoint.X++;
                    if (mapPoint.X + OffSetX >= mapWidth)
                    {
                        mapPoint.X = mapWidth - OffSetX - 1;
                    }
                    break;
                case Keys.A:
                    mapPoint.X--;
                    if (mapPoint.X < 0)
                    {
                        mapPoint.X = 0;
                    }
                    break;
                case Keys.W:
                    mapPoint.Y--;
                    if (mapPoint.Y < 0)
                    {
                        mapPoint.Y = 0;
                    }
                    break;
                case Keys.S:
                    mapPoint.Y++;
                    if (mapPoint.Y + OffSetY >= mapHeight)
                    {
                        mapPoint.Y = mapHeight - OffSetY - 1;
                    }
                    break;
                case Keys.Z:
                    selectImageIndex--;
                    break;
                case Keys.X:
                    selectImageIndex++;
                    break;
            }

            if (M2CellInfo != null)
            {
                if (layer == Layer.BackImage)
                {
                    if (e.Control)
                    {
                        keyDown = true;
                    }
                }

                if (layer == Layer.BrushMir2BigTiles || layer == Layer.BrushSmTiles || layer == Layer.BrushMir3BigTiles)
                {
                    if (e.Control)
                    {
                        keyDown = true;
                    }
                }
            }
        }

        private void ObjectslistBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            var name = ObjectslistBox.SelectedItem + ".X";
            var objectFile = Application.StartupPath + "\\Objects\\" + name;
            objectDatas = ReadObjectsFile(objectFile);
            picObjects.Image = GetObjectPreview(26, 24, objectDatas);
        }

        private void chkMiddleTag_Click(object sender, EventArgs e)
        {
            chkMiddleTag.Checked = !chkMiddleTag.Checked;
        }

        private void Main_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.ControlKey)
            {
                keyDown = false;
            }
        }


        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            Dispose();
        }

        private void chkShowCellInfo_Click(object sender, EventArgs e)
        {
            chkShowCellInfo.Checked = !chkShowCellInfo.Checked;
            cellInfoControl.Visible = chkShowCellInfo.Checked;
        }

        private void ShowCellInfo(bool blShow)
        {
            if (blShow)
            {
                cellInfoControl.SetText(
                    cellX,
                    cellY,
                    (M2CellInfo[cellX, cellY].BackImage & 0x1FFFF) - 1,
                    M2CellInfo[cellX, cellY].MiddleImage - 1,
                    (M2CellInfo[cellX, cellY].FrontImage & 0x7FFF) - 1,
                    M2CellInfo[cellX, cellY].BackIndex,
                    M2CellInfo[cellX, cellY].MiddleIndex,
                    M2CellInfo[cellX, cellY].FrontIndex,
                    GetLibName(M2CellInfo[cellX, cellY].BackIndex),
                    GetLibName(M2CellInfo[cellX, cellY].MiddleIndex),
                    GetLibName(M2CellInfo[cellX, cellY].FrontIndex),
                    M2CellInfo[cellX, cellY].BackImage & 0x20000000,
                    M2CellInfo[cellX, cellY].FrontImage & 0x8000,
                    (byte) (M2CellInfo[cellX, cellY].FrontAnimationFrame & 0x7F),
                    M2CellInfo[cellX, cellY].FrontAnimationTick,
                    Convert.ToBoolean(M2CellInfo[cellX, cellY].FrontAnimationFrame & 0x80),
                    M2CellInfo[cellX, cellY].MiddleAnimationFrame,
                    M2CellInfo[cellX, cellY].MiddleAnimationTick,
                    Convert.ToBoolean(M2CellInfo[cellX, cellY].MiddleAnimationFrame),
                    M2CellInfo[cellX, cellY].DoorOffset,
                    (byte) (M2CellInfo[cellX, cellY].DoorIndex & 0x7F),
                    Convert.ToBoolean(M2CellInfo[cellX, cellY].DoorIndex & 0x80),
                    M2CellInfo[cellX, cellY].Light,
                    M2CellInfo[cellX, cellY].FishingCell
                    );
                drawX = (cellX - mapPoint.X + 1)*(CellWidth*zoomMIN/zoomMAX);
                drawY = (cellY - mapPoint.Y + 1)*(CellHeight*zoomMIN/zoomMAX);
                if (drawX + cellInfoControl.Width >= MapPanel.Width)
                {
                    drawX = drawX - cellInfoControl.Width - 1;
                }
                cellInfoControl.Location = new Point(drawX, drawY);
                if (!MapPanel.Controls.Contains(cellInfoControl))
                {
                    MapPanel.Controls.Add(cellInfoControl);
                }
            }
        }

        private void btnMir3ToMir2_Click(object sender, EventArgs e)
        {
            InvertMir3Layer();
        }

        private void WemadeMir2LibListView_Click(object sender, EventArgs e)
        {
            selectListItem = wemadeMir2ListItem;
            //在此处设断点，发现点击不同的Item后，此事件居然执行了2次 //第一次是取消当前Item选中状态，导致整个ListView的SelectedIndices变为0
            //第二次才将新选中的Item设置为选中状态，SelectedIndices变为1
            //如果不加listview.SelectedIndices.Count>0判断，将导致获取listview.Items[]索引超界的异常
            if (WemadeMir2LibListView.SelectedIndices.Count > 0)
            {
                selectLibMImage =
                    Libraries.MapLibs[wemadeMir2ListItem.Value].GetMImage(WemadeMir2LibListView.SelectedIndices[0]);
                if (selectLibMImage != null)
                {
                    selectImageIndex = WemadeMir2LibListView.SelectedIndices[0];

                    picWemdeMir2.Image = selectLibMImage.Image;
                    LabWemadeMir2Width.Text = "Width : " + selectLibMImage.Width;
                    LabWemadeMir2Height.Text = "Height : " + selectLibMImage.Height;
                    labeWemadeMir2OffSetX.Text = "OffSetX : " + selectLibMImage.X;
                    labWemadeMir2OffSetY.Text = "OffSetY : " + selectLibMImage.Y;
                    Libraries.MapLibs[wemadeMir2ListItem.Value].Images[WemadeMir2LibListView.SelectedIndices[0]] = null;
                }
            }
        }

        private void ShandaMir2LibListView_Click(object sender, EventArgs e)
        {
            selectListItem = shangdaMir2ListItem;
            if (ShandaMir2LibListView.SelectedIndices.Count > 0)
            {
                selectLibMImage =
                    Libraries.MapLibs[shangdaMir2ListItem.Value].GetMImage(ShandaMir2LibListView.SelectedIndices[0]);
                if (selectLibMImage != null)
                {
                    selectImageIndex = ShandaMir2LibListView.SelectedIndices[0];

                    picShandaMir2.Image = selectLibMImage.Image;
                    labShandaMir2Width.Text = "Width : " + selectLibMImage.Width;
                    labShandaMir2Height.Text = "Height : " + selectLibMImage.Height;
                    labShandaMir2OffSetX.Text = "OffSetX : " + selectLibMImage.X;
                    labshandaMir2OffSetY.Text = "OffSetY : " + selectLibMImage.Y;
                    Libraries.MapLibs[shangdaMir2ListItem.Value].Images[ShandaMir2LibListView.SelectedIndices[0]] = null;
                }
            }
        }

        private void WemadeMir3LibListView_Click(object sender, EventArgs e)
        {
            selectListItem = wemadeMir3ListItem;
            if (WemadeMir3LibListView.SelectedIndices.Count > 0)
            {
                selectLibMImage =
                    Libraries.MapLibs[wemadeMir3ListItem.Value].GetMImage(WemadeMir3LibListView.SelectedIndices[0]);
                if (selectLibMImage != null)
                {
                    selectImageIndex = WemadeMir3LibListView.SelectedIndices[0];

                    picWemdeMir3.Image = selectLibMImage.Image;
                    LabWemadeMir3Width.Text = "Width : " + selectLibMImage.Width;
                    LabWemadeMir3Height.Text = "Height : " + selectLibMImage.Height;
                    labeWemadeMir3OffSetX.Text = "OffSetX : " + selectLibMImage.X;
                    labWemadeMir3OffSetY.Text = "OffSetY : " + selectLibMImage.Y;
                    Libraries.MapLibs[wemadeMir3ListItem.Value].Images[WemadeMir3LibListView.SelectedIndices[0]] = null;
                }
            }
        }

        private void ShandaMir3LibListView_Click(object sender, EventArgs e)
        {
            selectListItem = shangdaMir3ListItem;
            if (ShandaMir3LibListView.SelectedIndices.Count > 0)
            {
                selectLibMImage =
                    Libraries.MapLibs[shangdaMir3ListItem.Value].GetMImage(ShandaMir3LibListView.SelectedIndices[0]);
                if (selectLibMImage != null)
                {
                    selectImageIndex = ShandaMir3LibListView.SelectedIndices[0];
                    picShandaMir3.Image = selectLibMImage.Image;
                    labShandaMir3Width.Text = "Width : " + selectLibMImage.Width;
                    labShandaMir3Height.Text = "Height : " + selectLibMImage.Height;
                    labShandaMir3OffSetX.Text = "OffSetX : " + selectLibMImage.X;
                    labshandaMir3OffSetY.Text = "OffSetY : " + selectLibMImage.Y;
                    Libraries.MapLibs[shangdaMir3ListItem.Value].Images[ShandaMir3LibListView.SelectedIndices[0]] = null;
                }
            }
        }

        private void CreateMir2BigTiles()
        {
            if (selectListItem == null || selectTilesIndex == -1) return;
            bigTilePoints.Clear();
            PutAutoTile(cellX, cellY, RandomAutoMir2Tile(TileType.Center));
            DrawAutoMir2TileSide(cellX, cellY);
            AutoTileRange = 2;

            for (var i = 0; i < 30; ++i)
            {
                AutoTileChanges = 0;
                DrawAutoMir2TilePattern(cellX, cellY);
                if (AutoTileChanges == 0)
                {
                    break;
                }
                AutoTileRange += 2;
            }
        }

        private void CreateMir3BigTiles()
        {
            if (selectListItem == null || selectTilesIndex == -1) return;
            bigTilePoints.Clear();
            PutAutoTile(cellX, cellY, RandomAutoMir3Tile(TileType.Center));
            DrawAutoMir3TileSide(cellX, cellY);
            AutoTileRange = 2;

            for (var i = 0; i < 30; ++i)
            {
                AutoTileChanges = 0;
                DrawAutoMir3TilePattern(cellX, cellY);
                if (AutoTileChanges == 0)
                {
                    break;
                }
                AutoTileRange += 2;
            }
        }

        private void CreateSmTiles()
        {
            if (selectListItem == null || selectTilesIndex == -1) return;
            smTilePoints.Clear();
            PutAutoSmTile(cellX, cellY, RandomAutoSmTile(TileType.Center));
            DrawAutoSmTileSide(cellX, cellY);
            AutoTileRange = 2;
            for (var i = 0; i < 30; ++i)
            {
                AutoTileChanges = 0;
                DrawAutoSmTilePattern(cellX, cellY);
                if (AutoTileChanges == 0)
                {
                    break;
                }
                AutoTileRange += 2;
            }
        }

        private int GetTile(int x, int y)
        {
            if (x < 0 || (y < 0) || (x >= mapWidth) || (y >= mapHeight))
            {
                return -1;
            }
            return (M2CellInfo[x, y].BackImage & 0X1FFFFFFF) - 1;
        }

        private int GetSmTile(int x, int y)
        {
            if (x < 0 || (y < 0) || (x >= mapWidth) || (y >= mapHeight))
            {
                return -1;
            }
            return M2CellInfo[x, y].MiddleImage - 1;
        }

        private TileType GetAutoMir2TileType(int x, int y)
        {
            var imageIndex = GetTile(x, y);
            if (imageIndex/Mir2BigTileBlock != selectTilesIndex)
            {
                return TileType.None;
            }
            switch (imageIndex%Mir2BigTileBlock)
            {
                case 0:
                case 1:
                case 2:
                case 3:
                case 4:
                    return TileType.Center;
                    break;
                case 5:
                    return TileType.UpLeft;
                    break;
                case 6:
                    return TileType.UpRight;
                    break;
                case 7:
                    return TileType.DownLeft;
                    break;
                case 8:
                    return TileType.DownRight;
                    break;
                case 10:
                    return TileType.InUpLeft;
                    break;
                case 11:
                    return TileType.InUpRight;
                    break;
                case 12:
                    return TileType.InDownLeft;
                    break;
                case 13:
                    return TileType.InDownRight;
                    break;
                case 15:
                case 16:
                    return TileType.Up;
                    break;
                case 17:
                case 18:
                    return TileType.Down;
                    break;
                case 20:
                case 22:
                    return TileType.Left;
                    break;
                case 21:
                case 23:
                    return TileType.Right;
                    break;
            }
            return TileType.None;
        }

        private TileType GetAutoMir3TileType(int x, int y)
        {
            var imageIndex = GetTile(x, y);

            int flag;
            if (Libraries.MapLibs[selectListItem.Value].Images.Count%10 != 0)
            {
                flag = (Libraries.MapLibs[selectListItem.Value].Images.Count + 1)/Mir3BigTileBlock;
            }
            else
            {
                flag = Libraries.MapLibs[selectListItem.Value].Images.Count/Mir3BigTileBlock;
            }

            if ((imageIndex/Mir3BigTileBlock != selectTilesIndex) &&
                imageIndex/Mir3BigTileBlock != selectTilesIndex - flag)
            {
                return TileType.None;
            }
            if (selectTilesIndex < flag)
            {
                switch (imageIndex%Mir3BigTileBlock)
                {
                    case 0:
                    case 1:
                    case 2:
                    case 3:
                    case 4:
                        return TileType.Center;
                        break;
                    case 10:
                        return TileType.UpLeft;
                        break;
                    case 11:
                        return TileType.UpRight;
                        break;
                    case 12:
                        return TileType.DownLeft;
                        break;
                    case 13:
                        return TileType.DownRight;
                        break;
                    case 15:
                        return TileType.InUpLeft;
                        break;
                    case 16:
                        return TileType.InUpRight;
                        break;
                    case 17:
                        return TileType.InDownLeft;
                        break;
                    case 18:
                        return TileType.InDownRight;
                        break;
                    case 20:
                    case 21:
                        return TileType.Up;
                        break;
                    case 22:
                    case 23:
                        return TileType.Down;
                        break;
                    case 25:
                    case 27:
                        return TileType.Left;
                        break;
                    case 26:
                    case 28:
                        return TileType.Right;
                        break;
                }
            }
            else
            {
                switch (imageIndex%Mir3BigTileBlock)
                {
                    case 5:
                    case 6:
                    case 7:
                    case 8:
                    case 9:
                        return TileType.Center;
                        break;
                    case 18:
                        return TileType.UpLeft;
                        break;
                    case 17:
                        return TileType.UpRight;
                        break;
                    case 16:
                        return TileType.DownLeft;
                        break;
                    case 15:
                        return TileType.DownRight;
                        break;
                    case 13:
                        return TileType.InUpLeft;
                        break;
                    case 12:
                        return TileType.InUpRight;
                        break;
                    case 11:
                        return TileType.InDownLeft;
                        break;
                    case 10:
                        return TileType.InDownRight;
                        break;
                    case 22:
                    case 23:
                        return TileType.Up;
                        break;
                    case 20:
                    case 21:
                        return TileType.Down;
                        break;
                    case 26:
                    case 28:
                        return TileType.Left;
                        break;
                    case 25:
                    case 27:
                        return TileType.Right;
                        break;
                }
            }
            return TileType.None;
        }

        private TileType GetAutoSmTileType(int iX, int iY)
        {
            //获取格子iX, iY中小地砖所属的地砖类型, 上中下等等。。。
            int iImageIndex;

            iImageIndex = GetSmTile(iX, iY); //获取小地砖的图片索引
            //然后判断这个索引是否是当前选择的样式
            if (iImageIndex >= selectTilesIndex*smTileBlock && iImageIndex < (selectTilesIndex + 1)*smTileBlock)
            {
                //如果是则可以根据小地砖样式中各种类型地砖的布局来计算出是属于哪种类型的地砖了
                iImageIndex -= selectTilesIndex*smTileBlock;
                if (iImageIndex < 8)
                {
                    return 0;
                }
                return (TileType) ((iImageIndex - 8)/4 + 1);
            }

            //return -1;	//如果不是属于当前的样式的则返回-1
            return TileType.None;
        }

        private void PutAutoTile(int x, int y, int imageIndex)
        {
            if (x < 0 || (y < 0) || (x >= mapWidth) || (y >= mapHeight)) return;

            ++AutoTileChanges;

            for (var i = 0; i < bigTilePoints.Count; i++)
            {
                if (bigTilePoints[i].X == x && bigTilePoints[i].Y == y)
                {
                    M2CellInfo[x, y].BackImage = imageIndex + 1;
                    M2CellInfo[x, y].BackIndex = (short) selectListItem.Value;
                    return;
                }
            }
            bigTilePoints.Add(new CellInfoData(x, y, M2CellInfo[x, y]));
            M2CellInfo[x, y].BackImage = imageIndex + 1;
            M2CellInfo[x, y].BackIndex = (short) selectListItem.Value;
        }

        private void PutAutoSmTile(int x, int y, int imageIndex)
        {
            if (x < 0 || (y < 0) || (x >= mapWidth) || (y >= mapHeight)) return;
            ++AutoTileChanges;

            for (var i = 0; i < smTilePoints.Count; i++)
            {
                if (smTilePoints[i].X == x && smTilePoints[i].Y == y)
                {
                    M2CellInfo[x, y].MiddleImage = (short) (imageIndex + 1);
                    M2CellInfo[x, y].MiddleIndex = (short) selectListItem.Value;
                    return;
                }
            }
            smTilePoints.Add(new CellInfoData(x, y, M2CellInfo[x, y]));
            M2CellInfo[x, y].MiddleImage = (short) (imageIndex + 1);
            M2CellInfo[x, y].MiddleIndex = (short) selectListItem.Value;
        }

        private void DrawAutoMir2TileSide(int iX, int iY)
        {
            if (GetAutoMir2TileType(iX, iY - 2) < 0) //上
            {
                PutAutoTile(iX, iY - 2, RandomAutoMir2Tile(TileType.Up));
            }
            if (GetAutoMir2TileType(iX + 2, iY - 2) < 0) //右上
            {
                PutAutoTile(iX + 2, iY - 2, RandomAutoMir2Tile(TileType.UpRight));
            }
            if (GetAutoMir2TileType(iX + 2, iY) < 0) //右
            {
                PutAutoTile(iX + 2, iY, RandomAutoMir2Tile(TileType.Right));
            }
            if (GetAutoMir2TileType(iX + 2, iY + 2) < 0) //右下
            {
                PutAutoTile(iX + 2, iY + 2, RandomAutoMir2Tile(TileType.DownRight));
            }
            if (GetAutoMir2TileType(iX, iY + 2) < 0) //下
            {
                PutAutoTile(iX, iY + 2, RandomAutoMir2Tile(TileType.Down));
            }
            if (GetAutoMir2TileType(iX - 2, iY + 2) < 0) //左下
            {
                PutAutoTile(iX - 2, iY + 2, RandomAutoMir2Tile(TileType.DownLeft));
            }
            if (GetAutoMir2TileType(iX - 2, iY) < 0) //左
            {
                PutAutoTile(iX - 2, iY, RandomAutoMir2Tile(TileType.Left));
            }
            if (GetAutoMir2TileType(iX - 2, iY - 2) < 0) //左上
            {
                PutAutoTile(iX - 2, iY - 2, RandomAutoMir2Tile(TileType.UpLeft));
            }
        }

        private void DrawAutoMir3TileSide(int iX, int iY)
        {
            if (GetAutoMir3TileType(iX, iY - 2) < 0) //上
            {
                PutAutoTile(iX, iY - 2, RandomAutoMir3Tile(TileType.Up));
            }
            if (GetAutoMir3TileType(iX + 2, iY - 2) < 0) //右上
            {
                PutAutoTile(iX + 2, iY - 2, RandomAutoMir3Tile(TileType.UpRight));
            }
            if (GetAutoMir3TileType(iX + 2, iY) < 0) //右
            {
                PutAutoTile(iX + 2, iY, RandomAutoMir3Tile(TileType.Right));
            }
            if (GetAutoMir3TileType(iX + 2, iY + 2) < 0) //右下
            {
                PutAutoTile(iX + 2, iY + 2, RandomAutoMir3Tile(TileType.DownRight));
            }
            if (GetAutoMir3TileType(iX, iY + 2) < 0) //下
            {
                PutAutoTile(iX, iY + 2, RandomAutoMir3Tile(TileType.Down));
            }
            if (GetAutoMir3TileType(iX - 2, iY + 2) < 0) //左下
            {
                PutAutoTile(iX - 2, iY + 2, RandomAutoMir3Tile(TileType.DownLeft));
            }
            if (GetAutoMir3TileType(iX - 2, iY) < 0) //左
            {
                PutAutoTile(iX - 2, iY, RandomAutoMir3Tile(TileType.Left));
            }
            if (GetAutoMir3TileType(iX - 2, iY - 2) < 0) //左上
            {
                PutAutoTile(iX - 2, iY - 2, RandomAutoMir3Tile(TileType.UpLeft));
            }
        }

        private void DrawAutoSmTileSide(int iX, int iY)
        {
            //这个就是绘制一个边
            if (GetAutoSmTileType(iX, iY - 1) < 0) //上 上下左右这样逐个绘制, 不过绘制之前先要检查这个格子是否已经有当前样式的地砖, 如果有则不绘制
            {
                PutAutoSmTile(iX, iY - 1, RandomAutoSmTile(TileType.Up)); //随机返回上的一个地砖然后绘制
            }
            if (GetAutoSmTileType(iX + 1, iY - 1) < 0) //右上
            {
                PutAutoSmTile(iX + 1, iY - 1, RandomAutoSmTile(TileType.UpRight));
            }
            if (GetAutoSmTileType(iX + 1, iY) < 0) //右
            {
                PutAutoSmTile(iX + 1, iY, RandomAutoSmTile(TileType.Right));
            }
            if (GetAutoSmTileType(iX + 1, iY + 1) < 0) //右下
            {
                PutAutoSmTile(iX + 1, iY + 1, RandomAutoSmTile(TileType.DownRight));
            }
            if (GetAutoSmTileType(iX, iY + 1) < 0) //下
            {
                PutAutoSmTile(iX, iY + 1, RandomAutoSmTile(TileType.Down));
            }
            if (GetAutoSmTileType(iX - 1, iY + 1) < 0) //左下
            {
                PutAutoSmTile(iX - 1, iY + 1, RandomAutoSmTile(TileType.DownLeft));
            }
            if (GetAutoSmTileType(iX - 1, iY) < 0) //左
            {
                PutAutoSmTile(iX - 1, iY, RandomAutoSmTile(TileType.Left));
            }
            if (GetAutoSmTileType(iX - 1, iY - 1) < 0) //左上
            {
                PutAutoSmTile(iX - 1, iY - 1, RandomAutoSmTile(TileType.UpLeft));
            }
        }

        private void DrawAutoMir2TilePattern(int iX, int iY)
        {
            int i, j, c;
            TileType n1, n2;

            for (j = iY - AutoTileRange; j <= iY + AutoTileRange; j += 2) //间隔为2
            {
                for (i = iX - AutoTileRange; i <= iX + AutoTileRange; i += 2) //间隔为2, 其他算法跟小地砖一样
                {
                    if (i > 1 && j > 1)
                    {
                        if (GetAutoMir2TileType(i, j) > 0)
                        {
                            //Check CENTER
                            if (GetAutoMir2TileType(i, j) != TileType.Center)
                            {
                                c = 0;
                                if (GetAutoMir2TileType(i, j - 2) >= 0)
                                {
                                    ++c;
                                }
                                if (GetAutoMir2TileType(i + 2, j - 2) >= 0)
                                {
                                    ++c;
                                }
                                if (GetAutoMir2TileType(i + 2, j) >= 0)
                                {
                                    ++c;
                                }
                                if (GetAutoMir2TileType(i + 2, j + 2) >= 0)
                                {
                                    ++c;
                                }
                                if (GetAutoMir2TileType(i, j + 2) >= 0)
                                {
                                    ++c;
                                }
                                if (GetAutoMir2TileType(i - 2, j + 2) >= 0)
                                {
                                    ++c;
                                }
                                if (GetAutoMir2TileType(i - 2, j) >= 0)
                                {
                                    ++c;
                                }
                                if (GetAutoMir2TileType(i - 2, j - 2) >= 0)
                                {
                                    ++c;
                                }
                                if (c >= 8)
                                {
                                    PutAutoTile(i, j, RandomAutoMir2Tile(TileType.Center));
                                }
                            }

                            //Check UP
                            if (GetAutoMir2TileType(i, j) != TileType.Up)
                            {
                                n1 = GetAutoMir2TileType(i - 2, j);
                                n2 = GetAutoMir2TileType(i + 2, j);
                                if ((n1 == TileType.Up || n1 == TileType.UpLeft || n1 == TileType.InDownLeft) &&
                                    (n2 == TileType.Up || n2 == TileType.UpRight || n2 == TileType.InDownRight) &&
                                    GetAutoMir2TileType(i, j - 2) < 0)
                                {
                                    PutAutoTile(i, j, RandomAutoMir2Tile(TileType.Up));
                                }
                                if (GetAutoMir2TileType(i, j) == TileType.UpRight &&
                                    ((GetAutoMir2TileType(i, j + 2) == TileType.Center &&
                                      GetAutoMir2TileType(i + 2, j + 2) == TileType.Center) ||
                                     GetAutoMir2TileType(i + 2, j) == TileType.UpLeft ||
                                     GetAutoMir2TileType(i + 2, j) == TileType.Left))
                                {
                                    PutAutoTile(i, j, RandomAutoMir2Tile(TileType.Up));
                                }
                                if (GetAutoMir2TileType(i, j) == TileType.UpLeft &&
                                    ((GetAutoMir2TileType(i, j + 2) == TileType.Center &&
                                      GetAutoMir2TileType(i - 2, j + 2) == TileType.Center) ||
                                     GetAutoMir2TileType(i - 2, j) == TileType.UpRight ||
                                     GetAutoMir2TileType(i - 2, j) == TileType.Right))
                                {
                                    PutAutoTile(i, j, RandomAutoMir2Tile(TileType.Up));
                                }
                            }

                            //Check RIGHT
                            if (GetAutoMir2TileType(i, j) != TileType.Right)
                            {
                                n1 = GetAutoMir2TileType(i, j - 2);
                                n2 = GetAutoMir2TileType(i, j + 2);
                                if ((n1 == TileType.Right || n1 == TileType.UpRight || n1 == TileType.InUpLeft) &&
                                    (n2 == TileType.Right || n2 == TileType.DownRight || n2 == TileType.InDownLeft) &&
                                    GetAutoMir2TileType(i + 2, j) < 0)
                                {
                                    PutAutoTile(i, j, RandomAutoMir2Tile(TileType.Right));
                                }
                                if (GetAutoMir2TileType(i, j) == TileType.DownRight &&
                                    ((GetAutoMir2TileType(i - 2, j) == TileType.Center &&
                                      GetAutoMir2TileType(i - 2, j + 2) == TileType.Center) ||
                                     GetAutoMir2TileType(i, j + 2) == TileType.UpRight ||
                                     GetAutoMir2TileType(i, j + 2) == TileType.Up))
                                {
                                    PutAutoTile(i, j, RandomAutoMir2Tile(TileType.Right));
                                }
                                if (GetAutoMir2TileType(i, j) == TileType.UpRight &&
                                    ((GetAutoMir2TileType(i - 2, j) == TileType.Center &&
                                      GetAutoMir2TileType(i - 2, j - 2) == TileType.Center) ||
                                     GetAutoMir2TileType(i, j - 2) == TileType.DownRight ||
                                     GetAutoMir2TileType(i, j - 2) == TileType.Down))
                                {
                                    PutAutoTile(i, j, RandomAutoMir2Tile(TileType.Right));
                                }
                            }

                            //Check DOWN
                            if (GetAutoMir2TileType(i, j) != TileType.Down)
                            {
                                n1 = GetAutoMir2TileType(i - 2, j);
                                n2 = GetAutoMir2TileType(i + 2, j);
                                if ((n1 == TileType.Down || n1 == TileType.DownLeft || n1 == TileType.InUpLeft) &&
                                    (n2 == TileType.Down || n2 == TileType.DownRight || n2 == TileType.InUpRight) &&
                                    GetAutoMir2TileType(i, j + 2) < 0)
                                {
                                    PutAutoTile(i, j, RandomAutoMir2Tile(TileType.Down));
                                }
                                if (GetAutoMir2TileType(i, j) == TileType.DownRight &&
                                    ((GetAutoMir2TileType(i, j - 2) == TileType.Center &&
                                      GetAutoMir2TileType(i + 2, j - 2) == TileType.Center) ||
                                     GetAutoMir2TileType(i + 2, j) == TileType.DownLeft ||
                                     GetAutoMir2TileType(i + 2, j) == TileType.Left))
                                {
                                    PutAutoTile(i, j, RandomAutoMir2Tile(TileType.Down));
                                }
                                if (GetAutoMir2TileType(i, j) == TileType.DownLeft &&
                                    ((GetAutoMir2TileType(i - 2, j - 2) == TileType.Center &&
                                      GetAutoMir2TileType(i, j - 2) == TileType.Center) ||
                                     GetAutoMir2TileType(i - 2, j) == TileType.DownRight ||
                                     GetAutoMir2TileType(i - 2, j) == TileType.Right))
                                {
                                    PutAutoTile(i, j, RandomAutoMir2Tile(TileType.Down));
                                }
                            }

                            //Check LEFT
                            if (GetAutoMir2TileType(i, j) != TileType.Left)
                            {
                                n1 = GetAutoMir2TileType(i, j - 2);
                                n2 = GetAutoMir2TileType(i, j + 2);
                                if ((n1 == TileType.Left || n1 == TileType.UpLeft || n1 == TileType.InUpRight) &&
                                    (n2 == TileType.Left || n2 == TileType.DownLeft || n2 == TileType.InDownRight) &&
                                    GetAutoMir2TileType(i - 2, j) < 0)
                                {
                                    PutAutoTile(i, j, RandomAutoMir2Tile(TileType.Left));
                                }
                                if (GetAutoMir2TileType(i, j) == TileType.DownLeft &&
                                    ((GetAutoMir2TileType(i + 2, j) == TileType.Center &&
                                      GetAutoMir2TileType(i + 2, j + 2) == TileType.Center) ||
                                     GetAutoMir2TileType(i, j + 2) == TileType.UpLeft ||
                                     GetAutoMir2TileType(i, j + 2) == TileType.Up))
                                {
                                    PutAutoTile(i, j, RandomAutoMir2Tile(TileType.Left));
                                }
                                if (GetAutoMir2TileType(i, j) == TileType.UpLeft &&
                                    ((GetAutoMir2TileType(i + 2, j) == TileType.Center &&
                                      GetAutoMir2TileType(i + 2, j + 2) == TileType.Center) ||
                                     GetAutoMir2TileType(i, j - 2) == TileType.DownLeft ||
                                     GetAutoMir2TileType(i, j - 2) == TileType.Down))
                                {
                                    PutAutoTile(i, j, RandomAutoMir2Tile(TileType.Left));
                                }
                            }

                            //Check INUPRIGHT
                            n1 = GetAutoMir2TileType(i - 2, j);
                            n2 = GetAutoMir2TileType(i, j + 2);
                            if (GetAutoMir2TileType(i, j) != TileType.InDownLeft)
                            {
                                if ((n1 == TileType.DownLeft || n1 == TileType.Down || n1 == TileType.InUpLeft) &&
                                    (n2 == TileType.DownLeft || n2 == TileType.Left || n2 == TileType.InDownRight))
                                {
                                    //PutAutoTile(i, j, RandomAutoMir2Tile(TileType.InUpRight));
                                    PutAutoTile(i, j, RandomAutoMir2Tile(TileType.InDownLeft));
                                }
                            }

                            //Check INDOWNRIGHT
                            n1 = GetAutoMir2TileType(i, j - 2);
                            n2 = GetAutoMir2TileType(i - 2, j);
                            if (GetAutoMir2TileType(i, j) != TileType.InUpLeft)
                            {
                                if ((n1 == TileType.UpLeft || n1 == TileType.Left || n1 == TileType.InUpRight) &&
                                    (n2 == TileType.UpLeft || n2 == TileType.Up || n2 == TileType.InDownLeft))
                                {
                                    //PutAutoTile(i, j, RandomAutoMir2Tile(TileType.InDownRight));
                                    PutAutoTile(i, j, RandomAutoMir2Tile(TileType.InUpLeft));
                                }
                            }

                            //Check INDOWNLEFT
                            n1 = GetAutoMir2TileType(i, j - 2);
                            n2 = GetAutoMir2TileType(i + 2, j);
                            if (GetAutoMir2TileType(i, j) != TileType.InUpRight)
                            {
                                if ((n1 == TileType.UpRight || n1 == TileType.Right || n1 == TileType.InUpLeft) &&
                                    (n2 == TileType.UpRight || n2 == TileType.Up || n2 == TileType.InDownRight))
                                {
                                    //PutAutoTile(i, j, RandomAutoMir2Tile(TileType.InDownLeft));
                                    PutAutoTile(i, j, RandomAutoMir2Tile(TileType.InUpRight));
                                }
                            }

                            //Check INUPLEFT
                            n1 = GetAutoMir2TileType(i + 2, j);
                            n2 = GetAutoMir2TileType(i, j + 2);
                            if (GetAutoMir2TileType(i, j) != TileType.InDownRight)
                            {
                                if ((n1 == TileType.DownRight || n1 == TileType.Down || n1 == TileType.InUpRight) &&
                                    (n2 == TileType.DownRight || n2 == TileType.Right || n2 == TileType.InDownLeft))
                                {
                                    //PutAutoTile(i, j, RandomAutoMir2Tile(TileType.InUpLeft));
                                    PutAutoTile(i, j, RandomAutoMir2Tile(TileType.InDownRight));
                                }
                            }

                            //Check Paradox
                            if ((GetAutoMir2TileType(i - 2, j) == TileType.Down &&
                                 GetAutoMir2TileType(i, j - 2) == TileType.Right &&
                                 GetAutoMir2TileType(i + 2, j) == TileType.Up &&
                                 GetAutoMir2TileType(i, j + 2) == TileType.Left) ||
                                (GetAutoMir2TileType(i - 2, j) == TileType.Up &&
                                 GetAutoMir2TileType(i, j + 2) == TileType.Right &&
                                 GetAutoMir2TileType(i, j - 2) == TileType.Left &&
                                 GetAutoMir2TileType(i + 2, j) == TileType.Down))
                            {
                                PutAutoTile(i, j, RandomAutoMir2Tile(TileType.Center));
                                DrawAutoMir2TileSide(i, j);
                            }
                        }
                    }
                }
            }
        }

        private void DrawAutoMir3TilePattern(int iX, int iY)
        {
            int i, j, c;
            TileType n1, n2;

            for (j = iY - AutoTileRange; j <= iY + AutoTileRange; j += 2) //间隔为2
            {
                for (i = iX - AutoTileRange; i <= iX + AutoTileRange; i += 2) //间隔为2, 其他算法跟小地砖一样
                {
                    if (i > 1 && j > 1)
                    {
                        if (GetAutoMir3TileType(i, j) > 0)
                        {
                            //Check CENTER
                            if (GetAutoMir3TileType(i, j) != TileType.Center)
                            {
                                c = 0;
                                if (GetAutoMir3TileType(i, j - 2) >= 0)
                                {
                                    ++c;
                                }
                                if (GetAutoMir3TileType(i + 2, j - 2) >= 0)
                                {
                                    ++c;
                                }
                                if (GetAutoMir3TileType(i + 2, j) >= 0)
                                {
                                    ++c;
                                }
                                if (GetAutoMir3TileType(i + 2, j + 2) >= 0)
                                {
                                    ++c;
                                }
                                if (GetAutoMir3TileType(i, j + 2) >= 0)
                                {
                                    ++c;
                                }
                                if (GetAutoMir3TileType(i - 2, j + 2) >= 0)
                                {
                                    ++c;
                                }
                                if (GetAutoMir3TileType(i - 2, j) >= 0)
                                {
                                    ++c;
                                }
                                if (GetAutoMir3TileType(i - 2, j - 2) >= 0)
                                {
                                    ++c;
                                }
                                if (c >= 8)
                                {
                                    PutAutoTile(i, j, RandomAutoMir3Tile(TileType.Center));
                                }
                            }

                            //Check UP
                            if (GetAutoMir3TileType(i, j) != TileType.Up)
                            {
                                n1 = GetAutoMir3TileType(i - 2, j);
                                n2 = GetAutoMir3TileType(i + 2, j);
                                if ((n1 == TileType.Up || n1 == TileType.UpLeft || n1 == TileType.InDownLeft) &&
                                    (n2 == TileType.Up || n2 == TileType.UpRight || n2 == TileType.InDownRight) &&
                                    GetAutoMir3TileType(i, j - 2) < 0)
                                {
                                    PutAutoTile(i, j, RandomAutoMir3Tile(TileType.Up));
                                }
                                if (GetAutoMir3TileType(i, j) == TileType.UpRight &&
                                    ((GetAutoMir3TileType(i, j + 2) == TileType.Center &&
                                      GetAutoMir3TileType(i + 2, j + 2) == TileType.Center) ||
                                     GetAutoMir3TileType(i + 2, j) == TileType.UpLeft ||
                                     GetAutoMir3TileType(i + 2, j) == TileType.Left))
                                {
                                    PutAutoTile(i, j, RandomAutoMir3Tile(TileType.Up));
                                }
                                if (GetAutoMir3TileType(i, j) == TileType.UpLeft &&
                                    ((GetAutoMir3TileType(i, j + 2) == TileType.Center &&
                                      GetAutoMir3TileType(i - 2, j + 2) == TileType.Center) ||
                                     GetAutoMir3TileType(i - 2, j) == TileType.UpRight ||
                                     GetAutoMir3TileType(i - 2, j) == TileType.Right))
                                {
                                    PutAutoTile(i, j, RandomAutoMir3Tile(TileType.Up));
                                }
                            }

                            //Check RIGHT
                            if (GetAutoMir3TileType(i, j) != TileType.Right)
                            {
                                n1 = GetAutoMir3TileType(i, j - 2);
                                n2 = GetAutoMir3TileType(i, j + 2);
                                if ((n1 == TileType.Right || n1 == TileType.UpRight || n1 == TileType.InUpLeft) &&
                                    (n2 == TileType.Right || n2 == TileType.DownRight || n2 == TileType.InDownLeft) &&
                                    GetAutoMir3TileType(i + 2, j) < 0)
                                {
                                    PutAutoTile(i, j, RandomAutoMir3Tile(TileType.Right));
                                }
                                if (GetAutoMir3TileType(i, j) == TileType.DownRight &&
                                    ((GetAutoMir3TileType(i - 2, j) == TileType.Center &&
                                      GetAutoMir3TileType(i - 2, j + 2) == TileType.Center) ||
                                     GetAutoMir3TileType(i, j + 2) == TileType.UpRight ||
                                     GetAutoMir3TileType(i, j + 2) == TileType.Up))
                                {
                                    PutAutoTile(i, j, RandomAutoMir3Tile(TileType.Right));
                                }
                                if (GetAutoMir3TileType(i, j) == TileType.UpRight &&
                                    ((GetAutoMir3TileType(i - 2, j) == TileType.Center &&
                                      GetAutoMir3TileType(i - 2, j - 2) == TileType.Center) ||
                                     GetAutoMir3TileType(i, j - 2) == TileType.DownRight ||
                                     GetAutoMir3TileType(i, j - 2) == TileType.Down))
                                {
                                    PutAutoTile(i, j, RandomAutoMir3Tile(TileType.Right));
                                }
                            }

                            //Check DOWN
                            if (GetAutoMir3TileType(i, j) != TileType.Down)
                            {
                                n1 = GetAutoMir3TileType(i - 2, j);
                                n2 = GetAutoMir3TileType(i + 2, j);
                                if ((n1 == TileType.Down || n1 == TileType.DownLeft || n1 == TileType.InUpLeft) &&
                                    (n2 == TileType.Down || n2 == TileType.DownRight || n2 == TileType.InUpRight) &&
                                    GetAutoMir3TileType(i, j + 2) < 0)
                                {
                                    PutAutoTile(i, j, RandomAutoMir3Tile(TileType.Down));
                                }
                                if (GetAutoMir3TileType(i, j) == TileType.DownRight &&
                                    ((GetAutoMir3TileType(i, j - 2) == TileType.Center &&
                                      GetAutoMir3TileType(i + 2, j - 2) == TileType.Center) ||
                                     GetAutoMir3TileType(i + 2, j) == TileType.DownLeft ||
                                     GetAutoMir3TileType(i + 2, j) == TileType.Left))
                                {
                                    PutAutoTile(i, j, RandomAutoMir3Tile(TileType.Down));
                                }
                                if (GetAutoMir3TileType(i, j) == TileType.DownLeft &&
                                    ((GetAutoMir3TileType(i - 2, j - 2) == TileType.Center &&
                                      GetAutoMir3TileType(i, j - 2) == TileType.Center) ||
                                     GetAutoMir3TileType(i - 2, j) == TileType.DownRight ||
                                     GetAutoMir3TileType(i - 2, j) == TileType.Right))
                                {
                                    PutAutoTile(i, j, RandomAutoMir3Tile(TileType.Down));
                                }
                            }

                            //Check LEFT
                            if (GetAutoMir3TileType(i, j) != TileType.Left)
                            {
                                n1 = GetAutoMir3TileType(i, j - 2);
                                n2 = GetAutoMir3TileType(i, j + 2);
                                if ((n1 == TileType.Left || n1 == TileType.UpLeft || n1 == TileType.InUpRight) &&
                                    (n2 == TileType.Left || n2 == TileType.DownLeft || n2 == TileType.InDownRight) &&
                                    GetAutoMir3TileType(i - 2, j) < 0)
                                {
                                    PutAutoTile(i, j, RandomAutoMir3Tile(TileType.Left));
                                }
                                if (GetAutoMir3TileType(i, j) == TileType.DownLeft &&
                                    ((GetAutoMir3TileType(i + 2, j) == TileType.Center &&
                                      GetAutoMir3TileType(i + 2, j + 2) == TileType.Center) ||
                                     GetAutoMir3TileType(i, j + 2) == TileType.UpLeft ||
                                     GetAutoMir3TileType(i, j + 2) == TileType.Up))
                                {
                                    PutAutoTile(i, j, RandomAutoMir3Tile(TileType.Left));
                                }
                                if (GetAutoMir3TileType(i, j) == TileType.UpLeft &&
                                    ((GetAutoMir3TileType(i + 2, j) == TileType.Center &&
                                      GetAutoMir3TileType(i + 2, j + 2) == TileType.Center) ||
                                     GetAutoMir3TileType(i, j - 2) == TileType.DownLeft ||
                                     GetAutoMir3TileType(i, j - 2) == TileType.Down))
                                {
                                    PutAutoTile(i, j, RandomAutoMir3Tile(TileType.Left));
                                }
                            }

                            //Check INUPRIGHT
                            n1 = GetAutoMir3TileType(i - 2, j);
                            n2 = GetAutoMir3TileType(i, j + 2);
                            if (GetAutoMir3TileType(i, j) != TileType.InDownLeft)
                            {
                                if ((n1 == TileType.DownLeft || n1 == TileType.Down || n1 == TileType.InUpLeft) &&
                                    (n2 == TileType.DownLeft || n2 == TileType.Left || n2 == TileType.InDownRight))
                                {
                                    //PutAutoTile(i, j, RandomAutoMir3Tile(TileType.InUpRight));
                                    PutAutoTile(i, j, RandomAutoMir3Tile(TileType.InDownLeft));
                                }
                            }

                            //Check INDOWNRIGHT
                            n1 = GetAutoMir3TileType(i, j - 2);
                            n2 = GetAutoMir3TileType(i - 2, j);
                            if (GetAutoMir3TileType(i, j) != TileType.InUpLeft)
                            {
                                if ((n1 == TileType.UpLeft || n1 == TileType.Left || n1 == TileType.InUpRight) &&
                                    (n2 == TileType.UpLeft || n2 == TileType.Up || n2 == TileType.InDownLeft))
                                {
                                    //PutAutoTile(i, j, RandomAutoMir3Tile(TileType.InDownRight));
                                    PutAutoTile(i, j, RandomAutoMir3Tile(TileType.InUpLeft));
                                }
                            }

                            //Check INDOWNLEFT
                            n1 = GetAutoMir3TileType(i, j - 2);
                            n2 = GetAutoMir3TileType(i + 2, j);
                            if (GetAutoMir3TileType(i, j) != TileType.InUpRight)
                            {
                                if ((n1 == TileType.UpRight || n1 == TileType.Right || n1 == TileType.InUpLeft) &&
                                    (n2 == TileType.UpRight || n2 == TileType.Up || n2 == TileType.InDownRight))
                                {
                                    //PutAutoTile(i, j, RandomAutoMir3Tile(TileType.InDownLeft));
                                    PutAutoTile(i, j, RandomAutoMir3Tile(TileType.InUpRight));
                                }
                            }

                            //Check INUPLEFT
                            n1 = GetAutoMir3TileType(i + 2, j);
                            n2 = GetAutoMir3TileType(i, j + 2);
                            if (GetAutoMir3TileType(i, j) != TileType.InDownRight)
                            {
                                if ((n1 == TileType.DownRight || n1 == TileType.Down || n1 == TileType.InUpRight) &&
                                    (n2 == TileType.DownRight || n2 == TileType.Right || n2 == TileType.InDownLeft))
                                {
                                    //PutAutoTile(i, j, RandomAutoMir3Tile(TileType.InUpLeft));
                                    PutAutoTile(i, j, RandomAutoMir3Tile(TileType.InDownRight));
                                }
                            }

                            //Check Paradox
                            if ((GetAutoMir3TileType(i - 2, j) == TileType.Down &&
                                 GetAutoMir3TileType(i, j - 2) == TileType.Right &&
                                 GetAutoMir3TileType(i + 2, j) == TileType.Up &&
                                 GetAutoMir3TileType(i, j + 2) == TileType.Left) ||
                                (GetAutoMir3TileType(i - 2, j) == TileType.Up &&
                                 GetAutoMir3TileType(i, j + 2) == TileType.Right &&
                                 GetAutoMir3TileType(i, j - 2) == TileType.Left &&
                                 GetAutoMir3TileType(i + 2, j) == TileType.Down))
                            {
                                PutAutoTile(i, j, RandomAutoMir3Tile(TileType.Center));
                                DrawAutoMir3TileSide(i, j);
                            }
                        }
                    }
                }
            }
        }

        private void DrawAutoSmTilePattern(int iX, int iY)
        {
            //这个算法就比较复杂了。。他是通过检查周边的地砖的类型来自动化绘制的, 根据已经绘制的地砖来动态调整和增加需要绘制的地砖, 从而达到自动绘制的目的
            int i, j, c;
            TileType n1, n2;
            for (j = iY - AutoTileRange; j <= iY + AutoTileRange; ++j)
            {
                for (i = iX - AutoTileRange; i <= iX + AutoTileRange; ++i)
                    //根据当前鼠标所指格子的周边m_iAutoTileRange范围的格子开始检查调整
                {
                    if (i > 0 && j > 0) //首先去确保检查的格子的合法性
                    {
                        if (GetAutoSmTileType(i, j) > 0) //然后获取格子的小地砖类型, 是否是当前样式的格子, 如果是则需要检查调整
                        {
                            //Check CENTER
                            if (GetAutoSmTileType(i, j) != TileType.Center) //首先检查是否需要调整为中间类型的格子
                            {
                                c = 0;
                                if (GetAutoSmTileType(i, j - 1) >= 0)
                                {
                                    ++c;
                                }
                                if (GetAutoSmTileType(i + 1, j - 1) >= 0)
                                {
                                    ++c;
                                }
                                if (GetAutoSmTileType(i + 1, j) >= 0)
                                {
                                    ++c;
                                }
                                if (GetAutoSmTileType(i + 1, j + 1) >= 0)
                                {
                                    ++c;
                                }
                                if (GetAutoSmTileType(i, j + 1) >= 0)
                                {
                                    ++c;
                                }
                                if (GetAutoSmTileType(i - 1, j + 1) >= 0)
                                {
                                    ++c;
                                }
                                if (GetAutoSmTileType(i - 1, j) >= 0)
                                {
                                    ++c;
                                }
                                if (GetAutoSmTileType(i - 1, j - 1) >= 0)
                                {
                                    ++c;
                                }
                                if (c >= 8) //只要是被8个格子包围的格子就要被调整为中间类型的格子
                                {
                                    PutAutoSmTile(i, j, RandomAutoSmTile(TileType.Center));
                                }
                            }

                            //Check UP
                            if (GetAutoSmTileType(i, j) != TileType.Up) //然后检查是否需要调整为上类型的格子。。
                            {
                                //下面的算法有3种情况需要调整为上的格子。。。大家可以自己看看。。
                                n1 = GetAutoSmTileType(i - 1, j);
                                n2 = GetAutoSmTileType(i + 1, j);
                                if ((n1 == TileType.Up || n1 == TileType.UpLeft || n1 == TileType.InDownLeft) &&
                                    (n2 == TileType.Up || n2 == TileType.UpRight || n2 == TileType.InDownRight) &&
                                    GetAutoSmTileType(i, j - 1) < 0)
                                {
                                    PutAutoSmTile(i, j, RandomAutoSmTile(TileType.Up));
                                }
                                if (GetAutoSmTileType(i, j) == TileType.UpRight &&
                                    ((GetAutoSmTileType(i, j + 1) == TileType.Center &&
                                      GetAutoSmTileType(i + 1, j + 1) == TileType.Center) ||
                                     GetAutoSmTileType(i + 1, j) == TileType.UpLeft ||
                                     GetAutoSmTileType(i + 1, j) == TileType.Left))
                                {
                                    PutAutoSmTile(i, j, RandomAutoSmTile(TileType.Up));
                                }
                                if (GetAutoSmTileType(i, j) == TileType.UpLeft &&
                                    ((GetAutoSmTileType(i, j + 1) == TileType.Center &&
                                      GetAutoSmTileType(i - 1, j + 1) == TileType.Center) ||
                                     GetAutoSmTileType(i - 1, j) == TileType.UpRight ||
                                     GetAutoSmTileType(i - 1, j) == TileType.Right))
                                {
                                    PutAutoSmTile(i, j, RandomAutoSmTile(TileType.Up));
                                }
                            }

                            //Check RIGHT  //然后检查是否需要调整为右类型的格子。。
                            if (GetAutoSmTileType(i, j) != TileType.Right)
                            {
                                n1 = GetAutoSmTileType(i, j - 1);
                                n2 = GetAutoSmTileType(i, j + 1);
                                if ((n1 == TileType.Right || n1 == TileType.UpRight || n1 == TileType.InUpLeft) &&
                                    (n2 == TileType.Right || n2 == TileType.DownRight || n2 == TileType.InDownLeft) &&
                                    GetAutoSmTileType(i + 1, j) < 0)
                                {
                                    PutAutoSmTile(i, j, RandomAutoSmTile(TileType.Right));
                                }
                                if (GetAutoSmTileType(i, j) == TileType.DownRight &&
                                    ((GetAutoSmTileType(i - 1, j) == TileType.Center &&
                                      GetAutoSmTileType(i - 1, j + 1) == TileType.Center) ||
                                     GetAutoSmTileType(i, j + 1) == TileType.UpRight ||
                                     GetAutoSmTileType(i, j + 1) == TileType.Up))
                                {
                                    PutAutoSmTile(i, j, RandomAutoSmTile(TileType.Right));
                                }
                                if (GetAutoSmTileType(i, j) == TileType.UpRight &&
                                    ((GetAutoSmTileType(i - 1, j) == TileType.Center &&
                                      GetAutoSmTileType(i - 1, j - 1) == TileType.Center) ||
                                     GetAutoSmTileType(i, j - 1) == TileType.DownRight ||
                                     GetAutoSmTileType(i, j - 1) == TileType.Down))
                                {
                                    PutAutoSmTile(i, j, RandomAutoSmTile(TileType.Right));
                                }
                            }

                            //Check DOWN //然后检查是否需要调整为下类型的格子。。
                            if (GetAutoSmTileType(i, j) != TileType.Down)
                            {
                                n1 = GetAutoSmTileType(i - 1, j);
                                n2 = GetAutoSmTileType(i + 1, j);
                                if ((n1 == TileType.Down || n1 == TileType.DownLeft || n1 == TileType.InUpLeft) &&
                                    (n2 == TileType.Down || n2 == TileType.DownRight || n2 == TileType.InUpRight) &&
                                    GetAutoSmTileType(i, j + 1) < 0)
                                {
                                    PutAutoSmTile(i, j, RandomAutoSmTile(TileType.Down));
                                }
                                if (GetAutoSmTileType(i, j) == TileType.DownRight &&
                                    ((GetAutoSmTileType(i, j - 1) == TileType.Center &&
                                      GetAutoSmTileType(i + 1, j - 1) == TileType.Center) ||
                                     GetAutoSmTileType(i + 1, j) == TileType.DownLeft ||
                                     GetAutoSmTileType(i + 1, j) == TileType.Left))
                                {
                                    PutAutoSmTile(i, j, RandomAutoSmTile(TileType.Down));
                                }
                                if (GetAutoSmTileType(i, j) == TileType.DownLeft &&
                                    ((GetAutoSmTileType(i - 1, j - 1) == TileType.Center &&
                                      GetAutoSmTileType(i, j - 1) == TileType.Center) ||
                                     GetAutoSmTileType(i - 1, j) == TileType.DownRight ||
                                     GetAutoSmTileType(i - 1, j) == TileType.Right))
                                {
                                    PutAutoSmTile(i, j, RandomAutoSmTile(TileType.Down));
                                }
                            }

                            //Check LEFT  //然后检查是否需要调整为左类型的格子。。
                            if (GetAutoSmTileType(i, j) != TileType.Left)
                            {
                                n1 = GetAutoSmTileType(i, j - 1);
                                n2 = GetAutoSmTileType(i, j + 1);
                                if ((n1 == TileType.Left || n1 == TileType.UpLeft || n1 == TileType.InUpRight) &&
                                    (n2 == TileType.Left || n2 == TileType.DownLeft || n2 == TileType.InDownRight) &&
                                    GetAutoSmTileType(i - 1, j) < 0)
                                {
                                    PutAutoSmTile(i, j, RandomAutoSmTile(TileType.Left));
                                }
                                if (GetAutoSmTileType(i, j) == TileType.DownLeft &&
                                    ((GetAutoSmTileType(i + 1, j) == TileType.Center &&
                                      GetAutoSmTileType(i + 1, j + 1) == TileType.Center) ||
                                     GetAutoSmTileType(i, j + 1) == TileType.UpLeft ||
                                     GetAutoSmTileType(i, j + 1) == TileType.Up))
                                {
                                    PutAutoSmTile(i, j, RandomAutoSmTile(TileType.Left));
                                }
                                if (GetAutoSmTileType(i, j) == TileType.UpLeft &&
                                    ((GetAutoSmTileType(i + 1, j) == TileType.Center &&
                                      GetAutoSmTileType(i + 1, j + 1) == TileType.Center) ||
                                     GetAutoSmTileType(i, j - 1) == TileType.DownLeft ||
                                     GetAutoSmTileType(i, j - 1) == TileType.Down))
                                {
                                    PutAutoSmTile(i, j, RandomAutoSmTile(TileType.Left));
                                }
                            }

                            //Check INUPRIGHT  //然后检查是否需要调整为内右上类型的格子。。
                            if (GetAutoSmTileType(i, j) != TileType.InUpRight)
                            {
                                n1 = GetAutoSmTileType(i - 1, j);
                                n2 = GetAutoSmTileType(i, j + 1);
                                if ((n1 == TileType.DownLeft || n1 == TileType.Down || n1 == TileType.InUpLeft) &&
                                    (n2 == TileType.DownLeft || n2 == TileType.Left || n2 == TileType.InDownRight))
                                {
                                    PutAutoSmTile(i, j, RandomAutoSmTile(TileType.InUpRight));
                                }
                            }

                            //Check INDOWNRIGHT //然后检查是否需要调整为内右下类型的格子。。
                            if (GetAutoSmTileType(i, j) != TileType.InDownRight)
                            {
                                n1 = GetAutoSmTileType(i, j - 1);
                                n2 = GetAutoSmTileType(i - 1, j);
                                if ((n1 == TileType.UpLeft || n1 == TileType.Left || n1 == TileType.InUpRight) &&
                                    (n2 == TileType.UpLeft || n2 == TileType.Up || n2 == TileType.InDownLeft))
                                {
                                    PutAutoSmTile(i, j, RandomAutoSmTile(TileType.InDownRight));
                                }
                            }

                            //Check INDOWNLEFT  //然后检查是否需要调整为内左下类型的格子。。
                            if (GetAutoSmTileType(i, j) != TileType.InDownLeft)
                            {
                                n1 = GetAutoSmTileType(i, j - 1);
                                n2 = GetAutoSmTileType(i + 1, j);
                                if ((n1 == TileType.UpRight || n1 == TileType.Right || n1 == TileType.InUpLeft) &&
                                    (n2 == TileType.UpRight || n2 == TileType.Up || n2 == TileType.InDownRight))
                                {
                                    PutAutoSmTile(i, j, RandomAutoSmTile(TileType.InDownLeft));
                                }
                            }

                            //Check INUPLEFT //然后检查是否需要调整为内左上类型的格子。。
                            if (GetAutoSmTileType(i, j) != TileType.InUpLeft)
                            {
                                n1 = GetAutoSmTileType(i + 1, j);
                                n2 = GetAutoSmTileType(i, j + 1);
                                if ((n1 == TileType.DownRight || n1 == TileType.Down || n1 == TileType.InUpRight) &&
                                    (n2 == TileType.DownRight || n2 == TileType.Right || n2 == TileType.InDownLeft))
                                {
                                    PutAutoSmTile(i, j, RandomAutoSmTile(TileType.InUpLeft));
                                }
                            }

                            //四个外角是不用检查的哦。。。

                            //Check Paradox //最后检查完后看看是否出现了矛盾的地方, 假如出现了, 可能就需要增加地砖来调和这种矛盾, 所以在这个矛盾的地方的周边绘制一个样式
                            if ((GetAutoSmTileType(i - 1, j) == TileType.Down &&
                                 GetAutoSmTileType(i, j - 1) == TileType.Right &&
                                 GetAutoSmTileType(i + 1, j) == TileType.Up &&
                                 GetAutoSmTileType(i, j + 1) == TileType.Left) ||
                                (GetAutoSmTileType(i - 1, j) == TileType.Up &&
                                 GetAutoSmTileType(i, j + 1) == TileType.Right &&
                                 GetAutoSmTileType(i, j - 1) == TileType.Left &&
                                 GetAutoSmTileType(i + 1, j) == TileType.Down))
                            {
                                PutAutoSmTile(i, j, RandomAutoSmTile(TileType.Center));
                                DrawAutoSmTileSide(i, j);
                            }
                        }
                    }
                }
            } //最后检查完一趟, 假如发现没有地砖需要变化调整, 则说明调整完毕了。。算法比较复杂, 大家可以慢慢看代码搞清楚, 调整地砖也不止这一种算法, 大家有兴趣的可以想想自己的一些算法来绘制自动化的样式
        }


        private Bitmap GetTilesPreview(ListItem selectListView, int index)
        {
            var preview = new Bitmap(6*CellWidth, 6*CellHeight);
            var graphics = Graphics.FromImage(preview);
            graphics.InterpolationMode = InterpolationMode.Low;
            switch (selectListItem.Version)
            {
                case (byte) MirVerSion.WemadeMir2:
                case (byte) MirVerSion.ShandaMir2:
                    if ((selectListView.Text.IndexOf("SmTiles", StringComparison.Ordinal) > -1) ||
                        (selectListView.Text.IndexOf("Smtiles", StringComparison.Ordinal) > -1))
                    {
                        var i = 0;
                        preview = new Bitmap(3*CellWidth, 3*CellHeight);
                        graphics = Graphics.FromImage(preview);
                        graphics.InterpolationMode = InterpolationMode.Low;
                        for (var y = 0; y < 3; y++)
                        {
                            drawY = y*CellHeight;
                            for (var x = 0; x < 3; x++)
                            {
                                drawX = x*CellWidth;
                                if (index*smTileBlock + smTilesPreviewIndex[i] >=
                                    Libraries.MapLibs[selectListView.Value].Images.Count)
                                {
                                    continue;
                                }
                                Libraries.MapLibs[selectListView.Value].CheckImage(index*smTileBlock +
                                                                                   smTilesPreviewIndex[i]);
                                var mi =
                                    Libraries.MapLibs[selectListView.Value].Images[
                                        index*smTileBlock + smTilesPreviewIndex[i]];
                                if (mi.Image == null)
                                {
                                    continue;
                                }
                                var destRect = new Rectangle(drawX, drawY, mi.Width, mi.Height);
                                var srcRect = new Rectangle(0, 0, mi.Width, mi.Height);
                                graphics.DrawImage(mi.Image, destRect, srcRect, GraphicsUnit.Pixel);
                                Libraries.MapLibs[libIndex].Images[index] = null;
                                i++;
                            }
                        }
                    }
                    else if (selectListView.Text.IndexOf("Tiles", StringComparison.Ordinal) > -1)
                    {
                        var i = 0;
                        preview = new Bitmap(6*CellWidth, 6*CellHeight);
                        graphics = Graphics.FromImage(preview);
                        graphics.InterpolationMode = InterpolationMode.Low;
                        for (var y = 0; y < 3; y++)
                        {
                            drawY = y*2*CellHeight;
                            for (var x = 0; x < 3; x++)
                            {
                                drawX = x*2*CellWidth;
                                if (index*Mir2BigTileBlock + Mir2BigTilesPreviewIndex[i] >=
                                    Libraries.MapLibs[selectListView.Value].Images.Count)
                                {
                                    continue;
                                }
                                Libraries.MapLibs[selectListView.Value].CheckImage(index*Mir2BigTileBlock +
                                                                                   Mir2BigTilesPreviewIndex[i]);
                                var mi =
                                    Libraries.MapLibs[selectListView.Value].Images[
                                        index*Mir2BigTileBlock + Mir2BigTilesPreviewIndex[i]];
                                if (mi.Image == null)
                                {
                                    continue;
                                }
                                var destRect = new Rectangle(drawX, drawY, mi.Width, mi.Height);
                                var srcRect = new Rectangle(0, 0, mi.Width, mi.Height);
                                graphics.DrawImage(mi.Image, destRect, srcRect, GraphicsUnit.Pixel);
                                Libraries.MapLibs[libIndex].Images[index] = null;
                                i++;
                            }
                        }
                    }
                    break;
                case (byte) MirVerSion.WemadeMir3:
                case (byte) MirVerSion.ShandaMir3:
                    if ((selectListView.Text.IndexOf("SmTiles", StringComparison.Ordinal) > -1) ||
                        (selectListView.Text.IndexOf("Smtiles", StringComparison.Ordinal) > -1))
                    {
                        var i = 0;
                        preview = new Bitmap(3*CellWidth, 3*CellHeight);
                        graphics = Graphics.FromImage(preview);
                        graphics.InterpolationMode = InterpolationMode.Low;

                        for (var y = 0; y < 3; y++)
                        {
                            drawY = y*CellHeight;
                            for (var x = 0; x < 3; x++)
                            {
                                drawX = x*CellWidth;
                                if (index*smTileBlock + smTilesPreviewIndex[i] >=
                                    Libraries.MapLibs[selectListView.Value].Images.Count)
                                {
                                    continue;
                                }
                                Libraries.MapLibs[selectListView.Value].CheckImage(index*smTileBlock +
                                                                                   smTilesPreviewIndex[i]);
                                var mi =
                                    Libraries.MapLibs[selectListView.Value].Images[
                                        index*smTileBlock + smTilesPreviewIndex[i]];
                                if (mi.Image == null)
                                {
                                    continue;
                                }
                                var destRect = new Rectangle(drawX, drawY, mi.Width, mi.Height);
                                var srcRect = new Rectangle(0, 0, mi.Width, mi.Height);
                                graphics.DrawImage(mi.Image, destRect, srcRect, GraphicsUnit.Pixel);
                                Libraries.MapLibs[libIndex].Images[index] = null;
                                i++;
                            }
                        }
                    }
                    else if (selectListView.Text.IndexOf("Tiles30", StringComparison.Ordinal) > -1)
                    {
                        var i = 0;
                        int flag;
                        if (Libraries.MapLibs[selectListView.Value].Images.Count%10 != 0)
                        {
                            flag = (Libraries.MapLibs[selectListView.Value].Images.Count + 1)/Mir3BigTileBlock;
                        }
                        else
                        {
                            flag = Libraries.MapLibs[selectListView.Value].Images.Count/Mir3BigTileBlock;
                        }
                        int[] bigTilesIndex;
                        int tempIndex;
                        preview = new Bitmap(6*CellWidth, 6*CellHeight);
                        graphics = Graphics.FromImage(preview);
                        graphics.InterpolationMode = InterpolationMode.Low;
                        if (index < flag)
                        {
                            bigTilesIndex = Mir3BigTilesPreviewIndex1;
                        }
                        else
                        {
                            bigTilesIndex = Mir3BigTilesPreviewIndex2;
                        }
                        if (index < flag)
                        {
                            tempIndex = index;
                        }
                        else
                        {
                            tempIndex = index - flag;
                        }
                        for (var y = 0; y < 3; y++)
                        {
                            drawY = y*2*CellHeight;
                            for (var x = 0; x < 3; x++)
                            {
                                drawX = x*2*CellWidth;
                                if (tempIndex*Mir3BigTileBlock + bigTilesIndex[i] >=
                                    Libraries.MapLibs[selectListView.Value].Images.Count)
                                {
                                    continue;
                                }
                                Libraries.MapLibs[selectListView.Value].CheckImage(tempIndex*Mir3BigTileBlock +
                                                                                   bigTilesIndex[i]);
                                var mi =
                                    Libraries.MapLibs[selectListView.Value].Images[
                                        tempIndex*Mir3BigTileBlock + bigTilesIndex[i]];
                                if (mi.Image == null)
                                {
                                    continue;
                                }
                                var destRect = new Rectangle(drawX, drawY, mi.Width, mi.Height);
                                var srcRect = new Rectangle(0, 0, mi.Width, mi.Height);
                                graphics.DrawImage(mi.Image, destRect, srcRect, GraphicsUnit.Pixel);
                                Libraries.MapLibs[libIndex].Images[tempIndex] = null;
                                i++;
                            }
                        }
                    }
                    break;
            }
            return preview;
        }

        private void TileslistView_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e)
        {
            int index;

            if (_tilesIndexList.TryGetValue(e.ItemIndex, out index))
            {
                e.Item = new ListViewItem {ImageIndex = index, Text = e.ItemIndex.ToString()};
                return;
            }

            _tilesIndexList.Add(e.ItemIndex, TilesImageList.Images.Count);

            TilesImageList.Images.Add(GetTilesPreview(selectListItem, e.ItemIndex));
            e.Item = new ListViewItem {ImageIndex = index, Text = e.ItemIndex.ToString()};
            Libraries.MapLibs[selectListItem.Value].Images[e.ItemIndex] = null;
        }

        private void TileslistView_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (TileslistView.SelectedIndices.Count > 0)
            {
                picTile.Image = GetTilesPreview(selectListItem, TileslistView.SelectedIndices[0]);
                selectTilesIndex = TileslistView.SelectedIndices[0];
            }
        }

        private int RandomAutoMir2Tile(TileType tileType)
        {
            if (selectTilesIndex < 0) return -1;

            switch (tileType)
            {
                case TileType.Center:
                    return selectTilesIndex*Mir2BigTileBlock + random.Next(5);
                case TileType.Up:
                    return selectTilesIndex*Mir2BigTileBlock + random.Next(15, 17);
                case TileType.Down:
                    return selectTilesIndex*Mir2BigTileBlock + random.Next(17, 19);
                case TileType.Left:
                    if (random.Next(2) == 0)
                    {
                        return selectTilesIndex*Mir2BigTileBlock + 20;
                    }
                    return selectTilesIndex*Mir2BigTileBlock + 22;
                case TileType.Right:
                    if (random.Next(2) == 0)
                    {
                        return selectTilesIndex*Mir2BigTileBlock + 21;
                    }
                    return selectTilesIndex*Mir2BigTileBlock + 23;
                case TileType.UpLeft:
                    return selectTilesIndex*Mir2BigTileBlock + 5;

                case TileType.UpRight:
                    return selectTilesIndex*Mir2BigTileBlock + 6;

                case TileType.DownLeft:
                    return selectTilesIndex*Mir2BigTileBlock + 7;
                case TileType.DownRight:
                    return selectTilesIndex*Mir2BigTileBlock + 8;

                case TileType.InUpLeft:
                    return selectTilesIndex*Mir2BigTileBlock + 10;
                case TileType.InUpRight:
                    return selectTilesIndex*Mir2BigTileBlock + 11;
                case TileType.InDownLeft:
                    return selectTilesIndex*Mir2BigTileBlock + 12;
                case TileType.InDownRight:
                    return selectTilesIndex*Mir2BigTileBlock + 13;
            }
            return -1;
        }

        private int RandomAutoMir3Tile(TileType tileType)
        {
            //传奇3 bigTile 30块 2 中组合

            //中  	0-4
            //左上	10
            //右上	11
            //左下	12	
            //右下	13
            //上	20，21
            //下	22，23
            //左	25，27
            //右	26，28
            //内左上 	15
            //内右上	16
            //内左下	17	
            //内右下	18

            //中	5-9
            //左上	18
            //右上	17
            //左下	16
            //右下	15
            //上	22，23
            //下	20，21	
            //左	26，28
            //右	25 ，27	
            //内左上 	13
            //内右上	12
            //内左下	11	
            //内右下	10
            if (selectTilesIndex < 0) return -1;
            int flag;
            if (Libraries.MapLibs[selectListItem.Value].Images.Count%10 != 0)
            {
                flag = (Libraries.MapLibs[selectListItem.Value].Images.Count + 1)/Mir3BigTileBlock;
            }
            else
            {
                flag = Libraries.MapLibs[selectListItem.Value].Images.Count/Mir3BigTileBlock;
            }

            if (selectTilesIndex < flag)
            {
                switch (tileType)
                {
                    case TileType.Center:
                        return selectTilesIndex*Mir3BigTileBlock + random.Next(5);
                    case TileType.Up:
                        return selectTilesIndex*Mir3BigTileBlock + random.Next(20, 22);
                    case TileType.Down:
                        return selectTilesIndex*Mir3BigTileBlock + random.Next(22, 24);
                    case TileType.Left:
                        if (random.Next(2) == 0)
                        {
                            return selectTilesIndex*Mir3BigTileBlock + 25;
                        }
                        return selectTilesIndex*Mir3BigTileBlock + 27;
                    case TileType.Right:
                        if (random.Next(2) == 0)
                        {
                            return selectTilesIndex*Mir3BigTileBlock + 26;
                        }
                        return selectTilesIndex*Mir3BigTileBlock + 28;
                    case TileType.UpLeft:
                        return selectTilesIndex*Mir3BigTileBlock + 10;

                    case TileType.UpRight:
                        return selectTilesIndex*Mir3BigTileBlock + 11;

                    case TileType.DownLeft:
                        return selectTilesIndex*Mir3BigTileBlock + 12;
                    case TileType.DownRight:
                        return selectTilesIndex*Mir3BigTileBlock + 13;

                    case TileType.InUpLeft:
                        return selectTilesIndex*Mir3BigTileBlock + 15;
                    case TileType.InUpRight:
                        return selectTilesIndex*Mir3BigTileBlock + 16;
                    case TileType.InDownLeft:
                        return selectTilesIndex*Mir3BigTileBlock + 17;
                    case TileType.InDownRight:
                        return selectTilesIndex*Mir3BigTileBlock + 18;
                }
            }
            else
            {
                switch (tileType)
                {
                    case TileType.Center:
                        return (selectTilesIndex - flag)*Mir3BigTileBlock + random.Next(5, 10);
                    case TileType.Up:
                        return (selectTilesIndex - flag)*Mir3BigTileBlock + random.Next(22, 24);
                    case TileType.Down:
                        return (selectTilesIndex - flag)*Mir3BigTileBlock + random.Next(20, 22);
                    case TileType.Left:
                        if (random.Next(2) == 0)
                        {
                            return (selectTilesIndex - flag)*Mir3BigTileBlock + 26;
                        }
                        return (selectTilesIndex - flag)*Mir3BigTileBlock + 28;
                    case TileType.Right:
                        if (random.Next(2) == 0)
                        {
                            return (selectTilesIndex - flag)*Mir3BigTileBlock + 25;
                        }
                        return (selectTilesIndex - flag)*Mir3BigTileBlock + 27;
                    case TileType.UpLeft:
                        return (selectTilesIndex - flag)*Mir3BigTileBlock + 18;

                    case TileType.UpRight:
                        return (selectTilesIndex - flag)*Mir3BigTileBlock + 17;

                    case TileType.DownLeft:
                        return (selectTilesIndex - flag)*Mir3BigTileBlock + 16;
                    case TileType.DownRight:
                        return (selectTilesIndex - flag)*Mir3BigTileBlock + 15;

                    case TileType.InUpLeft:
                        return (selectTilesIndex - flag)*Mir3BigTileBlock + 13;
                    case TileType.InUpRight:
                        return (selectTilesIndex - flag)*Mir3BigTileBlock + 12;
                    case TileType.InDownLeft:
                        return (selectTilesIndex - flag)*Mir3BigTileBlock + 11;
                    case TileType.InDownRight:
                        return (selectTilesIndex - flag)*Mir3BigTileBlock + 10;
                }
            }

            return -1;
        }

        private int RandomAutoSmTile(TileType iTileType)
        {
            //然后我们根据这个规律就可以返回某种地砖类型的某一块了。。
            if ((int) iTileType >= 1) //这除了中这种地砖类型
            {
                return selectTilesIndex*smTileBlock + 8 + ((int) iTileType - 1)*4 + random.Next(4);
            }
            //中间地砖
            return selectTilesIndex*smTileBlock + random.Next(8);
        }

        private enum Layer
        {
            None,
            BackImage,
            MiddleImage,
            FrontImage,
            BackLimit,
            FrontLimit,
            BackFrontLimit,
            GraspingMir2Front,
            GraspingInvertMir3FrontMiddle,
            PlaceObjects,
            ClearAll,
            ClearBack,
            ClearMidd,
            ClearFront,
            ClearBackFrontLimit,
            ClearBackLimit,
            ClearFrontLimit,
            BrushMir2BigTiles,
            BrushSmTiles,
            BrushMir3BigTiles
        }

        private enum MirVerSion : byte
        {
            None,
            WemadeMir2,
            ShandaMir2,
            WemadeMir3,
            ShandaMir3
        }

        private enum TileType
        {
            None = -1,
            Center,
            Up,
            UpRight,
            Right,
            DownRight,
            Down,
            DownLeft,
            Left,
            UpLeft,
            InUpRight,
            InDownRight,
            InDownLeft,
            InUpLeft
        }

        #region Idle Check

        public static bool AppStillIdle
        {
            get
            {
                PeekMsg msg;
                return !PeekMessage(out msg, IntPtr.Zero, 0, 0, 0);
            }
        }

        [SuppressUnmanagedCodeSecurity]
        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        private static extern bool PeekMessage(out PeekMsg msg, IntPtr hWnd, uint messageFilterMin,
            uint messageFilterMax, uint flags);

        [StructLayout(LayoutKind.Sequential)]
        private struct PeekMsg
        {
            private readonly IntPtr hWnd;
            private readonly Message msg;
            private readonly IntPtr wParam;
            private readonly IntPtr lParam;
            private readonly uint time;
            private readonly Point p;
        }

        #endregion
    }
}