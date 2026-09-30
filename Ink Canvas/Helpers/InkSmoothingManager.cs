using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Ink;
using System.Windows.Media;
using System.Windows.Threading;

namespace Ink_Canvas.Helpers
{
    /// <summary>
    /// 统一的墨迹平滑管理器，整合异步处理和硬件加速
    /// </summary>
    public class InkSmoothingManager : IDisposable
    {
        private readonly AsyncAdvancedBezierSmoothing _asyncSmoothing;
        private readonly HardwareAcceleratedInkProcessor _hardwareProcessor;
        private readonly InkSmoothingConfig _config;
        private readonly Dispatcher _uiDispatcher;
        private bool _disposed;

        public InkSmoothingManager(Dispatcher uiDispatcher)
        {
            _uiDispatcher = uiDispatcher;
            _config = InkSmoothingConfig.FromSettings();
            _config.ApplyQualitySettings();

            _asyncSmoothing = new AsyncAdvancedBezierSmoothing(uiDispatcher)
            {
                SmoothingStrength = _config.SmoothingStrength,
                ResampleInterval = _config.ResampleInterval,
                InterpolationSteps = _config.InterpolationSteps,
                UseHardwareAcceleration = _config.UseHardwareAcceleration,
                MaxConcurrentTasks = _config.MaxConcurrentTasks,
                UseAdaptiveInterpolation = _config.UseAdaptiveInterpolation,
                CurveTension = _config.CurveTension
            };

            _hardwareProcessor = new HardwareAcceleratedInkProcessor();
        }

        /// <summary>
        /// 把 <paramref name="source"/> 的全部 property data 复制到 <paramref name="target"/>。
        /// 平滑器（AsyncAdvancedBezierSmoothing / HardwareAcceleratedInkProcessor / AdvancedBezierSmoothing）
        /// 创建的新 Stroke 只克隆 DrawingAttributes，会丢失 LaserRenderModeGuid 等标记——
        /// 激光笔迹会因此失去激光渲染效果。
        /// 必须在 UI 线程调用（property data 值可能是 DispatcherObject）。
        /// </summary>
        public static void CopyPropertyData(Stroke source, Stroke target)
        {
            if (source == null || target == null || ReferenceEquals(source, target))
                return;

            foreach (var id in source.GetPropertyDataIds())
            {
                try
                {
                    if (!target.ContainsPropertyData(id))
                        target.AddPropertyData(id, source.GetPropertyData(id));
                }
                catch
                {
                    // 个别 property data 值无法复制（如 DispatcherObject），跳过不阻断替换。
                }
            }
        }

        /// <summary>
        /// 平滑笔画（自动选择最佳方法）
        /// </summary>
        public async Task<Stroke> SmoothStrokeAsync(Stroke originalStroke,
            Action<Stroke, Stroke> onCompleted = null,
            CancellationToken cancellationToken = default)
        {
            if (originalStroke == null || originalStroke.StylusPoints.Count < 2)
                return originalStroke;

            Stroke result = originalStroke;

            try
            {
                if (_config.UseAsyncProcessing)
                {
                    // 使用异步处理
                    result = await _asyncSmoothing.SmoothStrokeAsync(originalStroke, onCompleted, cancellationToken);
                }
                else if (_config.UseHardwareAcceleration)
                {
                    // 使用硬件加速但同步处理
                    result = await _hardwareProcessor.SmoothStrokeWithGPU(originalStroke);
                    onCompleted?.Invoke(originalStroke, result);
                }
                else
                {
                    // 回退到传统同步处理
                    result = await Task.Run(() =>
                    {
                        var traditionalSmoothing = new AdvancedBezierSmoothing();
                        return traditionalSmoothing.SmoothStroke(originalStroke);
                    }, cancellationToken);
                    onCompleted?.Invoke(originalStroke, result);
                }
            }
            catch (OperationCanceledException)
            {
                result = originalStroke;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"墨迹平滑失败: {ex.Message}");
                result = originalStroke;
            }

            return result;
        }

        /// <summary>
        /// 同步平滑笔画（用于向后兼容）
        /// </summary>
        public Stroke SmoothStroke(Stroke originalStroke)
        {
            if (originalStroke == null || originalStroke.StylusPoints.Count < 2)
                return originalStroke;

            Stroke result;

            try
            {
                if (_config.UseHardwareAcceleration)
                {
                    // GPU 平滑为异步任务，此处是同步 API 的向后兼容入口；
                    // 用带超时的 GetAwaiter().GetResult() 有界等待，超时/失败/取消回退原始笔画。
                    // 注意：调用方在 UI 线程上时仍会发生 sync-over-async 阻塞，调用方应优先走 SmoothStrokeAsync。
                    var task = _hardwareProcessor.SmoothStrokeWithGPU(originalStroke);
                    using (var cts = new CancellationTokenSource(5000))
                    {
                        try
                        {
                            result = task.WaitAsync(cts.Token).GetAwaiter().GetResult();
                        }
                        catch (TimeoutException)
                        {
                            LogHelper.WriteLogToFile("墨迹平滑超时，返回原始笔画", LogHelper.LogType.Warning);
                            result = originalStroke;
                        }
                        catch (OperationCanceledException)
                        {
                            Debug.WriteLine("同步墨迹平滑被取消，返回原始笔画");
                            result = originalStroke;
                        }
                        catch (Exception hwEx)
                        {
                            Debug.WriteLine($"硬件加速平滑失败，回退原始笔画: {hwEx.Message}");
                            result = originalStroke;
                        }
                    }
                }
                else
                {
                    // 传统同步处理
                    var traditionalSmoothing = new AdvancedBezierSmoothing();
                    result = traditionalSmoothing.SmoothStroke(originalStroke);
                }
            }
            catch (AggregateException ex) when (ex.InnerException is OperationCanceledException)
            {
                Debug.WriteLine($"同步墨迹平滑被取消: {ex.InnerException.Message}");
                result = originalStroke;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"同步墨迹平滑失败: {ex.Message}");
                result = originalStroke;
            }

