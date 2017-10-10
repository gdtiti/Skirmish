﻿using SharpDX;
using SharpDX.Direct3D;
using SharpDX.DXGI;
using System;
using BindFlags = SharpDX.Direct3D11.BindFlags;
using BlendOperation = SharpDX.Direct3D11.BlendOperation;
using BlendOption = SharpDX.Direct3D11.BlendOption;
using BlendState = SharpDX.Direct3D11.BlendState;
using BlendStateDescription = SharpDX.Direct3D11.BlendStateDescription;
using Buffer = SharpDX.Direct3D11.Buffer;
using ColorWriteMaskFlags = SharpDX.Direct3D11.ColorWriteMaskFlags;
using Comparison = SharpDX.Direct3D11.Comparison;
using CpuAccessFlags = SharpDX.Direct3D11.CpuAccessFlags;
using CullMode = SharpDX.Direct3D11.CullMode;
using DepthStencilClearFlags = SharpDX.Direct3D11.DepthStencilClearFlags;
using DepthStencilOperationDescription = SharpDX.Direct3D11.DepthStencilOperationDescription;
using DepthStencilState = SharpDX.Direct3D11.DepthStencilState;
using DepthStencilStateDescription = SharpDX.Direct3D11.DepthStencilStateDescription;
using DepthStencilView = SharpDX.Direct3D11.DepthStencilView;
using DepthStencilViewDescription = SharpDX.Direct3D11.DepthStencilViewDescription;
using DepthStencilViewDimension = SharpDX.Direct3D11.DepthStencilViewDimension;
using DepthWriteMask = SharpDX.Direct3D11.DepthWriteMask;
using Device = SharpDX.Direct3D11.Device;
using DeviceContext = SharpDX.Direct3D11.DeviceContext;
using DeviceCreationFlags = SharpDX.Direct3D11.DeviceCreationFlags;
using FillMode = SharpDX.Direct3D11.FillMode;
using InputLayout = SharpDX.Direct3D11.InputLayout;
using RasterizerState = SharpDX.Direct3D11.RasterizerState;
using RasterizerStateDescription = SharpDX.Direct3D11.RasterizerStateDescription;
using RenderTargetView = SharpDX.Direct3D11.RenderTargetView;
using Resource = SharpDX.Direct3D11.Resource;
using ResourceOptionFlags = SharpDX.Direct3D11.ResourceOptionFlags;
using ResourceUsage = SharpDX.Direct3D11.ResourceUsage;
using StencilOperation = SharpDX.Direct3D11.StencilOperation;
using Texture2D = SharpDX.Direct3D11.Texture2D;
using Texture2DDescription = SharpDX.Direct3D11.Texture2DDescription;
using VertexBufferBinding = SharpDX.Direct3D11.VertexBufferBinding;

namespace Engine
{
    using Engine.Helpers;

    /// <summary>
    /// Graphics class
    /// </summary>
    public class Graphics : IDisposable
    {
        /// <summary>
        /// On resized event
        /// </summary>
        public event EventHandler Resized;

        /// <summary>
        /// Vertical sync enabled
        /// </summary>
        private bool vsyncEnabled = false;
        /// <summary>
        /// Multisample count
        /// </summary>
        private int msCount = 1;
        /// <summary>
        /// Multisample quality
        /// </summary>
        private int msQuality = 0;

        public SampleDescription CurrentSampleDescription
        {
            get
            {
                return new SampleDescription(this.msCount, this.msQuality);
            }
        }
        /// <summary>
        /// Swap chain
        /// </summary>
        private SwapChain swapChain = null;
        /// <summary>
        /// Render target view
        /// </summary>
        private RenderTargetView renderTargetView = null;
        /// <summary>
        /// Depth stencil buffer
        /// </summary>
        private Texture2D depthStencilBuffer = null;
        /// <summary>
        /// Depth stencil view
        /// </summary>
        private DepthStencilView depthStencilView = null;

        /// <summary>
        /// Current depth-stencil state
        /// </summary>
        private DepthStencilState currentDepthStencilState = null;
        /// <summary>
        /// Current depth-stencil state reference value
        /// </summary>
        private int currentDepthStencilStateRef = 0;
        /// <summary>
        /// Current blend state
        /// </summary>
        private BlendState currentBlendState = null;
        /// <summary>
        /// Current rasterizer state
        /// </summary>
        private RasterizerState currentRasterizerState = null;
        /// <summary>
        /// Depth stencil state with z-buffer enabled for write
        /// </summary>
        private DepthStencilState depthStencilzBufferEnabled = null;
        /// <summary>
        /// Depth stencil state with z-buffer disabled for write
        /// </summary>
        private DepthStencilState depthStencilzBufferDisabled = null;
        /// <summary>
        /// Depth stencil state with z-buffer enabled for read
        /// </summary>
        private DepthStencilState depthStencilRDzBufferEnabled = null;
        /// <summary>
        /// Depth stencil state with z-buffer disabled for read
        /// </summary>
        private DepthStencilState depthStencilRDzBufferDisabled = null;
        /// <summary>
        /// No depth, no stencil
        /// </summary>
        private DepthStencilState depthStencilNone = null;
        /// <summary>
        /// Depth stencil state for volume marking
        /// </summary>
        private DepthStencilState depthStencilVolumeMarking = null;
        /// <summary>
        /// Depth stencil state for volume drawing
        /// </summary>
        private DepthStencilState depthStencilVolumeDrawing = null;
        /// <summary>
        /// Default blend state
        /// </summary>
        private BlendState blendDefault = null;
        /// <summary>
        /// Default alpha blend state
        /// </summary>
        private BlendState blendDefaultAlpha = null;
        /// <summary>
        /// Blend state for transparent blending
        /// </summary>
        private BlendState blendTransparent = null;
        /// <summary>
        /// Additive blend state
        /// </summary>
        private BlendState blendAdditive = null;
        /// <summary>
        /// Blend state for deferred lighting blending
        /// </summary>
        private BlendState blendDeferredLighting = null;
        /// <summary>
        /// Blend state for defered composer blending
        /// </summary>
        private BlendState blendDeferredComposer = null;
        /// <summary>
        /// Blend state for transparent defered composer blending
        /// </summary>
        private BlendState blendDeferredComposerTransparent = null;
        /// <summary>
        /// Default rasterizer
        /// </summary>
        private RasterizerState rasterizerDefault = null;
        /// <summary>
        /// Wireframe rasterizer
        /// </summary>
        private RasterizerState rasterizerWireframe = null;
        /// <summary>
        /// No-cull rasterizer
        /// </summary>
        private RasterizerState rasterizerNoCull = null;
        /// <summary>
        /// Cull counter-clockwise face rasterizer
        /// </summary>
        private RasterizerState rasterizerCullFrontFace = null;
        /// <summary>
        /// Stencil pass rasterizer (No Cull, No depth limit)
        /// </summary>
        private RasterizerState rasterizerStencilPass = null;
        /// <summary>
        /// Lighting pass rasterizer (Cull Front faces, No depth limit)
        /// </summary>
        private RasterizerState rasterizerLightingPass = null;
        /// <summary>
        /// Current vertex buffer first slot
        /// </summary>
        private int currentVertexBufferFirstSlot = -1;
        /// <summary>
        /// Current vertex buffer bindings
        /// </summary>
        private VertexBufferBinding[] currentVertexBufferBindings = null;
        /// <summary>
        /// Current index buffer reference
        /// </summary>
        private Buffer currentIndexBufferRef = null;
        /// <summary>
        /// Current index buffer format
        /// </summary>
        private Format currentIndexFormat = Format.Unknown;
        /// <summary>
        /// Current index buffer offset
        /// </summary>
        private int currentIndexOffset = -1;
        /// <summary>
        /// Current primitive topology set in input assembler
        /// </summary>
        private PrimitiveTopology currentIAPrimitiveTopology = PrimitiveTopology.Undefined;
        /// <summary>
        /// Current input layout set in input assembler
        /// </summary>
        private InputLayout currentIAInputLayout = null;

