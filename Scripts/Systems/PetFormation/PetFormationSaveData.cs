using System;
using System.Collections.Generic;

namespace ClawRPG.Systems.PetFormation
{
    /// <summary>
    /// 宠物阵型系统保存数据
    /// </summary>
    [System.Serializable]
    public class PetFormationSaveData
    {
        /// <summary>前锋宠物ID (0 = 空)</summary>
        public int FrontPetId;
        /// <summary>中线宠物ID (0 = 空)</summary>
        public int MidPetId;
        /// <summary>后卫宠物ID (0 = 空)</summary>
        public int RearPetId;
        /// <summary>上次激活的阵型类型</summary>
        public int LastFormationType;
        /// <summary>当前 run 是否已验证过阵型</summary>
        public bool FormationValidatedThisRun;
    }
}
