using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace GM16.Shared.CommunicationLibrary
{
    public static class GcCanAPI
    {
        private const string DllName = "ECanVci.dll";
        [DllImport(DllName, EntryPoint = "OpenDevice")]
        public static extern UInt32 OpenDevice(
            UInt32 DeviceType,
            UInt32 DeviceInd,
            UInt32 Reserved);

        [DllImport(DllName, EntryPoint = "CloseDevice")]
        public static extern UInt32 CloseDevice(
            UInt32 DeviceType,
            UInt32 DeviceInd);


        [DllImport(DllName, EntryPoint = "InitCAN")]
        public static extern UInt32 InitCAN(
            UInt32 DeviceType,
            UInt32 DeviceInd,
            UInt32 CANInd,
            ref VCI_INIT_CONFIG InitConfig);

        [DllImport(DllName, EntryPoint = "ClearBuffer")]
        public static extern UInt32 ClearBuffer(UInt32 DeviceType, UInt32 DeviceInd, UInt32 CANInd);

        [DllImport(DllName, EntryPoint = "StartCAN")]
        public static extern UInt32 StartCAN(
            UInt32 DeviceType,
            UInt32 DeviceInd,
            UInt32 CANInd);


        [DllImport(DllName, EntryPoint = "ResetCAN")]
        public static extern UInt32 ResetCAN(
            UInt32 DeviceType,
            UInt32 DeviceInd,
            UInt32 CANInd);

        [DllImport(DllName, EntryPoint = "GetReceiveNum")]
        public static extern UInt32 GetReceiveNum(UInt32 DeviceType, UInt32 DeviceInd, UInt32 CANInd);

        [DllImport(DllName, EntryPoint = "Transmit")]
        public static extern UInt32 Transmit(
            UInt32 DeviceType,
            UInt32 DeviceInd,
            UInt32 CANInd,
            ref VCI_CAN_OBJ Send,
            UInt32 length);


        [DllImport(DllName, EntryPoint = "Receive")]
        public static extern UInt32 Receive(
            UInt32 DeviceType,
            UInt32 DeviceInd,
            UInt32 CANInd,
            IntPtr pReceive,
            UInt32 length,
            Int32 WaitTime);

        [DllImport(DllName, EntryPoint = "ReadErrInfo")]
        public static extern UInt32 ReadErrInfo(
            UInt32 DeviceType,
            UInt32 DeviceInd,
            UInt32 CANInd,
            ref VCI_ERR_INFO ReadErrInfo);
    }
}
