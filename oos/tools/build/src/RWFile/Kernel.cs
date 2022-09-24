using System;
using System.Collections.Generic;
using System.Text;

namespace Build
{
    class Kernel
    {
        /// <summary>
		/// 启动文件
		/// </summary>
		private RWFiles _kernelFile;

        /// <summary>
        /// 磁盘文件
        /// </summary>
        private Disk _diskFile;

        /// <summary>
        /// 构造函数
        /// </summary>
		public Kernel(Disk diskFile)
		{
			_kernelFile = new RWFiles(MachinePara.OsPath);
            _diskFile = diskFile;
		}

        /// <summary>
        /// 将编译好的内核kernel.bin文件按规定写入磁盘
        /// </summary>
		public void WriteKernelBlock()
		{
            _kernelFile.OpenFile();
            int kernelFileLength = (int)_kernelFile.ReturnFileLength();
            byte[] writeTo;
            writeTo = new byte[kernelFileLength];
            
            //读出kernel.bin文件的内容
			_kernelFile.SeekFilePosition(0, System.IO.SeekOrigin.Begin);
            _kernelFile.ReadFile(ref writeTo, 0, kernelFileLength);
            _kernelFile.CloseFile();

            //将kernel.bin的内容写入c.img
            _diskFile.OpenFile();
            _diskFile.SeekFilePosition(1*512, System.IO.SeekOrigin.Begin);
            _diskFile.WriteFile(ref writeTo, 0, kernelFileLength);
            _diskFile.CloseFile();
		}
    }
}
