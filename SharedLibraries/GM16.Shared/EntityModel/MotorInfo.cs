using SqlSugar;

namespace GM16.Shared.EntityModel
{
    [SugarTable("MotorInfos")]
    public class MotorInfo
    {
        /// <summary>
        /// ID 
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 电机ID
        /// </summary>
        public byte MotorId { get; set; }

        /// <summary>
        /// 电机名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 电机运行最大步数
        /// </summary>
        public int MaxStep { get; set; }

        /// <summary>
        /// 电机所属设备ID
        /// </summary>
        public short DeviceId { get; set; }

        /// <summary>
        /// 正常运行速度
        /// </summary>
        public int NormalSpeed { get; set; }

        /// <summary>
        /// 复位速度
        /// </summary>
        public int ResetSpeed { get; set; }

        /// <summary>
        /// 工作电流
        /// </summary>
        public int Current { get; set; }

        /// <summary>
        /// 复位修正步数
        /// </summary>
        public int ResetStep { get; set; }
        /// <summary>
        /// 复位安全距离
        /// </summary>
        public int ResetSafeStep { get; set; }
    }
}
