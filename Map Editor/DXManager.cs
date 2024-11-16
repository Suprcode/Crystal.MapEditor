using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;
using SlimDX;
using SlimDX.Direct3D9;
using Blend = SlimDX.Direct3D9.Blend;

namespace Map_Editor
{


    class DXManager
    {
        public static List<MLibrary.MImage> TextureList = new List<MLibrary.MImage>();
        /// <summary>
        /// Master Device
        /// </summary>
        public static Device Device;
        /// <summary>
        /// Elf
        /// </summary>
        public static Sprite Sprite, TextSprite;
        /// <summary>
        /// Wire
        /// </summary>
        public static Line Line;
        /// <summary>
        /// Current Surface or Existing Surface
        /// </summary>
        public static Surface CurrentSurface;
        /// <summary>
        /// Main surface
        /// </summary>
        public static Surface MainSurface;
        /// <summary>
        /// Describes a rendering parameter. 
        /// </summary>
        public static PresentParameters Parameters;
        /// <summary>
        /// Whether the device is lost: true if lost, false if not lost
        /// </summary>
        public static bool DeviceLost;
        /// <summary>
        /// Opacity
        /// </summary>
        public static float Opacity = 1F;
        /// <summary>
        /// Blending
        /// </summary>
        public static bool Blending;

        /// <summary>
        /// Light Texture Collection
        /// </summary>
        public static List<Texture> Lights = new List<Texture>();

        /// <summary>
        /// Light source coordinates collection
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
                //Retrieves or sets the format of the back buffer // A 32-bit RGB pixel format using 8 bits per color.
                BackBufferFormat = Format.X8R8G8B8,
                //Retrieve or set the presentation flag //Notify the driver that the back buffer contains video data
                PresentFlags = PresentFlags.LockableBackBuffer,
                //Retrieves or sets the height of the swap chain's back buffer
                BackBufferWidth = control.Width,
                //Retrieves or sets the width of the swap chain's back buffer
                BackBufferHeight = control.Height,
                //Retrieve or set the swap effect ////After the current screen is drawn it will be automatically deleted from memory
                SwapEffect = SwapEffect.Discard,
                //Describes the relationship between the adapter refresh rate and the rate at which the Device's Present operations are completed
				//One The driver needs to wait for the vertical retrace cycle (the runtime will implement tracking to prevent disconnection). The frequency of the Present operation being affected does not exceed the refresh rate of the screen; the runtime completes at most one Present operation per adapter refresh cycle. This option is always available for both windowed and fullscreen swap chains.
				//Immediate The runtime will update the window client area immediately, and may update it more than once during the adapter refresh cycle. The Present operation may be affected immediately. This option is always available for both windowed and fullscreen swap chains
                PresentationInterval =  PresentInterval.One,
                DeviceWindowHandle = control.Handle,
                //Indicates whether the application is running in windowed mode. //If the application runs with a window, it is true, otherwise it is false
                Windowed = true
            };

            Direct3D d3d = new Direct3D();

            //Retrieve device-specific information.
            Capabilities devCaps = d3d.GetDeviceCaps(0, DeviceType.Hardware);

            // Specifies the device type.

            //---------------DeviceType------------
            //NullReference 4 A null reference rasterizer version.
            //Software 3 A software device.
            //Reference 2 Microsoft Direct3D features are implemented in software; however, the reference rasterizer uses special CPU instructions whenever possible.
            //Hardware 1 A hardware device
            DeviceType devType = DeviceType.Reference;
            //Defines the flags to use when creating a device
            CreateFlags devFlags = CreateFlags.HardwareVertexProcessing; //Specifying hardware vertex processing
            //Retrieves an object indicating the vertex shader versions supported by the primary and subordinate devices. Retrieves an object indicating the primary and subordinate pixel shader versions.
            if (devCaps.VertexShaderVersion.Major >= 2 && devCaps.PixelShaderVersion.Major >= 2)
                //Equipment type
                devType = DeviceType.Hardware;