            return result;
        }

        /// <summary>
        /// 更新配置
        /// </summary>
        public void UpdateConfig()
        {
            var newConfig = InkSmoothingConfig.FromSettings();
            newConfig.ApplyQualitySettings();

            _asyncSmoothing.SmoothingStrength = newConfig.SmoothingStrength;
            _asyncSmoothing.ResampleInterval = newConfig.ResampleInterval;
            _asyncSmoothing.InterpolationSteps = newConfig.InterpolationSteps;
            _asyncSmoothing.UseHardwareAcceleration = newConfig.UseHardwareAcceleration;
            _asyncSmoothing.MaxConcurrentTasks = newConfig.MaxConcurrentTasks;
            _asyncSmoothing.UseAdaptiveInterpolation = newConfig.UseAdaptiveInterpolation;
            _asyncSmoothing.CurveTension = newConfig.CurveTension;
        }

        /// <summary>
        /// 取消所有正在进行的任务
        /// </summary>
        public void CancelAllTasks()
        {
            _asyncSmoothing?.CancelAllTasks();
        }

        /// <summary>
        /// 检查系统是否支持硬件加速
        /// </summary>
        public static bool IsHardwareAccelerationSupported()
        {
            try
            {
                return RenderCapability.Tier >= 0x00020000;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 获取推荐的配置
        /// </summary>
        public static InkSmoothingConfig GetRecommendedConfig()
        {
            var config = new InkSmoothingConfig();

            // 根据系统性能调整配置
            var processorCount = Environment.ProcessorCount;
            var isHardwareAccelerated = IsHardwareAccelerationSupported();

            if (processorCount >= 4 && isHardwareAccelerated)
            {
                // 降低高质量模式的门槛，4核以上且支持硬件加速就使用高质量
                config.Quality = (InkSmoothingConfig.SmoothingQuality)InkSmoothingConfig.InkSmoothingQuality.HighQuality;
                config.UseHardwareAcceleration = true;
                config.UseAsyncProcessing = true;
                config.MaxConcurrentTasks = Math.Min(processorCount, 8);
            }
            else if (processorCount >= 2)
            {
                // 2核以上使用平衡模式
                config.Quality = (InkSmoothingConfig.SmoothingQuality)InkSmoothingConfig.InkSmoothingQuality.Balanced;
                config.UseHardwareAcceleration = isHardwareAccelerated;
                config.UseAsyncProcessing = true;
                config.MaxConcurrentTasks = Math.Min(processorCount, 4);
            }
            else
            {
                // 单核或性能较低的设备使用高性能模式
                config.Quality = (InkSmoothingConfig.SmoothingQuality)InkSmoothingConfig.InkSmoothingQuality.HighPerformance;
                config.UseHardwareAcceleration = false;
                config.UseAsyncProcessing = false;
                config.MaxConcurrentTasks = 1;
            }

            config.ApplyQualitySettings();
            return config;
        }

        /// <summary>
        /// 应用推荐配置到设置
        /// </summary>
        public static void ApplyRecommendedSettings()
        {
            var config = GetRecommendedConfig();

            MainWindow.Settings.Canvas.InkSmoothingQuality = (int)config.Quality;
            MainWindow.Settings.Canvas.UseHardwareAcceleration = config.UseHardwareAcceleration;
            MainWindow.Settings.Canvas.UseAsyncInkSmoothing = config.UseAsyncProcessing;
            MainWindow.Settings.Canvas.MaxConcurrentSmoothingTasks = config.MaxConcurrentTasks;
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                CancelAllTasks();
                _asyncSmoothing?.Dispose();
                _hardwareProcessor?.Dispose();
                _disposed = true;
            }
        }
    }

    /// <summary>
    /// 墨迹平滑事件参数
    /// </summary>
    public class InkSmoothingEventArgs : EventArgs
    {
        public Stroke OriginalStroke { get; set; }
        public Stroke SmoothedStroke { get; set; }
        public TimeSpan ProcessingTime { get; set; }
        public bool WasAsync { get; set; }
        public bool UsedHardwareAcceleration { get; set; }
    }
}
