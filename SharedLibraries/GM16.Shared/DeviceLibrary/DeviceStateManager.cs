using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GM16.Shared.DeviceLibrary
{
    public enum DeviceState
    {
        Idle,   //空闲
        Running, //运行
        Pause,  //暂停
        Continue,//继续
        Stop    //停止
    }

    public class DeviceStateChangedEventArgs : EventArgs
    {
        public DeviceState DeviceState
        {
            get;
            private set;
        }

        public DeviceStateChangedEventArgs(DeviceState devState)
        {
            DeviceState = devState;
        }
    }

    public class DeviceStateManager
    {
        public delegate void DeviceStateChangedEventHandler(object sender, DeviceStateChangedEventArgs e);
        public event DeviceStateChangedEventHandler DeviceStateChanged = null;

        protected virtual void OnDeviceStateChanged(DeviceStateChangedEventArgs e)
        {
            DeviceStateChangedEventHandler temp = Interlocked.CompareExchange(ref DeviceStateChanged, null, null);//线程安全
            if (temp != null)
            {
                temp(this, e);
            }
        }

        private void RefreshDeviceState(DeviceState devState)
        {
            DeviceStateChangedEventArgs e = new DeviceStateChangedEventArgs(devState);
            OnDeviceStateChanged(e);
        }

        private DeviceState _currentDevState = DeviceState.Idle;

        /// <summary>
        /// 当前设备状态
        /// </summary>
        public DeviceState CurrentDevState
        {
            get
            {
                return _currentDevState;
            }
            set
            {
                if (value != _currentDevState)
                {
                    _currentDevState = value;
                    RefreshDeviceState(_currentDevState);
                }
            }
        }

    }
}