            //NoWindowChanges 2048 Microsoft Direct3D instructs the runtime not to change the focused window in any way. Use with caution! The burden of supporting centralized management events (ALT tabs, etc.) falls on the application, and appropriate responses (switching display modes, etc.) should be coded.
            //DisableDriverManagementEx 1024 Specifies that resource management utilizes Direct3D instead of the device. Direct3D resource calls will not fail for errors such as insufficient video memory.
            //AdapterGroupDevice 512
            //DisableDriverManagement 256 Specifies that resource management utilizes Direct3D instead of the device. Direct3D resource calls will not fail for errors such as insufficient video memory.
            //MixedVertexProcessing 128 Specifies mixed vertex processing (both software and hardware).
            //HardwareVertexProcessing 64 Specifies hardware vertex processing.
            //SoftwareVertexProcessing 32 Specifies software vertex processing.
            //PureDevice 16 If the device does not support vertex processing, the application can only use post-change vertices.
            //MultiThreaded 4 indicates that the application requests multithreaded safety for Direct3D. This causes Direct3D to check its global critical sections more frequently, which can reduce performance. Starting with the Microsoft DirectX 9.0 SDK Update (Summer 2003), this always specifies the enumeration value unless the parameter is four. ForceNoMultiThreadedFlag is set to true.
            //FpuPreserve 2 indicates that the application requires floating-point unit (FPU) exceptions or double-precision floating-point exceptions to be enabled. By default, Direct3D uses single precision. Because Direct3D sets the FPU state each time it is called, setting this flag reduces Direct3D performance.

            //Indicates whether the device supports hardware transformations and lighting.
            if ((devCaps.DeviceCaps & DeviceCaps.HWTransformAndLight) != 0)
                //Specifies hardware vertex processing.
                devFlags = CreateFlags.HardwareVertexProcessing;

            //Indicates whether the device supports rasterization, transformations, lighting, and shading in hardware.
            if ((devCaps.DeviceCaps & DeviceCaps.PureDevice) != 0)
                //If the device does not support vertex processing, the application can just use the altered vertices.
                devFlags |= CreateFlags.PureDevice;

            //            Parameter
            //adapter
            //    An ordinal number that identifies which physical device the object represents. Device 0 represents the default device. The maximum value that can be used in this parameter is the total number of physical devices minus 1.

            //deviceType
            //    DeviceType A member of an enumeration type that represents the type of device required. This method fails if the required device type is not available.

            //renderWindow
            //    Form or any other Control A handle to a derived class. This parameter indicates the surface to be bound to the device.
            //    The specified window must be a top-level window, Null values ​​are not supported.

            //behaviorFlags
            //    A combination of one or more options that control the device creation operation.

            //presentationParameters
            //    一个 PresentParameters Object that describes the presentation parameters of the device to be created.

            Device = new Device(d3d, d3d.Adapters.DefaultAdapter.Adapter, devType, control.Handle, devFlags, Parameters);

            //Enables use of the Microsoft Windows Graphics Device Interface (GDI) in full-screen application dialog boxes.
            Device.SetDialogBoxMode(true);

