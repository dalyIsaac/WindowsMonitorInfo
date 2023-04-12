using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Windows.Foundation;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;
using Windows.Win32.UI.HiDpi;
using Windows.Win32.UI.Shell.Common;

namespace WindowsMonitorInfo;

public sealed partial class MainWindow : Window
{
    internal ObservableCollection<MonitorInfo> MonitorInfo { get; } = new();

    public MainWindow()
    {
        this.InitializeComponent();
        GetAllMonitorInfo();
    }

    private void Refresh_Click(object sender, RoutedEventArgs e)
    {
        GetAllMonitorInfo();
    }

    private unsafe void GetAllMonitorInfo()
    {
        MonitorEnumCallback callback = new();
        MONITORENUMPROC proc = new(callback.Callback);

        PInvoke.EnumDisplayMonitors(null, null, proc, 0);

        MonitorInfo.Clear();

        foreach (HMONITOR monitor in callback.Monitors)
        {
            MONITORINFOEXW infoEx = new() { monitorInfo = new MONITORINFO() { cbSize = (uint)sizeof(MONITORINFOEXW) } };
            PInvoke.GetMonitorInfo(monitor, (MONITORINFO*)&infoEx);

            // Effective DPI
            HRESULT effectiveDpiResult = PInvoke.GetDpiForMonitor(monitor, MONITOR_DPI_TYPE.MDT_EFFECTIVE_DPI, out uint effectiveDpiX, out uint effectiveDpiY);
            if (!effectiveDpiResult.Succeeded)
            {
                throw new Exception($"GetDpiForMonitor failed with HRESULT {effectiveDpiResult}");
            }

            // Angular DPI
            HRESULT angularDpiResult = PInvoke.GetDpiForMonitor(monitor, MONITOR_DPI_TYPE.MDT_ANGULAR_DPI, out uint angularDpiX, out uint angularDpiY);
            if (!angularDpiResult.Succeeded)
            {
                throw new Exception($"GetDpiForMonitor failed with HRESULT {angularDpiResult}");
            }

            // Raw DPI
            HRESULT rawDpiResult = PInvoke.GetDpiForMonitor(monitor, MONITOR_DPI_TYPE.MDT_RAW_DPI, out uint rawDpiX, out uint rawDpiY);
            if (!rawDpiResult.Succeeded)
            {
                throw new Exception($"GetDpiForMonitor failed with HRESULT {rawDpiResult}");
            }

            // Resolution
            DEVMODEW dm = new() { dmSize = (ushort)sizeof(DEVMODEW), dmDriverExtra = 0 };
            BOOL resolutionResult = PInvoke.EnumDisplaySettings(infoEx.szDevice.ToString(), ENUM_DISPLAY_SETTINGS_MODE.ENUM_CURRENT_SETTINGS, ref dm);
            if (!resolutionResult)
            {
                throw new Exception($"EnumDisplaySettings failed with HRESULT {resolutionResult}");
            }

            // Reported scale factor
            HRESULT scaleFactorResult = PInvoke.GetScaleFactorForMonitor(monitor, out DEVICE_SCALE_FACTOR reportedScaleFactor);
            if (!scaleFactorResult.Succeeded)
            {
                throw new Exception($"GetScaleFactorResult failed with HRESULT {scaleFactorResult}");
            }

            MonitorInfo.Add(new MonitorInfo(
                infoEx.szDevice.ToString(),
                infoEx.monitorInfo.rcMonitor,
                infoEx.monitorInfo.rcWork,
                new Point(effectiveDpiX, effectiveDpiY),
                new Point(angularDpiX, angularDpiY),
                new Point(rawDpiX, rawDpiY),
                new Point(dm.dmPelsWidth, dm.dmPelsHeight),
                reportedScaleFactor
            ));
        }
    }
}

internal class MonitorEnumCallback
{
    public List<HMONITOR> Monitors { get; } = new();

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter")]
    public unsafe BOOL Callback(HMONITOR monitor, HDC _hdc, RECT* _rect, LPARAM _param)
    {
        Monitors.Add(monitor);
        return (BOOL)true;
    }
}

internal class MonitorInfo
{
    public string SzDevice { get; }
    public string MonitorRect { get; }
    public string WorkRect { get; }
    public string EffectiveDpi { get; }
    public string AngularDpi { get; }
    public string RawDpi { get; }
    public string Resolution { get; }
    public int ReportedScale { get; }
    public double CalculatedScale { get; }

    public MonitorInfo(
        string szDevice,
        RECT monitorRect,
        RECT workRect,
        Point effectiveDpi,
        Point angularDpi,
        Point rawDpi,
        Point resolution,
        DEVICE_SCALE_FACTOR reportedScale)
    {
        SzDevice = szDevice;
        MonitorRect = $"({monitorRect.left}, {monitorRect.top}, {monitorRect.right}, {monitorRect.bottom})";
        WorkRect = $"({workRect.left}, {workRect.top}, {workRect.right}, {workRect.bottom})";
        EffectiveDpi = $"({effectiveDpi.X} x {effectiveDpi.Y})";
        AngularDpi = $"({angularDpi.X} x {angularDpi.Y})";
        RawDpi = $"({rawDpi.X} x {rawDpi.Y})";
        Resolution = $"({resolution.X} x {resolution.Y})";
        ReportedScale = (int)reportedScale;
        CalculatedScale = effectiveDpi.X / 96;
    }
}