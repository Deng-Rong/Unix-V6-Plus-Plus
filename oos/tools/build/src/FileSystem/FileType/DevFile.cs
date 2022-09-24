using System;

namespace Build
{
	/// <summary>
	/// devfile 的摘要说明。
	/// </summary>
	public class DevFile : File
	{
		/// <summary>
		/// 设备种类
		/// </summary>
		private string _devType;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="tsb"></param>
        /// <param name="tid"></param>
        /// <param name="tdb"></param>
        /// <param name="disk"></param>
        /// <param name="createFilePath"></param>
        /// <param name="devType"></param>
        public DevFile(Superblock tsb, InodeBlock tid, DataBlock tdb, Disk disk, string createFilePath, string devType)
            : base(tsb, tid, tdb, disk, createFilePath)
		{
            this._devType = devType;
        }

        /// <summary>
        /// 设置文件的inode内容
        /// </summary>
		protected override void SetCommonFileInode()
		{
			base.SetCommonFileInode ();
			if(_devType == "tty")
			{
				_fileInode._i_mode = (CommonFile.IALLOC | IFCHR | IREAD | IWRITE | IEXEC | (IREAD >> 3) | (IWRITE >> 3) | (IEXEC >> 3) | (IREAD >> 6) | (IWRITE >> 6) | (IEXEC>>6));
    			_fileInode._i_addr[0] = 0;

			}
			else if(_devType == "disk")
			{
				_fileInode._i_mode = (CommonFile.IALLOC | IFBLK | IREAD | IWRITE | IEXEC | (IREAD >> 3) | (IWRITE >> 3) | (IEXEC >> 3) | (IREAD >> 6) | (IWRITE >> 6) | (IEXEC>>6));
				_fileInode._i_addr[0] = 0;
			}
		}
	}
}
