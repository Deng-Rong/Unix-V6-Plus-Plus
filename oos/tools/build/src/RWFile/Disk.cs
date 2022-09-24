using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Build
{
    public class Disk : RWFiles
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public Disk() : base(MachinePara.DiskPath)
        {
        }

        /// <summary>
        /// 磁盘文件初始化
        /// </summary>
        public void Init()
        {
            OpenFile();
            SetDiskLength(MachinePara.Cylinders * MachinePara.Heads * MachinePara.Spt * MachinePara.Block_Size);
            CleanDiskFile();
            CloseFile();
        }
    }
}
