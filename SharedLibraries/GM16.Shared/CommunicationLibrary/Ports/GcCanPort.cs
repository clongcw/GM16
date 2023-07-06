using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace GM16.Shared.CommunicationLibrary
{
    public class GcCanPort : Port
    {
        private VCI_INIT_CONFIG GetInitConfig(BaudRate baudRate)
        {
            VCI_INIT_CONFIG initConfig = new VCI_INIT_CONFIG
            {
                AccCode = 0,
                AccMask = 0xffffff,
                Filter = 0,
                Mode = 0
            };
            switch (baudRate)
            {
                case BaudRate.BAUD_1000K:
                    initConfig.Timing0 = 0;
                    initConfig.Timing1 = 0x14;
                    break;
                case BaudRate.BAUD_800K:
                    initConfig.Timing0 = 0;
                    initConfig.Timing1 = 0x16;
                    break;
                case BaudRate.BAUD_500K:
                    initConfig.Timing0 = 0;
                    initConfig.Timing1 = 0x1c;
                    break;
                case BaudRate.BAUD_250K:
                    initConfig.Timing0 = 0x01;
                    initConfig.Timing1 = 0x1c;
                    break;
                case BaudRate.BAUD_125K:
                    initConfig.Timing0 = 0x03;
                    initConfig.Timing1 = 0x1c;
                    break;
                case BaudRate.BAUD_100K:
                    initConfig.Timing0 = 0x04;
                    initConfig.Timing1 = 0x1c;
                    break;
                case BaudRate.BAUD_50K:
                    initConfig.Timing0 = 0x09;
                    initConfig.Timing1 = 0x1c;
                    break;
                default:
                    initConfig.Timing0 = 0;
                    initConfig.Timing1 = 0x1c;
                    break;
            }
            return initConfig;
        }

        static UInt32 DeviceType = 3;
        static UInt32 DeviceId = 0;
        private bool _isRunning = false;
        private readonly object _objLock = new object();

        private IntPtr pt;

        public GcCanPort()
        {
            _canId = 0;
        }

        public GcCanPort(byte portId)
        {
            _canId = portId;
        }

        public override int Open()
        {
            pt = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(VCI_CAN_OBJ)) * MAX_COUNT);

            IsOpened = true;
            return (int)GcCanAPI.OpenDevice(DeviceType, DeviceId, 0);
        }

        public override int Close()
        {
            Marshal.FreeHGlobal(pt);
            IsOpened = false;
            _isRunning = false;
            return (int)GcCanAPI.CloseDevice(DeviceType, DeviceId);
        }

        public override int SetAccCodeMask(uint accCode, uint accMask)
        {
            return 1;
        }

        public override int SetBaudRate(BaudRate baudRate)
        {
            return 1;
        }

        public override int Init(BaudRate baudRate)
        {
            var config = GetInitConfig(baudRate);
            return (int)GcCanAPI.InitCAN(DeviceType, DeviceId, _canId, ref config);
        }

        public override int Start()
        {
            GcCanAPI.ClearBuffer(DeviceType, DeviceId, _canId);
            int res = (int)GcCanAPI.StartCAN(DeviceType, DeviceId, _canId);
            _isRunning = res != 0;
            return res;
        }

        public override int Reset()
        {
            int res = (int)GcCanAPI.ResetCAN(DeviceType, DeviceId, _canId);
            return (int)res;
        }

        public override int Clear()
        {
            int res = (int)GcCanAPI.ClearBuffer(DeviceType, DeviceId, _canId);
            return (int)res;
        }

        public override int GetReceiveNum()
        {
            return (int)GcCanAPI.GetReceiveNum(DeviceType, DeviceId, _canId);
        }

        public override int Send(uint count, CanData[] datas)
        {
            lock (_objLock)
            {
                VCI_CAN_OBJ[] objSend = new VCI_CAN_OBJ[count];
                for (int i = 0; i < count; i++)
                {
                    objSend[i].Init();
                    objSend[i].ID = datas[i].ID;
                    objSend[i].RemoteFlag = (byte)datas[i].FrameMode;
                    objSend[i].ExternFlag = (byte)datas[i].FrameFlag;
                    objSend[i].DataLen = datas[i].Length;
                    datas[i].Data.CopyTo(objSend[i].Data, 0);
                }
                return (int)GcCanAPI.Transmit(DeviceType, DeviceId, _canId, ref objSend[0], count);
            }
        }

        public override int Send(uint id, byte len, byte[] datas)
        {
            if (IsConnected)
            {
                VCI_CAN_OBJ objSend = new VCI_CAN_OBJ();
                objSend.Init();
                objSend.ID = id;
                objSend.DataLen = len;
                datas.CopyTo(objSend.Data, 0);
                return (int)GcCanAPI.Transmit(DeviceType, DeviceId, _canId, ref objSend, 1);
            }
            else
            {
                return 0;
            }
        }

        public override int Receive(ushort count, ref CanData[] datas)
        {
            VCI_CAN_OBJ[] objRec = new VCI_CAN_OBJ[count];
            for (int i = 0; i < count; i++)
                objRec[i].Init();

            int res = (int)GcCanAPI.Receive(DeviceType, DeviceId, _canId, pt, count, -1);
            for (int i = 0; i < res; i++)
            {
                objRec[i] = (VCI_CAN_OBJ)Marshal.PtrToStructure((IntPtr)((UInt32)pt + i * Marshal.SizeOf(typeof(VCI_CAN_OBJ))), typeof(VCI_CAN_OBJ));
                datas[i].ID = objRec[i].ID;
                datas[i].TimeStamp = objRec[i].TimeStamp;
                datas[i].FrameMode = objRec[i].RemoteFlag == 0 ? FrameMode.Data : FrameMode.Remote;
                datas[i].FrameFlag = objRec[i].ExternFlag == 0 ? FrameFlag.Stanard : FrameFlag.Extern;
                datas[i].Length = objRec[i].DataLen;
                objRec[i].Data.CopyTo(datas[i].Data, 0);
            }

            return res;
        }

        public override int Receive(ref CanData data)
        {
            VCI_CAN_OBJ objRec = new VCI_CAN_OBJ();
            int timeout = -1;
            objRec.Init();
            int res = (int)GcCanAPI.Receive(DeviceType, DeviceId, _canId, pt, 1, timeout);
            objRec = (VCI_CAN_OBJ)Marshal.PtrToStructure(pt, typeof(VCI_CAN_OBJ));

            data.ID = objRec.ID;
            data.TimeStamp = objRec.TimeStamp;
            data.FrameMode = objRec.RemoteFlag == 0 ? FrameMode.Data : FrameMode.Remote;
            data.FrameFlag = objRec.ExternFlag == 0 ? FrameFlag.Stanard : FrameFlag.Extern;
            data.Length = objRec.DataLen;
            objRec.Data.CopyTo(data.Data, 0);
            return res;
        }

        public override int ReadErrInfo(ref VCI_ERR_INFO pErrInfo)
        {
            return (int)GcCanAPI.ReadErrInfo(DeviceType, DeviceId, _canId, ref pErrInfo);
        }

        public override bool IsRunning()
        {
            return _isRunning;
        }

    }
}