        /// <summary>
        /// Back buffer format
        /// </summary>
        protected Format BufferFormat = BackBufferFormats.R8G8B8A8_UNorm;
        /// <summary>
        /// Depth buffer format
        /// </summary>
        protected Format DepthFormat = DepthBufferFormats.D24_UNorm_S8_UInt;

        /// <summary>
        /// Graphics device
        /// </summary>
        public Device Device { get; private set; }
        /// <summary>
        /// Graphics inmmediate context
        /// </summary>
        public DeviceContext DeviceContext { get; private set; }
        /// <summary>
        /// Device description
        /// </summary>
        public readonly string DeviceDescription = null;
        /// <summary>
        /// Screen viewport
        /// </summary>
        public ViewportF Viewport { get; private set; }
        /// <summary>
        /// Gets the default render target
        /// </summary>
        public RenderTargetView DefaultRenderTarget
        {
            get
            {
                return this.renderTargetView;
            }
        }
        /// <summary>
        /// Gets the default depth stencil buffer
        /// </summary>
        public DepthStencilView DefaultDepthStencil
        {
            get
            {
                return this.depthStencilView;
            }
        }
        /// <summary>
        /// Gets if the device was created with multi-sampling active
        /// </summary>
        public bool MultiSampled
        {
            get
            {
                return this.msCount > 1;
            }
        }