            LoadTextures();
        }

        /// <summary>
        /// Loading Textures
        /// </summary>
        private static unsafe void LoadTextures()
        {//The unsafe keyword denotes an unsafe context, which is required for any operation involving pointers.

            //The methods and properties provided by Sprite are used to simplify the process of drawing sprites using Direct3D.
			//Initialize a new instance of the Sprite class.
			//Parameters
			//device
			//An instance of Device.
            Sprite = new Sprite(Device);
            TextSprite = new Sprite(Device);
            //The Line class draws a straight line between two points.
            Line = new Line(Device) { Width = 1F };
            //Get the specified back buffer.
			//Parameters
			//swapChain
			//Unsigned integer that specifies the swap chain.

			//backBuffer
			//The index of the back buffer object to return.

			//backBufferType
			//The type of the back buffer to return. Only Mono is a valid value.
			//Return value: A Surface that represents the returned back buffer surface.
            MainSurface = Device.GetBackBuffer(0, 0);
            CurrentSurface = MainSurface;

			//SetRenderTarget sets a new color buffer for the device.
			// Parameters

			//renderTarget
			// The indexed Surface of the render target.

			//newZStencil
			// The new color buffer Surface. If this parameter is set to a null reference (Nothing in Visual Basic), the color buffer for the corresponding renderTarget is disabled. A device must always be associated with a color buffer.
			// The new render target surface must specify at least Usage.
			Device.SetRenderTarget(0, MainSurface);

			//if (RadarTexture == null || RadarTexture.Disposed)
			//{
			// //Parameters

			// //device
			// // Type: Microsoft.WindowsMobile.DirectX.Direct3D.Device
			// // The Device object to associate with the Texture.

			// //width
			// // Type: System.Int32
			// // The width of the top level of the texture, in pixels. The pixel dimensions of subsequent levels are truncated to half the pixel dimensions of the previous level (calculated independently for each level). Each dimension is constrained to be at least one pixel in size. Therefore, if division by 2 and truncation results in 0, 1 is used instead.

			// //height
			// // Type: System.Int32
			// // The height of the top level of the texture in pixels. The pixel dimensions of subsequent levels are truncated to half the pixel dimensions of the previous level (the calculation is done independently for each level). Each dimension is limited to at least one pixel in size. Therefore, if division by 2 and truncation results in 0, 1 is used instead.

			// //numLevels
			// // Type: System.Int32
			// // The number of levels in the texture. If this value is 0, for hardware that supports mipmapped textures, Direct3D generates all children of the texture down to a size of 1 x 1 pixel. Check the BaseTexture.LevelCount property to see how many levels were generated.

			// //usage
			// // Type: Microsoft.WindowsMobile.DirectX.Direct3D.Usage
			// // Usage can be 0, indicating no usage value. However, if usage is required, use one or more Usage constants. It is recommended that the usage parameter matches the CreateFlags in the Device's constructor.

			// //format
			// // Type: Microsoft.WindowsMobile.DirectX.Direct3D.Format
			// // A Format value describing the format of all levels in the texture.

			// //pool
			// // Type: Microsoft.WindowsMobile.DirectX.Direct3D.Pool
			// // A Pool value describing the memory class that the texture should be placed in.
			// RadarTexture = new Texture(Device, 2, 2, 1, Usage.None, Format.A8R8G8B8, Pool.Managed);

			// //GraphicsStream contains a stream of graphics data.
			// //LockRectangle locks a rectangle in a texture resource.
			// //Parameters

			// //level
			// // Type: System.Int32
			// // The minmap level of the texture resource to lock.

			// //flags
			// // Type: Microsoft.WindowsMobile.DirectX.Direct3D.LockFlags
			// // Zero or more LockFlags values ​​describing the type of locking to perform. Valid flags for this method are Discard, NoDirtyUpdate, and ReadOnly.

			// //Return Value
			// // GraphicsStream describing the locked region.

			// //-------------------LockFlags----------------------
			// //None The application can both read from and write to the buffer.
			// //ReadOnly The application does not write to the buffer. This flag can be used to save the recompression step when unlocking resources stored in non-native formats. NoDirtyUpdate By default, a lock on a resource adds a "dirty" region to the resource. This flag disables any changes to the dirty state of the resource. The application should use this flag when it changes additional information about a set of regions during a locked operation.
			// //NoOverwrite ensures that the application does not overwrite any data in the vertex and index buffers. When using vertex buffers with this flag, the driver can return immediately and continue rendering. If this flag is not used, the driver must complete rendering before returning from the locked state.
			// //Discard The application overwrites every location within the locked region using a write-only operation. This option is a valid option when using dynamic textures, dynamic vertex buffers, and dynamic index buffers.
			// using (GraphicsStream stream = RadarTexture.LockRectangle(0, LockFlags.Discard))

			// //Bitmap encapsulates a GDI+ bitmap, which consists of the pixel data of a graphics image and its characteristics. A Bitmap is an object used to manipulate images defined by pixel data.

			// //Initializes a new instance of the Bitmap class with the specified size, pixel format, and pixel data
			// //Parameters

			// //width
			// // The width of the new Bitmap in pixels.

			// //height
			// // The height of the new Bitmap in pixels.

			// //stride
			// // An integer specifying the byte offset between the start of adjacent scanlines. This is usually (but not necessarily) the number of bytes represented in the pixel format (for example, 2 for 16 bits per pixel) times the width of the bitmap. The value passed to this parameter must be a multiple of 4.

			// //format
			// // The pixel format of the new Bitmap. This must specify a value beginning with Format.

			// //scan0
			// // Pointer to an array of bytes containing pixel data.
			// using (Bitmap image = new Bitmap(2, 2, 8, PixelFormat.Format32bppArgb, (IntPtr)stream.InternalDataPointer))
			// // Creates a new Graphics from the specified Image.
			// using (Graphics graphics = Graphics.FromImage(image))
			// // Clears the entire drawing surface and fills it with the specified background color.
			// graphics.Clear(Color.White);
			//}
			//if (PoisonDotBackground == null || PoisonDotBackground.Disposed)
			//{
			// PoisonDotBackground = new Texture(Device, 5, 5, 1, Usage.None, Format.A8R8G8B8, Pool.Managed);
			// //Lock the rectangle in the texture resource.
			// using (GraphicsStream stream = PoisonDotBackground.LockRectangle(0, LockFlags.Discard))
			// //Initialize a new instance of the Bitmap class with the specified size, pixel format, and pixel data
			// using (Bitmap image = new Bitmap(5, 5, 20, PixelFormat.Format32bppArgb, (IntPtr)stream.InternalDataPointer))
			// //Create a new Graphics from the specified Image.
			// using (Graphics graphics = Graphics.FromImage(image))
			// // Clear the entire drawing surface and fill it with the specified background color
			// graphics.Clear(Color.White);
			//}
			//CreateLights();
        }
        /// <summary>
        /// Creating a Light Source
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
                ////LockRectangle Locks the rectangle in the texture resource
                DataRectangle stream = light.LockRectangle(0, LockFlags.Discard);
                //Initializes a new instance of the Bitmap class with the specified size, pixel format, and pixel data.
                using (Bitmap image = new Bitmap(width, height, width * 4, PixelFormat.Format32bppArgb, stream.Data.DataPointer))
                {
                    //Creates a new Graphics from the specified Image.
                    using (Graphics graphics = Graphics.FromImage(image))
                    {
                        //GraphicsPath represents a series of connected straight lines and curves
                        using (GraphicsPath path = new GraphicsPath())
                        {
                            //AddEllipse Adds an ellipse to the current path.
                            // Parameter

                            //x
                            //    Type: System.Int32
                            //    Defines the X coordinate of the upper-left corner of the ellipse's bounding box.

                            //y
                            //    Type: System.Int32
                            //    Defines the Y coordinate of the upper-left corner of the ellipse's bounding box.

                            //width
                            //    Defines the width of the border of the ellipse.

                            //height
                            //    Defines the height of the bounding box of the ellipse.
                            path.AddEllipse(new Rectangle(0, 0, width, height));
                            // PathGradientBrush class Encapsulates a Brush object that fills the interior of a GraphicsPath object with a gradient.
                            //  Initializes a new instance of the PathGradientBrush class using the specified path.
                            //  parameter
                            //path
                            //    A GraphicsPath that defines the area filled by this PathGradientBrush.
                            using (PathGradientBrush brush = new PathGradientBrush(path))
                            {
                                graphics.Clear(Color.FromArgb(0, 0, 0, 0));
                                //Gets or sets an array of colors corresponding to the points in the path filled by this PathGradientBrush.
                                brush.SurroundColors = new[] { Color.FromArgb(0, 255, 255, 255) };
                                //Gets or sets the color at the center of the path gradient
                                brush.CenterColor = Color.FromArgb(255, 255, 255, 255);
                                //Fills the interior of a GraphicsPath.
                                //parameter

                                //brush
                                //    A Brush that determines the fill characteristics.

                                //path
                                //    A GraphicsPath that represents the path to be filled.
                                graphics.FillPath(brush, path);
                                //Saves the current state of this Graphics and identifies the saved state with GraphicsState.
                                graphics.Save();
                            }
                        }
                    }
                }
                light.UnlockRectangle(0);

                stream.Data.Dispose();

                //Adding lights
                Lights.Add(light);
            }
        }
        /// <summary>
        /// Setting the Surface
        /// </summary>
        /// <param name="surface"></param>
        public static void SetSurface(Surface surface)
        {
            if (CurrentSurface == surface)
                return;
            //Forces all batch sprites to be submitted to the device. 
            Sprite.Flush();
            CurrentSurface = surface;
            //Sets a new color buffer for the device. 
            Device.SetRenderTarget(0, surface);
        }
        /// <summary>
        /// Try resetting
        /// </summary>
        public static void AttemptReset()
        {
            try
            {
                Result result;
                //CheckCooperativeLevel reports the current cooperative level state of the Direct3D device for a windowed or full-screen application.
                //Parameters

                //hResult
                // The current cooperative level state of the device for a windowed or full-screen application, reported with a ResultCode value. A Success result indicates that the device is operational and the calling application can continue. A DeviceLost result indicates that the device is lost, but cannot be reset at this time; therefore, rendering is not possible. A DeviceNotReset result indicates that the device is lost, but can be reset at this time.

                //Return value
                // True if the device is operational and the calling application can continue; false if the device is lost or needs to be reset.
                result = Device.TestCooperativeLevel();

                //    -------------ResultCode)result return value type-------------
                // AlreadyLocked The device is locked.
                // ConflictingRenderState The rendering states are incompatible.
                // ConflictingTextureFilter The current texture filters cannot be used together.
                // ConflictingTexturePalette The current texture palettes cannot be used together.
                // DeviceLost The device has been lost, but it cannot be reset at this time. Therefore, rendering is not possible.
                // DeviceNotReset The device does not support the queried technology.
                // DriverInternalError A driver internal error occurred.
                // DriverInvalidCall The driver detected an invalid call.
                // DriverUnsupported The driver is not supported.
                // GenericFailure Generic failure.
                // InvalidCall The method call is invalid; for example, the method's parameters might not be valid pointers.
                // InvalidDevice The requested device type is invalid.
                // MemoryPoolEmpty The specified memory pool is empty.
                // MoreData The buffer passed into the routine does not contain enough elements to complete the operation.
                // NotAvailable The requested format is not available.
                // NotFound The search routine failed to return an element.
                // Success The operation was successful.
                // TooManyOperations The application requested more texture filter operations than the device supports.
                // UnsupportedAlphaArgument The device does not support the specified texture blending parameters for the alpha channel.
                // UnsupportedAlphaOperation The device does not support the specified texture blending operation for the alpha channel.
                // UnsupportedColorArgument The device does not support the specified texture blending parameters for color values.
                // UnsupportedColorOperation The device does not support the specified texture blending operation for color values.
                // UnsupportedFactorValue The device does not support the specified texture factor value.
                // UnsupportedTextureFilter The device does not support the specified texture filter.
                // WrongTextureFormat The pixel format of the texture surface is invalid.

                //The device does not support the technology being queried.
                if (result.Code == ResultCode.DeviceNotReset.Code)
                {
                    //Device.Reset(Parameters) resets the presentation parameters of the current device. 
                    //parameter

                    //presentationParameters
                    //    A PresentParameters structure that describes the new presentation parameters. This parameter cannot be null.

                    //Remarks
                    //When switching to full-screen mode, Direct3D attempts to find a desktop format that matches the back buffer format so that the back buffer format and the front buffer format are the same. This eliminates the need to convert colors.
                    //If the call to Reset fails, the device is placed in the "lost" state (indicated by a value of false returned from a call to CheckCooperativeLevel) unless the device is in the "not reset" state (indicated by DeviceNotReset returned from the hResult parameter of the CheckCooperativeLevel method).
                    //Calling Reset causes all texture memory surfaces and state information to be lost, and also causes managed textures to be flushed in video memory. Before calling Reset on a device, the application should release any explicit render targets, depth stencil surfaces, attached swap chains, state blocks, and default resources associated with the device.
                    //Swap chains can be full-screen or windowed. If the new swap chain is full-screen, the adapter is placed in a display mode that matches the new size.
                    //If the thread calling Reset is not the thread that created the device being reset, the call will fail.
                    //When calling Device, Reset, and SwapChain, you can specify the format of the window mode back buffer as "unknown". This means that the application does not have to query the current desktop format before calling Device in window mode. For full-screen mode, the back buffer format must be specified. Setting BackBufferCount to 0 creates a back buffer.
                    //When trying to reset multiple display adapters in a group, you can pass in an array of PresentParameters objects, each of which corresponds to a display adapter in the adapter group.
                    ResetDevice();
                }
                //The device has been lost, but it cannot be reset at this time. Therefore, rendering is not possible.
                else if (result.Code == ResultCode.DeviceLost.Code)
                {
                }
                //Successful operation
                else if (result.Code == ResultCode.Success.Code)
                {
                    DeviceLost = false;
                    //Gets the specified back buffer.
                    MainSurface = Device.GetBackBuffer(0, 0);
                    //Get the specified back buffer
                    CurrentSurface = Device.GetBackBuffer(0, 0);
                    //Sets a new color buffer for the device.
                    Device.SetRenderTarget(0, CurrentSurface);
                }

                DeviceLost = false;
            }
            catch
            {
            }
        }

        public static void ResetDevice()
        {
            if (DeviceLost) return;

            Clean();
            DeviceLost = true;

            if (Parameters == null) return;

            Size clientSize = _control.ClientSize;

            if (clientSize.Width == 0 || clientSize.Height == 0) return;


            Parameters.BackBufferWidth = clientSize.Width;
            Parameters.BackBufferHeight = clientSize.Height;
            Device.Reset(Parameters);

            LoadTextures();
        }

        /// <summary>
        /// Attempt to recover
        /// </summary>
        public static void AttemptRecovery()
        {
            try
            {
                //Restores the device to the state it was in before Begin was called.
                //Remark
                //This method cannot be used to override the Device's EndScene.

                Sprite.End();
                TextSprite.End();
            }
            catch
            {
            }

            try
            {
                //Ends a scene that was started by calling the BeginScene method.
                //Remark
                //Each call to BeginScene should ultimately be followed by a call to EndScene before updating the display with Present.
                //When EndScene succeeds, the scene is queued for rendering by the driver. This method is not synchronous, so there is no guarantee that the scene has finished rendering when this method returns.

                Device.EndScene();
            }
            catch
            {
            }

            try
            {
                ////Get the specified back buffer
                MainSurface = Device.GetBackBuffer(0, 0);
                CurrentSurface = MainSurface;
                ////Sets a new color buffer for the device.
                Device.SetRenderTarget(0, MainSurface);
            }
            catch
            {
            }
        }
        /// <summary>
        /// Set opacity
        /// </summary>
        /// <param name="opacity"> Opacity</param>
        public static void SetOpacity(float opacity)
        {
            if (Opacity == opacity)
                return;
            //Forces all batch sprites to be submitted to the device. 
            Sprite.Flush();
            //获取设备的呈现状态值。AlphaBlendEnable= true;
            Device.SetRenderState(RenderState.AlphaBlendEnable, true);
            if (opacity >= 1 || opacity < 0)
            {

                //----------Blend Enumeration ------------
                //DestinationAlpha	The mixing factor is (Ad, Ad, Ad, Ad)。 
                //	DestinationColor	The mixing factor is (Rd, Gd, Bd, Ad)。 
                //	InvDestinationAlpha	The mixing factor is (1 - Ad, 1 - Ad, 1 - Ad, 1 - Ad)。 
                //	InvDestinationColor	The mixing factor is (1 - Rd, 1 - Gd, 1 - Bd, 1 - Ad)。 
                //	InvSourceAlpha	The mixing factor is ( 1 - As, 1 - As, 1 - As, 1 - As)。 
                //	InvSourceColor	The mixing factor is (Rs, Gs, Bs, As)。 
                //	One	The mixing factor is (1,1,1,1)。 
                //	SourceAlpha	The mixing factor is (As, As, As, As)。 
                //	SourceAlphaSat	The mixing factor is (f, f, f, 1)；f = min(A, 1 - Ad)。 
                //	SourceColor	The mixing factor is (Rs, Gs, Bs, As)。 
                //	Zero	The mixing factor is (0, 0, 0, 0)。 

                //Gets or sets the color blending mode. 
                Device.SetRenderState(RenderState.SourceBlend, SlimDX.Direct3D9.Blend.SourceAlpha);
                //Indicates the current blending mode or the blending mode to be set
                Device.SetRenderState(RenderState.DestinationBlend, SlimDX.Direct3D9.Blend.InverseSourceAlpha);
                //Get or set the alpha blending mode
                Device.SetRenderState(RenderState.SourceBlendAlpha, Blend.One);
                //Blend a drawing operation using an existing color
                Device.SetRenderState(RenderState.BlendFactor, Color.FromArgb(255, 255, 255, 255).ToArgb());
            }
            else
            {
                var r = (byte)(255 * opacity);
                Device.SetRenderState(RenderState.SourceBlend, Blend.BlendFactor);
                Device.SetRenderState(RenderState.DestinationBlend, Blend.InverseBlendFactor);
                Device.SetRenderState(RenderState.SourceBlendAlpha, Blend.SourceAlpha);
                Device.SetRenderState(RenderState.BlendFactor, Color.FromArgb(r, r, r, r).ToArgb());
            }
            Opacity = opacity;
            //Forces all batch sprites to be submitted to the device.
            Sprite.Flush();
        }
        /// <summary>
        /// Setting up the mix
        /// </summary>
        /// <param name="value"> Whether</param>
        /// <param name="rate">Rate</param>
        public static void SetBlend(bool value, float rate = 1F)
        {
            if (value == Blending) return;
            Blending = value;
            //Forces all batch sprites to be submitted to the device.
            Sprite.Flush();
            // //Restores the device to the state it was in before Begin was called.
            Sprite.End();

            if (Blending)
            {
                //Sprite.Begin prepares the sprite for drawing
				//Parameters
				//flags
				//A combination of zero or more values ​​from SpriteFlags that describe the sprite rendering options.

				// AlphaBlend enables alpha blending with AlphaTestEnable set to true (for non-zero alpha). SourceAlpha is the source blend state and InvSourceAlpha is the destination blend state in a RenderStateManager call. The Font class requires this flag to be set when drawing text.
				// Billboard rotates each sprite around its center so that it faces the viewer. Must call SetWorldViewLH or SetWorldViewRH first.
				// DoNotModifyRenderState specifies that the device rendering state should not be changed when Begin is called.
				// DoNotSaveState disables saving or restoring the device state when Begin and End are called.
				// None interprets the value as 0.
				// ObjectSpace specifies that the world, view, and projection transforms should not be modified. The transformation currently set for the device is used to transform sprites when they are drawn in batches (that is, when Begin or End is called). If this option is not specified, the world, view, and projection transformations are modified so that sprites are drawn in screen space coordinates.
				// SortDepthBackToFront Sort sprites by depth, back to front, before drawing. This option is recommended when drawing transparent sprites of varying depths.
				// SortDepthFrontToBack Sort sprites by depth, front to back, before drawing. This option is recommended when drawing opaque sprites of varying depths.
				// SortTexture Sort sprites by texture, before drawing. This option is recommended when drawing non-overlapping sprites of uniform depth; for example, when drawing screen-aligned text with a Font. 

                Sprite.Begin(SpriteFlags.DoNotSaveState);//Disables saving or restoring device state between calls to Begin and End. 
                var r = (byte)(255 * rate);
                Device.SetRenderState(RenderState.AlphaBlendEnable, true);
                Device.SetRenderState(RenderState.SourceBlend, Blend.BlendFactor);
                Device.SetRenderState(RenderState.DestinationBlend, Blend.One);
                Device.SetRenderState(RenderState.BlendFactor, Color.FromArgb(r, r, r, r).ToArgb());
            }
            else
                //Sprite.Begin prepares the drawing of the sub-screen
                Sprite.Begin(SpriteFlags.AlphaBlend);//Enables alpha blending with AlphaTestEnable set to true (for non-zero alpha). SourceAlpha is the source blend state and InvSourceAlpha is the destination blend state in a RenderStateManager call. The Font class requires this flag to be set when drawing text.
            //SetRenderTarget sets a new color buffer for the device.
            Device.SetRenderTarget(0, CurrentSurface);
        }
        /// <summary>
        /// Clear
        /// </summary>
        public static void Clean()
        {
            if (Sprite != null)
            {
                if (!Sprite.Disposed)
                {
                    Sprite.Dispose();
                }

                Sprite = null;
            }

            if (TextSprite != null)
            {
                if (!TextSprite.Disposed)
                {
                    TextSprite.Dispose();
                }

                TextSprite = null;
            }

            if (Line != null)
            {
                if (!Line.Disposed)
                {
                    Line.Dispose();
                }

                Line = null;
            }

            if (CurrentSurface != null)
            {
                if (!CurrentSurface.Disposed)
                {
                    CurrentSurface.Dispose();
                }

                CurrentSurface = null;
            }

            if (Lights != null)
            {
                for (int i = 0; i < Lights.Count; i++)
                {
                    if (!Lights[i].Disposed)
                        Lights[i].Dispose();
                }
                Lights.Clear();
            }

            for(int i = TextureList.Count - 1; i >= 0; i--)
            {
                var m = TextureList[i];

                if (m == null) continue;

                m.DisposeTexture();
            }
            TextureList.Clear();
        }
    }
}
