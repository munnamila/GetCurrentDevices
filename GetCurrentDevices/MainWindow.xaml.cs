using System;
using System.Text;
using System.Windows;
using System.Runtime.InteropServices;

namespace GetCurrentDevices
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        // 导入 SetupDiGetClassDevs 函数
        [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern IntPtr SetupDiGetClassDevs(
            IntPtr ClassGuid,
            IntPtr Enumerator,
            IntPtr hwndParent,
            uint Flags);

        // 导入 SetupDiDestroyDeviceInfoList 函数，用于释放设备信息集合句柄
        [DllImport("setupapi.dll", SetLastError = true)]
        public static extern bool SetupDiDestroyDeviceInfoList(IntPtr DeviceInfoSet);

        // 导入 SetupDiEnumDeviceInfo 函数，用于枚举设备信息
        [DllImport("setupapi.dll", SetLastError = true)]
        public static extern bool SetupDiEnumDeviceInfo(
            IntPtr DeviceInfoSet,
            uint MemberIndex,
            ref SP_DEVINFO_DATA DeviceInfoData);

        // 导入 SetupDiGetDeviceRegistryProperty 函数，用于获取设备属性
        [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern bool SetupDiGetDeviceRegistryProperty(
            IntPtr DeviceInfoSet,
            ref SP_DEVINFO_DATA DeviceInfoData,
            uint Property,
            out uint PropertyRegDataType,
            byte[] PropertyBuffer,
            uint PropertyBufferSize,
            out uint RequiredSize);

        // 定义设备信息数据结构
        [StructLayout(LayoutKind.Sequential)]
        public struct SP_DEVINFO_DATA
        {
            public uint cbSize;
            public Guid ClassGuid;
            public uint DevInst;
            public IntPtr Reserved;
        }

        // 常量定义
        public const uint DIGCF_PRESENT = 0x00000002;
        public const uint DIGCF_ALLCLASSES = 0x00000004;
        public const uint SPDRP_DEVICEDESC = 0x00000000; // 获取设备描述


        private void OnGetDeviceInfoClick(object sender, RoutedEventArgs e)
        {
            IntPtr deviceInfoSet = SetupDiGetClassDevs(IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, DIGCF_PRESENT | DIGCF_ALLCLASSES);

            if (deviceInfoSet == IntPtr.Zero)
            {
                int errorCode = Marshal.GetLastWin32Error();
                DeviceInfoTextBox.Text = $"Failed to get device information set. Error code: {errorCode}";
                return;
            }

            StringBuilder deviceInfoOutput = new StringBuilder();
            deviceInfoOutput.AppendLine("Device information set handle obtained successfully:");

            SP_DEVINFO_DATA deviceInfoData = new SP_DEVINFO_DATA();
            deviceInfoData.cbSize = (uint)Marshal.SizeOf(deviceInfoData);

            uint index = 0;
            while (SetupDiEnumDeviceInfo(deviceInfoSet, index, ref deviceInfoData))
            {
                // 获取设备描述
                byte[] buffer = new byte[1024];
                uint propertyRegDataType;
                uint requiredSize;

                if (SetupDiGetDeviceRegistryProperty(deviceInfoSet, ref deviceInfoData, SPDRP_DEVICEDESC, out propertyRegDataType, buffer, (uint)buffer.Length, out requiredSize))
                {
                    string deviceDescription = Encoding.Unicode.GetString(buffer, 0, (int)requiredSize - 2); // -2 去掉末尾的 '\0'
                    deviceInfoOutput.AppendLine($"Device {index}: {deviceDescription}");
                }
                else
                {
                    deviceInfoOutput.AppendLine($"Device {index}: Unable to retrieve description.");
                }

                index++;
            }

            // 释放设备信息集合句柄
            SetupDiDestroyDeviceInfoList(deviceInfoSet);

            DeviceInfoTextBox.Text = deviceInfoOutput.ToString();
        }

        private void OnClearClick(object sender, RoutedEventArgs e)
        {
            DeviceInfoTextBox.Text = string.Empty;
        }
    }
}