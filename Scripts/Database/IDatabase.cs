namespace ClawRPG.Scripts.Database
{
    /// <summary>
    /// 数据库接口，所有数据库类都应实现此接口
    /// </summary>
    public interface IDatabase
    {
        /// <summary>
        /// 获取数据库单例实例
        /// </summary>
        object Instance { get; }

        /// <summary>
        /// 初始化数据库，加载配置数据
        /// </summary>
        void Initialize();

        /// <summary>
        /// 验证数据完整性
        /// </summary>
        bool ValidateData();
    }
}
