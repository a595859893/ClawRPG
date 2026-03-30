using System;

namespace ClawRPG.Scripts.Data.Enemy
{
    /// <summary>
    /// 敌人生成实例数据 - 运行时敌人生成后的实例信息
    /// </summary>
    public class EnemyInstance
    {
        public int InstanceId { get; set; }
        public string TypeId { get; set; }
        public float CurrentHp { get; set; }
        public float MaxHp { get; set; }
        public int Level { get; set; }
        public bool IsAlive { get; set; }

        public EnemyInstance()
        {
            IsAlive = true;
        }
    }
}
