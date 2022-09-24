using System;
using System.Collections.Generic;

namespace Build
{
	/// <summary>
	/// superblock 的摘要说明。
	/// </summary>
	public class Superblock
	{
		/// <summary>
		/// inode区占用的磁盘块数
		/// </summary>
        public int _s_isize;

		/// <summary>
		/// 盘块总数
		/// </summary>
		public int _s_fsize;

		/// <summary>
		///直接管理的空闲块数 
		/// </summary>
		public int _s_nfree;

		/// <summary>
		/// 空闲块索引表
		/// </summary>
		public int[] _s_free;

		/// <summary>
		/// 直接管理的inode数
		/// </summary>
		public int _s_ninode;

		/// <summary>
		/// 空闲inode索引表
		/// </summary>
		public int[] _s_inode;

		/// <summary>
		/// 封锁标志——空闲块
		/// </summary>
		public int _s_flock;

		/// <summary>
		/// 封锁inode标志
		/// </summary>
		public int _s_ilock;

		/// <summary>
		/// 已被修改
		/// </summary>
		public int _s_fmod;

		/// <summary>
		/// 只读标志
		/// </summary>
		public int _s_ronly;

		/// <summary>
		/// 最后一次修改时间，此时为安装时间
		/// </summary>
		public int _s_time;

        /// <summary>
        /// 最后的填充位
        /// </summary>
        public int[] _s_padding;


        /// <summary>
        /// 包含一个读写文件类
        /// </summary>
        private Disk _diskFile;

        /// <summary>
        /// 构造函数
        /// </summary>
		public Superblock(Disk diskFile)
		{
			_s_isize = MachinePara.InodeSize;
			_s_fsize = (MachinePara.Heads * MachinePara.Spt * MachinePara.Cylinders) ;
			_s_nfree = 100;
			_s_free = new int[100];
			_s_ninode = 100;
			_s_inode = new int[100];
			_s_flock = 0;
			_s_ilock = 0;
			_s_fmod = 0;
			_s_ronly = 0;
			_s_time = Helper.GetCurrentTime();

            //Init padding(这里padding的内容用于向OS内核代码传递一些数据信息)
            _s_padding = new int[47];
            InitPadding();

            _diskFile = diskFile;
		}

        /// <summary>
        ///padding填充数组的初始化函数 
        /// </summary>
        private void InitPadding()
        {
            // 表明s_paddind是否做过修改
            _s_padding[0] = 1;
            // 磁盘总扇区数
            _s_padding[1] = MachinePara.Cylinders * MachinePara.Heads * MachinePara.Spt;
            //Inode区起始位置
            _s_padding[2] = MachinePara.SuperBlockSize + MachinePara.BootAndKernelSize;
            //Inode区大小
            _s_padding[3] = MachinePara.InodeSize;
            //Data区起始位置
            _s_padding[4] = _s_padding[2] + _s_padding[3];
            //Data区大小
            _s_padding[5] = _s_padding[1] - _s_padding[4] - MachinePara.SwapSize;
            //Swap区起始位置
            _s_padding[6] = _s_padding[4] + _s_padding[5];
            //Swap区大小
            _s_padding[7] = MachinePara.SwapSize;
        }