        /// <summary>
        /// Gets desktop mode description
        /// </summary>
        /// <returns>Returns current desktop mode description</returns>
        public static OutputDescription GetDesktopMode()
        {
            using (Factory1 factory = new Factory1())
            {
                using (Adapter1 adapter = factory.GetAdapter1(0))
                {
                    using (Output adapterOutput = adapter.GetOutput(0))
                    {
                        return adapterOutput.Description;
                    }
                }
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="form">Game form</param>
        /// <param name="refreshRate">Refresh rate</param>
        /// <param name="multiSampling">Enable multisampling</param>
        public Graphics(EngineForm form, bool vsyncEnabled = false, int refreshRate = 0, int multiSampling = 0)
        {
            Adapter1 adapter = null;
            ModeDescription displayMode = this.FindModeDescription(
                this.BufferFormat,
                form.RenderWidth,
                form.RenderHeight,
                form.IsFullscreen,
                refreshRate,
                out adapter);

            using (adapter)
            {
                this.vsyncEnabled = vsyncEnabled && displayMode.RefreshRate != new Rational(0, 1);

                if (multiSampling != 0)
                {
                    using (Device tmpDevice = new Device(adapter))
                    {
                        this.CheckMultisample(tmpDevice, multiSampling, out this.msCount, out this.msQuality);
                    }
                }

                this.DeviceDescription = string.Format("{0}", adapter.Description1.Description);
            }

            DeviceCreationFlags creationFlags = DeviceCreationFlags.None;

#if DEBUG
            creationFlags |= DeviceCreationFlags.Debug;
#endif

            Device device = null;
            Device.CreateWithSwapChain(
                DriverType.Hardware,
                creationFlags,
                new[]
                {
                    FeatureLevel.Level_11_0,
                    FeatureLevel.Level_10_1,
                    FeatureLevel.Level_10_0,
                    FeatureLevel.Level_9_3,
                    FeatureLevel.Level_9_2,
                    FeatureLevel.Level_9_1,
                },
                new SwapChainDescription()
                {
                    BufferCount = 1,
                    ModeDescription = displayMode,
                    Usage = Usage.RenderTargetOutput,
                    OutputHandle = form.Handle,
                    SampleDescription = this.CurrentSampleDescription,
                    IsWindowed = !form.IsFullscreen,
                    SwapEffect = SwapEffect.Discard,
                    Flags = SwapChainFlags.None,
                },
                out device,
                out this.swapChain);

            this.Device = device;
            this.DeviceContext = device.ImmediateContext;

            this.PrepareDevice(displayMode.Width, displayMode.Height, false);

            #region Alt + Enter

            using (Factory factory = this.swapChain.GetParent<Factory>())
            {
                factory.MakeWindowAssociation(form.Handle, WindowAssociationFlags.IgnoreAltEnter);
            }

            form.KeyUp += (sender, eventArgs) =>
            {
                if (eventArgs.Alt && (int)eventArgs.KeyCode == (int)Keys.Enter)
                {
                    this.swapChain.IsFullScreen = !this.swapChain.IsFullScreen;
                }
            };

            #endregion
        }
        /// <summary>
        /// Prepare device
        /// </summary>
        /// <param name="width">Render width</param>
        /// <param name="height">Render height</param>
        /// <param name="resizing">Sets whether the render screen is resizing or not</param>
        public void PrepareDevice(int width, int height, bool resizing)
        {
            if (resizing)
            {
                this.DisposeResources();

                this.swapChain.ResizeBuffers(2, width, height, this.BufferFormat, SwapChainFlags.None);
            }

            #region Viewport

            this.Viewport = new ViewportF()
            {
                X = 0,
                Y = 0,
                Width = width,
                Height = height,
                MinDepth = 0.0f,
                MaxDepth = 1.0f,
            };

            #endregion

            #region Render Target

            using (Resource backBuffer = Resource.FromSwapChain<Texture2D>(swapChain, 0))
            {
                this.renderTargetView = new RenderTargetView(this.Device, backBuffer);
            }

            #endregion

            #region Depth Stencil Buffer and View

            this.depthStencilBuffer = new Texture2D(
                this.Device,
                new Texture2DDescription()
                {
                    Width = width,
                    Height = height,
                    MipLevels = 1,
                    ArraySize = 1,
                    Format = this.DepthFormat,
                    SampleDescription = this.CurrentSampleDescription,
                    Usage = ResourceUsage.Default,
                    BindFlags = BindFlags.DepthStencil,
                    CpuAccessFlags = CpuAccessFlags.None,
                    OptionFlags = ResourceOptionFlags.None,
                });

            this.depthStencilView = new DepthStencilView(
                this.Device,
                this.depthStencilBuffer,
                new DepthStencilViewDescription()
                {
                    Format = this.DepthFormat,
                    Dimension = this.MultiSampled ? DepthStencilViewDimension.Texture2DMultisampled : DepthStencilViewDimension.Texture2D,
                    Texture2D = new DepthStencilViewDescription.Texture2DResource()
                    {
                        MipSlice = 0
                    },
                });

            #endregion

            #region Depth Stencil States

            #region Z-buffer enabled for write depth-stencil state

            this.depthStencilzBufferEnabled = new DepthStencilState(
                this.Device,
                new DepthStencilStateDescription()
                {
                    IsDepthEnabled = true,
                    DepthWriteMask = DepthWriteMask.All,
                    DepthComparison = Comparison.Less,

                    IsStencilEnabled = true,
                    StencilReadMask = 0xFF,
                    StencilWriteMask = 0xFF,

                    FrontFace = new DepthStencilOperationDescription()
                    {
                        FailOperation = StencilOperation.Keep,
                        DepthFailOperation = StencilOperation.Increment,
                        PassOperation = StencilOperation.Keep,
                        Comparison = Comparison.Always,
                    },

                    BackFace = new DepthStencilOperationDescription()
                    {
                        FailOperation = StencilOperation.Keep,
                        DepthFailOperation = StencilOperation.Decrement,
                        PassOperation = StencilOperation.Keep,
                        Comparison = Comparison.Always,
                    },
                });

            #endregion

            #region Z-buffer disabled for write depth-stencil state

            this.depthStencilzBufferDisabled = new DepthStencilState(
                this.Device,
                new DepthStencilStateDescription()
                {
                    IsDepthEnabled = false,
                    DepthWriteMask = DepthWriteMask.Zero,
                    DepthComparison = Comparison.Never,

                    IsStencilEnabled = true,
                    StencilReadMask = 0xFF,
                    StencilWriteMask = 0xFF,

                    FrontFace = new DepthStencilOperationDescription()
                    {
                        FailOperation = StencilOperation.Keep,
                        DepthFailOperation = StencilOperation.Increment,
                        PassOperation = StencilOperation.Keep,
                        Comparison = Comparison.Always,
                    },

                    BackFace = new DepthStencilOperationDescription()
                    {
                        FailOperation = StencilOperation.Keep,
                        DepthFailOperation = StencilOperation.Decrement,
                        PassOperation = StencilOperation.Keep,
                        Comparison = Comparison.Always,
                    },
                });

            #endregion

            #region Z-buffer enabled for read depth-stencil state

            this.depthStencilRDzBufferEnabled = new DepthStencilState(
                this.Device,
                new DepthStencilStateDescription()
                {
                    IsDepthEnabled = true,
                    DepthWriteMask = DepthWriteMask.Zero,
                    DepthComparison = Comparison.Less,
                });

            #endregion

            #region Z-buffer disabled for read depth-stencil state

            this.depthStencilRDzBufferDisabled = new DepthStencilState(
                this.Device,
                new DepthStencilStateDescription()
                {
                    IsDepthEnabled = false,
                    DepthWriteMask = DepthWriteMask.Zero,
                    DepthComparison = Comparison.Never,
                });

            #endregion

            #region No depth, no stencil state

            this.depthStencilNone = new DepthStencilState(
                this.Device,
                new DepthStencilStateDescription()
                {
                    IsDepthEnabled = false,
                    DepthWriteMask = DepthWriteMask.Zero,
                    DepthComparison = Comparison.Never,

                    IsStencilEnabled = false,
                    StencilReadMask = 0xFF,
                    StencilWriteMask = 0xFF,
                });

            #endregion

            #region Depth-stencil state for volume marking (Value != 0 if object is inside of the current drawing volume)

            this.depthStencilVolumeMarking = new DepthStencilState(
                this.Device,
                new DepthStencilStateDescription()
                {
                    IsDepthEnabled = true,
                    DepthWriteMask = DepthWriteMask.Zero,
                    DepthComparison = Comparison.Less,

                    IsStencilEnabled = true,
                    StencilReadMask = 0xFF,
                    StencilWriteMask = 0xFF,

                    FrontFace = new DepthStencilOperationDescription()
                    {
                        FailOperation = StencilOperation.Keep,
                        DepthFailOperation = StencilOperation.Decrement,
                        PassOperation = StencilOperation.Keep,
                        Comparison = Comparison.Always,
                    },

                    BackFace = new DepthStencilOperationDescription()
                    {
                        FailOperation = StencilOperation.Keep,
                        DepthFailOperation = StencilOperation.Increment,
                        PassOperation = StencilOperation.Keep,
                        Comparison = Comparison.Always,
                    },
                });

            #endregion

            #region Depth-stencil state for volume drawing (Process pixels if stencil value != stencil reference)

            this.depthStencilVolumeDrawing = new DepthStencilState(
                this.Device,
                new DepthStencilStateDescription()
                {
                    IsDepthEnabled = false,
                    DepthWriteMask = DepthWriteMask.Zero,
                    DepthComparison = Comparison.Never,

                    IsStencilEnabled = true,
                    StencilReadMask = 0xFF,
                    StencilWriteMask = 0x00,

                    FrontFace = new DepthStencilOperationDescription()
                    {
                        FailOperation = StencilOperation.Keep,
                        DepthFailOperation = StencilOperation.Keep,
                        PassOperation = StencilOperation.Keep,
                        Comparison = Comparison.NotEqual,
                    },

                    BackFace = new DepthStencilOperationDescription()
                    {
                        FailOperation = StencilOperation.Keep,
                        DepthFailOperation = StencilOperation.Keep,
                        PassOperation = StencilOperation.Keep,
                        Comparison = Comparison.NotEqual,
                    },
                });

            #endregion

            #endregion

            #region Rasterizer States

            //Default rasterizer state
            this.rasterizerDefault = new RasterizerState(
                this.Device,
                new RasterizerStateDescription()
                {
                    CullMode = CullMode.Back,
                    FillMode = FillMode.Solid,
                    IsFrontCounterClockwise = false,
                    IsAntialiasedLineEnabled = false,
                    IsMultisampleEnabled = false,
                    IsScissorEnabled = false,
                    IsDepthClipEnabled = true,
                    DepthBias = 0,
                    DepthBiasClamp = 0.0f,
                    SlopeScaledDepthBias = 0.0f,
                });

            //Wireframe rasterizer state
            this.rasterizerWireframe = new RasterizerState(
                this.Device,
                new RasterizerStateDescription()
                {
                    CullMode = CullMode.Back,
                    FillMode = FillMode.Wireframe,
                    IsFrontCounterClockwise = false,
                    IsAntialiasedLineEnabled = true,
                    IsMultisampleEnabled = true,
                    IsScissorEnabled = false,
                    IsDepthClipEnabled = true,
                    DepthBias = 0,
                    DepthBiasClamp = 0.0f,
                    SlopeScaledDepthBias = 0.0f,
                });

            //No cull rasterizer state
            this.rasterizerNoCull = new RasterizerState(
                this.Device,
                new RasterizerStateDescription()
                {
                    CullMode = CullMode.None,
                    FillMode = FillMode.Solid,
                    IsFrontCounterClockwise = false,
                    IsAntialiasedLineEnabled = false,
                    IsMultisampleEnabled = false,
                    IsScissorEnabled = false,
                    IsDepthClipEnabled = true,
                    DepthBias = 0,
                    DepthBiasClamp = 0.0f,
                    SlopeScaledDepthBias = 0.0f,
                });

            //Counter clockwise cull rasterizer state
            this.rasterizerCullFrontFace = new RasterizerState(
                this.Device,
                new RasterizerStateDescription()
                {
                    CullMode = CullMode.Back,
                    FillMode = FillMode.Solid,
                    IsFrontCounterClockwise = true,
                    IsAntialiasedLineEnabled = false,
                    IsMultisampleEnabled = false,
                    IsScissorEnabled = false,
                    IsDepthClipEnabled = true,
                    DepthBias = 0,
                    DepthBiasClamp = 0.0f,
                    SlopeScaledDepthBias = 0.0f,
                });

            //Stencil pass rasterizer state
            this.rasterizerStencilPass = new RasterizerState(
                this.Device,
                new RasterizerStateDescription()
                {
                    CullMode = CullMode.None,
                    FillMode = FillMode.Solid,
                    IsFrontCounterClockwise = false,
                    IsAntialiasedLineEnabled = false,
                    IsMultisampleEnabled = false,
                    IsScissorEnabled = false,
                    IsDepthClipEnabled = false,
                    DepthBias = 0,
                    DepthBiasClamp = 0.0f,
                    SlopeScaledDepthBias = 0.0f,
                });

            //Counter clockwise cull rasterizer state
            this.rasterizerLightingPass = new RasterizerState(
                this.Device,
                new RasterizerStateDescription()
                {
                    CullMode = CullMode.Back,
                    FillMode = FillMode.Solid,
                    IsFrontCounterClockwise = true,
                    IsAntialiasedLineEnabled = false,
                    IsMultisampleEnabled = false,
                    IsScissorEnabled = false,
                    IsDepthClipEnabled = false,
                    DepthBias = 0,
                    DepthBiasClamp = 0.0f,
                    SlopeScaledDepthBias = 0.0f,
                });

            #endregion

            #region Blend States

            #region Default blend state (No alpha)
            {
                BlendStateDescription desc = new BlendStateDescription();
                desc.AlphaToCoverageEnable = false;
                desc.IndependentBlendEnable = false;

                desc.RenderTarget[0].IsBlendEnabled = true;
                desc.RenderTarget[0].RenderTargetWriteMask = ColorWriteMaskFlags.All;

                desc.RenderTarget[0].BlendOperation = BlendOperation.Add;
                desc.RenderTarget[0].SourceBlend = BlendOption.One;
                desc.RenderTarget[0].DestinationBlend = BlendOption.Zero;

                desc.RenderTarget[0].AlphaBlendOperation = BlendOperation.Add;
                desc.RenderTarget[0].SourceAlphaBlend = BlendOption.One;
                desc.RenderTarget[0].DestinationAlphaBlend = BlendOption.Zero;

                this.blendDefault = new BlendState(this.Device, desc);
            }
            #endregion

            #region Alpha blend state
            {
                BlendStateDescription desc = new BlendStateDescription();
                desc.AlphaToCoverageEnable = false;
                desc.IndependentBlendEnable = false;

                desc.RenderTarget[0].IsBlendEnabled = true;
                desc.RenderTarget[0].RenderTargetWriteMask = ColorWriteMaskFlags.All;

                desc.RenderTarget[0].BlendOperation = BlendOperation.Add;
                desc.RenderTarget[0].SourceBlend = BlendOption.SourceAlpha;
                desc.RenderTarget[0].DestinationBlend = BlendOption.InverseSourceAlpha;

                desc.RenderTarget[0].AlphaBlendOperation = BlendOperation.Add;
                desc.RenderTarget[0].SourceAlphaBlend = BlendOption.Zero;
                desc.RenderTarget[0].DestinationAlphaBlend = BlendOption.Zero;

                this.blendDefaultAlpha = new BlendState(this.Device, desc);
            }
            #endregion

            #region Transparent blend state
            {
                BlendStateDescription desc = new BlendStateDescription();
                desc.AlphaToCoverageEnable = true;
                desc.IndependentBlendEnable = false;

                desc.RenderTarget[0].IsBlendEnabled = true;
                desc.RenderTarget[0].RenderTargetWriteMask = ColorWriteMaskFlags.All;

                desc.RenderTarget[0].BlendOperation = BlendOperation.Add;
                desc.RenderTarget[0].SourceBlend = BlendOption.SourceAlpha;
                desc.RenderTarget[0].DestinationBlend = BlendOption.InverseSourceAlpha;

                desc.RenderTarget[0].AlphaBlendOperation = BlendOperation.Add;
                desc.RenderTarget[0].SourceAlphaBlend = BlendOption.Zero;
                desc.RenderTarget[0].DestinationAlphaBlend = BlendOption.Zero;

                this.blendTransparent = new BlendState(this.Device, desc);
            }
            #endregion

            #region Additive blend state
            {
                BlendStateDescription desc = new BlendStateDescription();
                desc.AlphaToCoverageEnable = false;
                desc.IndependentBlendEnable = false;

                desc.RenderTarget[0].IsBlendEnabled = true;
                desc.RenderTarget[0].RenderTargetWriteMask = ColorWriteMaskFlags.All;

                desc.RenderTarget[0].BlendOperation = BlendOperation.Add;
                desc.RenderTarget[0].SourceBlend = BlendOption.SourceAlpha;
                desc.RenderTarget[0].DestinationBlend = BlendOption.One;

                desc.RenderTarget[0].AlphaBlendOperation = BlendOperation.Add;
                desc.RenderTarget[0].SourceAlphaBlend = BlendOption.Zero;
                desc.RenderTarget[0].DestinationAlphaBlend = BlendOption.Zero;

                this.blendAdditive = new BlendState(this.Device, desc);
            }
            #endregion

            #region Deferred composer blend state (no alpha)
            {
                BlendStateDescription desc = new BlendStateDescription();
                desc.AlphaToCoverageEnable = false;
                desc.IndependentBlendEnable = true;

                desc.RenderTarget[0].IsBlendEnabled = true;
                desc.RenderTarget[0].RenderTargetWriteMask = ColorWriteMaskFlags.All;
                desc.RenderTarget[0].BlendOperation = BlendOperation.Add;
                desc.RenderTarget[0].SourceBlend = BlendOption.One;
                desc.RenderTarget[0].DestinationBlend = BlendOption.Zero;
                desc.RenderTarget[0].AlphaBlendOperation = BlendOperation.Add;
                desc.RenderTarget[0].SourceAlphaBlend = BlendOption.One;
                desc.RenderTarget[0].DestinationAlphaBlend = BlendOption.Zero;

                desc.RenderTarget[1].IsBlendEnabled = true;
                desc.RenderTarget[1].RenderTargetWriteMask = ColorWriteMaskFlags.All;
                desc.RenderTarget[1].BlendOperation = BlendOperation.Add;
                desc.RenderTarget[1].SourceBlend = BlendOption.One;
                desc.RenderTarget[1].DestinationBlend = BlendOption.Zero;
                desc.RenderTarget[1].AlphaBlendOperation = BlendOperation.Add;
                desc.RenderTarget[1].SourceAlphaBlend = BlendOption.One;
                desc.RenderTarget[1].DestinationAlphaBlend = BlendOption.Zero;

                desc.RenderTarget[2].IsBlendEnabled = true;
                desc.RenderTarget[2].RenderTargetWriteMask = ColorWriteMaskFlags.All;
                desc.RenderTarget[2].BlendOperation = BlendOperation.Add;
                desc.RenderTarget[2].SourceBlend = BlendOption.One;
                desc.RenderTarget[2].DestinationBlend = BlendOption.Zero;
                desc.RenderTarget[2].AlphaBlendOperation = BlendOperation.Add;
                desc.RenderTarget[2].SourceAlphaBlend = BlendOption.One;
                desc.RenderTarget[2].DestinationAlphaBlend = BlendOption.Zero;

                this.blendDeferredComposer = new BlendState(this.Device, desc);
            }
            #endregion

            #region Deferred composer transparent blend state
            {
                BlendStateDescription desc = new BlendStateDescription();
                desc.AlphaToCoverageEnable = true;
                desc.IndependentBlendEnable = true;

                //Transparent blending only in first buffer
                desc.RenderTarget[0].IsBlendEnabled = true;
                desc.RenderTarget[0].RenderTargetWriteMask = ColorWriteMaskFlags.All;
                desc.RenderTarget[0].BlendOperation = BlendOperation.Add;
                desc.RenderTarget[0].SourceBlend = BlendOption.SourceAlpha;
                desc.RenderTarget[0].DestinationBlend = BlendOption.InverseSourceAlpha;
                desc.RenderTarget[0].AlphaBlendOperation = BlendOperation.Add;
                desc.RenderTarget[0].SourceAlphaBlend = BlendOption.One;
                desc.RenderTarget[0].DestinationAlphaBlend = BlendOption.Zero;

                desc.RenderTarget[1].IsBlendEnabled = true;
                desc.RenderTarget[1].RenderTargetWriteMask = ColorWriteMaskFlags.All;
                desc.RenderTarget[1].BlendOperation = BlendOperation.Add;
                desc.RenderTarget[1].SourceBlend = BlendOption.One;
                desc.RenderTarget[1].DestinationBlend = BlendOption.Zero;
                desc.RenderTarget[1].AlphaBlendOperation = BlendOperation.Add;
                desc.RenderTarget[1].SourceAlphaBlend = BlendOption.One;
                desc.RenderTarget[1].DestinationAlphaBlend = BlendOption.Zero;

                desc.RenderTarget[2].IsBlendEnabled = true;
                desc.RenderTarget[2].RenderTargetWriteMask = ColorWriteMaskFlags.All;
                desc.RenderTarget[2].BlendOperation = BlendOperation.Add;
                desc.RenderTarget[2].SourceBlend = BlendOption.One;
                desc.RenderTarget[2].DestinationBlend = BlendOption.Zero;
                desc.RenderTarget[2].AlphaBlendOperation = BlendOperation.Add;
                desc.RenderTarget[2].SourceAlphaBlend = BlendOption.One;
                desc.RenderTarget[2].DestinationAlphaBlend = BlendOption.Zero;

                this.blendDeferredComposerTransparent = new BlendState(this.Device, desc);
            }
            #endregion

            #region Deferred lighting blend state
            {
                BlendStateDescription desc = new BlendStateDescription();
                desc.AlphaToCoverageEnable = false;
                desc.IndependentBlendEnable = false;

                desc.RenderTarget[0].IsBlendEnabled = true;
                desc.RenderTarget[0].RenderTargetWriteMask = ColorWriteMaskFlags.All;
                desc.RenderTarget[0].BlendOperation = BlendOperation.Add;
                desc.RenderTarget[0].SourceBlend = BlendOption.One;
                desc.RenderTarget[0].DestinationBlend = BlendOption.One;
                desc.RenderTarget[0].AlphaBlendOperation = BlendOperation.Add;
                desc.RenderTarget[0].SourceAlphaBlend = BlendOption.One;
                desc.RenderTarget[0].DestinationAlphaBlend = BlendOption.One;

                this.blendDeferredLighting = new BlendState(this.Device, desc);
            }
            #endregion

            #endregion

            #region Set Defaults

            this.SetDefaultViewport();
            this.SetDefaultRenderTarget();

            this.SetDepthStencilZEnabled();
            this.SetRasterizerDefault();
            this.SetBlendDefault();

            #endregion

            if (resizing)
            {
                if (this.Resized != null)
                {
                    //Launch the "resized" event
                    this.Resized(this, new EventArgs());
                }
            }
        }

        /// <summary>
        /// Begin frame
        /// </summary>
        public void Begin()
        {
            this.DeviceContext.ClearDepthStencilView(
                this.depthStencilView,
                DepthStencilClearFlags.Depth | DepthStencilClearFlags.Stencil,
                1.0f,
                0);

            this.DeviceContext.ClearRenderTargetView(
                this.renderTargetView,
                GameEnvironment.Background);
        }
        /// <summary>
        /// End frame
        /// </summary>
        public void End()
        {
            if (this.vsyncEnabled)
            {
                this.swapChain.Present(1, PresentFlags.None);
            }
            else
            {
                this.swapChain.Present(0, PresentFlags.None);
            }
        }
        /// <summary>
        /// Sets the default viewport
        /// </summary>
        public void SetDefaultViewport()
        {
            this.SetViewport(this.Viewport);
        }
        /// <summary>
        /// Sets default render target
        /// </summary>
        /// <param name="clear">Indicates whether the target and stencil buffer must be cleared</param>
        public void SetDefaultRenderTarget(bool clear = true)
        {
            this.SetRenderTarget(this.renderTargetView, clear, GameEnvironment.Background, this.depthStencilView, clear);
        }
        /// <summary>
        /// Sets viewport
        /// </summary>
        /// <param name="viewport">Viewport</param>
        public void SetViewport(Viewport viewport)
        {
            this.DeviceContext.Rasterizer.SetViewport(viewport);
        }
        /// <summary>
        /// Sets viewport
        /// </summary>
        /// <param name="viewport">Viewport</param>
        public void SetViewport(ViewportF viewport)
        {
            this.DeviceContext.Rasterizer.SetViewport(viewport);
        }
        /// <summary>
        /// Set render target
        /// </summary>
        /// <param name="renderTarget">Render target</param>
        /// <param name="clearRT">Indicates whether the target must be cleared</param>
        /// <param name="clearRTColor">Target clear color</param>
        /// <param name="depthMap">Depth map</param>
        /// <param name="clearDS">Indicates whether the stencil buffer must be cleared</param>
        /// <param name="clearDSFlags">Stencil cleraring flags</param>
        public void SetRenderTarget(RenderTargetView renderTarget, bool clearRT, Color4 clearRTColor, DepthStencilView depthMap, bool clearDS, DepthStencilClearFlags clearDSFlags = DepthStencilClearFlags.Depth | DepthStencilClearFlags.Stencil)
        {
            this.DeviceContext.OutputMerger.SetTargets(depthMap, renderTarget);

            if (renderTarget != null && clearRT)
            {
                this.DeviceContext.ClearRenderTargetView(
                    renderTarget,
                    clearRTColor);
            }

            if (depthMap != null && clearDS)
            {
                this.DeviceContext.ClearDepthStencilView(
                    depthMap,
                    clearDSFlags,
                    1.0f, 0);
            }
        }
        /// <summary>
        /// Set render targets
        /// </summary>
        /// <param name="renderTarget">Render target</param>
        /// <param name="renderTargets">Render targets</param>
        /// <param name="clearRT">Indicates whether the target must be cleared</param>
        /// <param name="clearRTColor">Target clear color</param>
        /// <param name="depthMap">Depth map</param>
        /// <param name="clearDS">Indicates whether the stencil buffer must be cleared</param>
        /// <param name="clearDSFlags">Stencil cleraring flags</param>
        public void SetRenderTargets(RenderTargetView[] renderTargets, bool clearRT, Color4 clearRTColor, DepthStencilView depthMap, bool clearDS, DepthStencilClearFlags clearDSFlags = DepthStencilClearFlags.Depth | DepthStencilClearFlags.Stencil)
        {
            this.DeviceContext.OutputMerger.SetTargets(depthMap, renderTargets.Length, renderTargets);

            if (clearRT && renderTargets != null && renderTargets.Length > 0)
            {
                for (int i = 0; i < renderTargets.Length; i++)
                {
                    this.DeviceContext.ClearRenderTargetView(
                        renderTargets[i],
                        clearRTColor);
                }
            }

            if (clearDS && depthMap != null)
            {
                this.DeviceContext.ClearDepthStencilView(
                    depthMap,
                    clearDSFlags,
                    1.0f, 0);
            }
        }
        /// <summary>
        /// Clear depth / stencil buffer
        /// </summary>
        /// <param name="depthMap">Depth buffer</param>
        /// <param name="clearDSFlags">Clear flags</param>
        public void ClearDepthStencilBuffer(DepthStencilView depthMap, DepthStencilClearFlags clearDSFlags = DepthStencilClearFlags.Depth | DepthStencilClearFlags.Stencil)
        {
            this.DeviceContext.ClearDepthStencilView(
                depthMap,
                clearDSFlags,
                1.0f, 0);
        }
        /// <summary>
        /// Enables z-buffer for write
        /// </summary>
        public void SetDepthStencilZEnabled()
        {
            this.SetDepthStencilState(this.depthStencilzBufferEnabled);
        }
        /// <summary>
        /// Disables z-buffer for write
        /// </summary>
        public void SetDepthStencilZDisabled()
        {
            this.SetDepthStencilState(this.depthStencilzBufferDisabled);
        }
        /// <summary>
        /// Enables z-buffer for read
        /// </summary>
        public void SetDepthStencilRDZEnabled()
        {
            this.SetDepthStencilState(this.depthStencilRDzBufferEnabled);
        }
        /// <summary>
        /// Disables z-buffer for read
        /// </summary>
        public void SetDepthStencilRDZDisabled()
        {
            this.SetDepthStencilState(this.depthStencilRDzBufferDisabled);
        }
        /// <summary>
        /// Disables depth stencil
        /// </summary>
        public void SetDepthStencilNone()
        {
            this.SetDepthStencilState(this.depthStencilNone);
        }
        /// <summary>
        /// Sets depth stencil for volume marking
        /// </summary>
        public void SetDepthStencilVolumeMarking()
        {
            this.SetDepthStencilState(this.depthStencilVolumeMarking);
        }
        /// <summary>
        /// Sets depth stencil for volume drawing
        /// </summary>
        public void SetDepthStencilVolumeDrawing(int stencilRef)
        {
            this.SetDepthStencilState(this.depthStencilVolumeDrawing, stencilRef);
        }
        /// <summary>
        /// Sets default blend state
        /// </summary>
        public void SetBlendDefault()
        {
            this.SetBlendState(this.blendDefault, Color.Transparent, -1);
        }
        /// <summary>
        /// Sets default alpha blend state
        /// </summary>
        public void SetBlendDefaultAlpha()
        {
            this.SetBlendState(this.blendDefaultAlpha, Color.Transparent, -1);
        }
        /// <summary>
        /// Sets transparent blend state
        /// </summary>
        public void SetBlendTransparent()
        {
            this.SetBlendState(this.blendTransparent, Color.Transparent, -1);
        }
        /// <summary>
        /// Sets additive blend state
        /// </summary>
        public void SetBlendAdditive()
        {
            this.SetBlendState(this.blendAdditive, Color.Transparent, -1);
        }
        /// <summary>
        /// Sets deferred composer blend state
        /// </summary>
        public void SetBlendDeferredComposer()
        {
            this.SetBlendState(this.blendDeferredComposer, Color.Transparent, -1);
        }
        /// <summary>
        /// Sets transparent deferred composer blend state
        /// </summary>
        public void SetBlendDeferredComposerTransparent()
        {
            this.SetBlendState(this.blendDeferredComposerTransparent, Color.Transparent, -1);
        }
        /// <summary>
        /// Sets deferred lighting blend state
        /// </summary>
        public void SetBlendDeferredLighting()
        {
            this.SetBlendState(this.blendDeferredLighting, Color.Transparent, -1);
        }
        /// <summary>
        /// Sets default rasterizer
        /// </summary>
        public void SetRasterizerDefault()
        {
            this.SetRasterizerState(this.rasterizerDefault);
        }
        /// <summary>
        /// Sets wireframe rasterizer
        /// </summary>
        public void SetRasterizerWireframe()
        {
            this.SetRasterizerState(this.rasterizerWireframe);
        }
        /// <summary>
        /// Sets no-cull rasterizer
        /// </summary>
        public void SetRasterizerCullNone()
        {
            this.SetRasterizerState(this.rasterizerNoCull);
        }
        /// <summary>
        /// Sets cull counter-clockwise face rasterizer
        /// </summary>
        public void SetRasterizerCullFrontFace()
        {
            this.SetRasterizerState(this.rasterizerCullFrontFace);
        }
        /// <summary>
        /// Sets stencil pass rasterizer
        /// </summary>
        public void SetRasterizerStencilPass()
        {
            this.SetRasterizerState(this.rasterizerStencilPass);
        }
        /// <summary>
        /// Stes lighting pass rasterizer
        /// </summary>
        public void SetRasterizerLightingPass()
        {
            this.SetRasterizerState(this.rasterizerLightingPass);
        }
        /// <summary>
        /// Bind an array of vertex buffers to the input-assembler stage.
        /// </summary>
        /// <param name="firstSlot">The first input slot for binding</param>
        /// <param name="vertexBufferBindings">A reference to an array of SharpDX.Direct3D11.VertexBufferBinding</param>
        public void IASetVertexBuffers(int firstSlot, params VertexBufferBinding[] vertexBufferBindings)
        {
            if (this.currentVertexBufferFirstSlot != firstSlot || this.currentVertexBufferBindings != vertexBufferBindings)
            {
                this.DeviceContext.InputAssembler.SetVertexBuffers(firstSlot, vertexBufferBindings);
                Counters.IAVertexBuffersSets++;

                this.currentVertexBufferFirstSlot = firstSlot;
                this.currentVertexBufferBindings = vertexBufferBindings;
            }
        }
        /// <summary>
        /// Bind an index buffer to the input-assembler stage.
        /// </summary>
        /// <param name="indexBufferRef">A reference to an SharpDX.Direct3D11.Buffer object</param>
        /// <param name="format">A SharpDX.DXGI.Format that specifies the format of the data in the index buffer</param>
        /// <param name="offset">Offset (in bytes) from the start of the index buffer to the first index to use</param>
        public void IASetIndexBuffer(Buffer indexBufferRef, Format format, int offset)
        {
            if (this.currentIndexBufferRef != indexBufferRef || this.currentIndexFormat != format || this.currentIndexOffset != offset)
            {
                this.DeviceContext.InputAssembler.SetIndexBuffer(indexBufferRef, format, offset);
                Counters.IAIndexBufferSets++;

                this.currentIndexBufferRef = indexBufferRef;
                this.currentIndexFormat = format;
                this.currentIndexOffset = offset;
            }
        }
        /// <summary>
        /// Gets or sets the input assembler's primitive topology
        /// </summary>
        public PrimitiveTopology IAPrimitiveTopology
        {
            get
            {
                return this.currentIAPrimitiveTopology;
            }
            set
            {
                if (this.currentIAPrimitiveTopology != value)
                {
                    this.DeviceContext.InputAssembler.PrimitiveTopology = value;
                    Counters.IAPrimitiveTopologySets++;

                    this.currentIAPrimitiveTopology = value;
                }
            }
        }
        /// <summary>
        /// Gets or sets the input assembler's input layout
        /// </summary>
        public InputLayout IAInputLayout
        {
            get
            {
                return this.currentIAInputLayout;
            }
            set
            {
                if (this.currentIAInputLayout != value)
                {
                    this.DeviceContext.InputAssembler.InputLayout = value;
                    Counters.IAInputLayoutSets++;

                    this.currentIAInputLayout = value;
                }
            }
        }
        /// <summary>
        /// Dispose created resources
        /// </summary>
        public void Dispose()
        {
            if (this.swapChain != null)
            {
                if (this.swapChain.IsFullScreen) this.swapChain.IsFullScreen = false;

                Helper.Dispose(this.swapChain);
            }

            this.DisposeResources();

            Helper.Dispose(this.Device);
        }

        /// <summary>
        /// Sets depth stencil state
        /// </summary>
        /// <param name="state">Depth stencil state</param>
        private void SetDepthStencilState(DepthStencilState state, int stencilRef = 0)
        {
            if (this.currentDepthStencilState != state || this.currentDepthStencilStateRef != stencilRef)
            {
                this.Device.ImmediateContext.OutputMerger.SetDepthStencilState(state, stencilRef);

                this.currentDepthStencilState = state;
                this.currentDepthStencilStateRef = stencilRef;

                Counters.DepthStencilStateChanges++;
            }
        }
        /// <summary>
        /// Stes blend state
        /// </summary>
        /// <param name="state">Blend state</param>
        /// <param name="blendFactor">Blend factor</param>
        /// <param name="sampleMask">Sample mask</param>
        private void SetBlendState(BlendState state, Color4? blendFactor = null, int sampleMask = -1)
        {
            if (this.currentBlendState != state)
            {
                this.Device.ImmediateContext.OutputMerger.SetBlendState(state, blendFactor, sampleMask);

                this.currentBlendState = state;

                Counters.BlendStateChanges++;
            }
        }
        /// <summary>
        /// Sets rasterizer state
        /// </summary>
        /// <param name="state">Rasterizer state</param>
        private void SetRasterizerState(RasterizerState state)
        {
            if (this.currentRasterizerState != state)
            {
                this.Device.ImmediateContext.Rasterizer.State = state;

                this.currentRasterizerState = state;

                Counters.RasterizerStateChanges++;
            }
        }

        /// <summary>
        /// Checks the multi-sample specified count
        /// </summary>
        /// <param name="tmpDevice">Temporary device</param>
        /// <param name="multiSampling">Multi-sample count</param>
        /// <param name="sampleCount">Sample count</param>
        /// <param name="maxQualityLevel">Maximum quality level</param>
        private void CheckMultisample(Device tmpDevice, int multiSampling, out int sampleCount, out int maxQualityLevel)
        {
            sampleCount = 1;
            maxQualityLevel = 0;
            int maxQuality = tmpDevice.CheckMultisampleQualityLevels(this.BufferFormat, multiSampling);
            if (maxQuality > 0)
            {
                sampleCount = multiSampling;
                maxQualityLevel = maxQuality - 1;
            }
        }
        /// <summary>
        /// Checks the multi-sample maximum quality
        /// </summary>
        /// <param name="tmpDevice">Temporary device</param>
        /// <param name="sampleCount">Sample count</param>
        /// <param name="maxQualityLevel">Maximum quality level</param>
        private void CheckMultisample(Device tmpDevice, out int sampleCount, out int maxQualityLevel)
        {
            sampleCount = 1;
            maxQualityLevel = 0;
            for (int count = 1; count <= Device.MultisampleCountMaximum; count++)
            {
                int maxQuality = tmpDevice.CheckMultisampleQualityLevels(this.BufferFormat, count);
                if (maxQuality > 0)
                {
                    sampleCount = count;
                    maxQualityLevel = maxQuality - 1;
                }
            }
        }
        /// <summary>
        /// Finds mode description
        /// </summary>
        /// <param name="format">Format</param>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        /// <param name="fullScreen">True for full screen modes</param>
        /// <param name="refreshRate">Refresh date</param>
        /// <param name="adapter">Selected adapter</param>
        /// <returns>Returns found mode description</returns>
        private ModeDescription FindModeDescription(Format format, int width, int height, bool fullScreen, int refreshRate, out Adapter1 adapter)
        {
            adapter = null;

            using (Factory1 factory = new Factory1())
            {
                adapter = factory.GetAdapter1(0);

                using (Adapter1 firstAdapter = factory.GetAdapter1(0))
                {
                    using (Output adapterOutput = firstAdapter.GetOutput(0))
                    {
                        try
                        {
                            var displayModeList = adapterOutput.GetDisplayModeList(
                                format,
                                DisplayModeEnumerationFlags.Interlaced);

                            displayModeList = Array.FindAll(displayModeList, d => d.Width == width && d.Height == height);
                            if (displayModeList.Length > 0)
                            {
                                if (refreshRate > 0)
                                {
                                    Array.Sort(displayModeList, (d1, d2) =>
                                    {
                                        float f1 = (float)d1.RefreshRate.Numerator / (float)d1.RefreshRate.Denominator;
                                        float f2 = (float)d2.RefreshRate.Numerator / (float)d2.RefreshRate.Denominator;

                                        f1 = Math.Abs(refreshRate - f1);
                                        f2 = Math.Abs(refreshRate - f2);

                                        return f1.CompareTo(f2);
                                    });
                                }
                                else
                                {
                                    Array.Sort(displayModeList, (d1, d2) =>
                                    {
                                        float f1 = (float)d1.RefreshRate.Numerator / (float)d1.RefreshRate.Denominator;
                                        float f2 = (float)d2.RefreshRate.Numerator / (float)d2.RefreshRate.Denominator;

                                        return f2.CompareTo(f1);
                                    });
                                }

                                return displayModeList[0];
                            }
                        }
                        catch
                        {

                        }

                        try
                        {
                            ModeDescription result;
                            adapterOutput.GetClosestMatchingMode(
                                null,
                                new ModeDescription()
                                {
                                    Format = format,
                                    Width = width,
                                    Height = height,
                                },
                                out result);

                            result.Width = width;
                            result.Height = height;

                            return result;
                        }
                        catch
                        {

                        }
                    }
                }
            }

            return new ModeDescription()
            {
                Width = width,
                Height = height,
                Format = format,
                RefreshRate = new Rational(0, 1),
                Scaling = DisplayModeScaling.Unspecified,
                ScanlineOrdering = DisplayModeScanlineOrder.Unspecified,
            };
        }
        /// <summary>
        /// Dispose resources
        /// </summary>
        private void DisposeResources()
        {
            Helper.Dispose(this.renderTargetView);
            Helper.Dispose(this.depthStencilBuffer);
            Helper.Dispose(this.depthStencilView);

            Helper.Dispose(this.depthStencilzBufferEnabled);
            Helper.Dispose(this.depthStencilzBufferDisabled);
            Helper.Dispose(this.depthStencilRDzBufferEnabled);
            Helper.Dispose(this.depthStencilRDzBufferDisabled);
            Helper.Dispose(this.depthStencilNone);
            Helper.Dispose(this.depthStencilVolumeMarking);
            Helper.Dispose(this.depthStencilVolumeDrawing);

            Helper.Dispose(this.rasterizerDefault);
            Helper.Dispose(this.rasterizerWireframe);
            Helper.Dispose(this.rasterizerNoCull);
            Helper.Dispose(this.rasterizerCullFrontFace);
            Helper.Dispose(this.rasterizerStencilPass);
            Helper.Dispose(this.rasterizerLightingPass);

            Helper.Dispose(this.blendDefault);
            Helper.Dispose(this.blendDefaultAlpha);
            Helper.Dispose(this.blendTransparent);
            Helper.Dispose(this.blendAdditive);
            Helper.Dispose(this.blendDeferredLighting);
            Helper.Dispose(this.blendDeferredComposer);
            Helper.Dispose(this.blendDeferredComposerTransparent);
        }
    }
}
