using System;

namespace Build
{
	public class Boot
	{
		/// <summary>
		/// 启动文件(boot.bin)
		/// </summary>
		private RWFiles _bootFile;

        /// <summary>
        /// 磁盘文件(c.img)
        /// </summary>
        private Disk _diskFile;

        /// <summary>
        /// 构造函数
        /// </summary>
		public Boot(Disk diskFile)
		{
			_bootFile = new RWFiles(MachinePara.BootPath);
            _diskFile = diskFile;
		}

        /// <summary>
        /// 将编译好的启动文件boot.bin按规定写入磁盘
        /// </summary>
		public void WriteBootBlock()
		{
            int bootSize = MachinePara.Boot_Size * MachinePara.Block_Size;
            byte[] writeTo = new byte[bootSize];

            //读取boot.bin
            _bootFile.OpenFile();
            _bootFile.SeekFilePosition(0, System.IO.SeekOrigin.Begin);
            _bootFile.ReadFile(ref writeTo, 0, 512);
            _bootFile.CloseFile();

            //写入c.img
            _diskFile.OpenFile();
		    _diskFile.SeekFilePosition(0, System.IO.SeekOrigin.Begin);
            _diskFile.WriteFile(ref writeTo, 0, bootSize);
            _diskFile.CloseFile();
		}
	}
}