        /// <summary>
        /// 将当前的超级快信息更新到磁盘
        /// </summary>
		public void UpdateSuperBlockToDisk()
		{
			byte[] writeTo;

			//定位到超级块起始位置
            _diskFile.OpenFile();
			_diskFile.SeekFilePosition(MachinePara.BootAndKernelSize*512, System.IO.SeekOrigin.Begin);

			//inode区占据的磁盘块数
			writeTo = Helper.Struct2Bytes(_s_isize);
			_diskFile.WriteFile(ref writeTo, 0, 4);

			//总的磁盘块数，为扇区数的一半
			writeTo = Helper.Struct2Bytes(_s_fsize);
			_diskFile.WriteFile(ref writeTo, 0, 4);

			//得到直接管理块数
			writeTo = Helper.Struct2Bytes(_s_nfree);
			_diskFile.WriteFile(ref writeTo, 0, 4);

			//得到空闲列表
			for (int i = 0; i < 100; ++i)
			{
				writeTo = Helper.Struct2Bytes(_s_free[i]);
				_diskFile.WriteFile(ref writeTo, 0, 4);
			}
 
			//得到直接管理的inodee数量
			writeTo = Helper.Struct2Bytes(_s_ninode);
			_diskFile.WriteFile(ref writeTo, 0, 4);

			//得到空闲inode列表
			for (int i = 0; i < 100; ++i)
			{
				writeTo = Helper.Struct2Bytes(_s_inode[i]);
				_diskFile.WriteFile(ref writeTo, 0, 4);
			}

			//封锁标志
			writeTo = Helper.Struct2Bytes(_s_flock);
			_diskFile.WriteFile(ref writeTo, 0, 4);
           
			//inode封锁标志
			writeTo = Helper.Struct2Bytes(_s_ilock);
			_diskFile.WriteFile(ref writeTo, 0, 4);

			//已修改标志
			writeTo = Helper.Struct2Bytes(_s_fmod);
			_diskFile.WriteFile(ref writeTo, 0, 4);

			//只读标志
			writeTo = Helper.Struct2Bytes(_s_ronly);
			_diskFile.WriteFile(ref writeTo, 0, 4);

			//最后修改时间
			writeTo = Helper.Struct2Bytes(_s_time);
			_diskFile.WriteFile(ref writeTo, 0, 4);

            //填充字段
            if (_s_padding[0] == 1)
            {
                for (int i = 0; i < 47; i++)
                {
                    writeTo = Helper.Struct2Bytes(_s_padding[i]);
                    _diskFile.WriteFile(ref writeTo, 0, 4);
                }
                _s_padding[0] = 0;
            }
            _diskFile.CloseFile();
		}

        /// <summary>
        /// 从磁盘读出当前超级块的内容(暂无引用)
        /// </summary>
        public void ReadSuperBlock()
        {
            byte[] read;
            //定位到超级块起始位置
            _diskFile.OpenFile();
            _diskFile.SeekFilePosition(MachinePara.BootAndKernelSize * 512, System.IO.SeekOrigin.Begin);

            //inode区占据的磁盘块数
            read = new byte[4];           
            _diskFile.ReadFile(ref read, 0, 4);
            _s_isize = (int)Helper.Bytes2Struct(read,typeof(int));

            //总的磁盘块数，为扇区数的一半
            read = new byte[4];
            _diskFile.ReadFile(ref read, 0, 4);
            _s_fsize = (int)Helper.Bytes2Struct(read, typeof(int));

            //得到直接管理块数
            read = new byte[4];
            _diskFile.ReadFile(ref read, 0, 4);
            _s_nfree = (int)Helper.Bytes2Struct(read, typeof(int));

            //得到空闲列表
            for (int i = 0; i < 100; ++i)
            {
                read = new byte[4];
                _diskFile.ReadFile(ref read, 0, 4);
                _s_free[i] = (int)Helper.Bytes2Struct(read, typeof(int));
            }

            //得到直接管理的inode数量
            read = new byte[4];
            _diskFile.ReadFile(ref read, 0, 4);
            _s_ninode = (int)Helper.Bytes2Struct(read, typeof(int));

            //得到空闲inode列表
            for (int i = 0; i < 100; ++i)
            {
                read = new byte[4];
                _diskFile.ReadFile(ref read, 0, 4);
                _s_inode[i] = (int)Helper.Bytes2Struct(read, typeof(int));
            }

            //封锁标志
            read = new byte[4];
            _diskFile.ReadFile(ref read, 0, 4);
            _s_flock = (int)Helper.Bytes2Struct(read, typeof(int));

            //inode封锁标志
            read = new byte[4];
            _diskFile.ReadFile(ref read, 0, 4);
            _s_ilock = (int)Helper.Bytes2Struct(read, typeof(int));

            //已修改标志
            read = new byte[4];
            _diskFile.ReadFile(ref read, 0, 4);
            _s_fmod = (int)Helper.Bytes2Struct(read, typeof(int));

            //只读标志
            read = new byte[4];
            _diskFile.ReadFile(ref read, 0, 4);
            _s_ronly = (int)Helper.Bytes2Struct(read, typeof(int));

            //最后修改时间
            read = new byte[4];
            _diskFile.ReadFile(ref read, 0, 4);
            _s_time = (int)Helper.Bytes2Struct(read, typeof(int));

            _diskFile.CloseFile();
        }
	}
}
