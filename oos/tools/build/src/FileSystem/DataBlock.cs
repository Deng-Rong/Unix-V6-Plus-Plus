using System;
using System.IO;

namespace Build
{
	/// <summary>
	/// datablock 的摘要说明。
	/// </summary>
	public class DataBlock
	{
        //超级快管理
		private Superblock _initSuper;
        //磁盘文件
        private Disk _diskFile;

		/// <summary>
        /// 数据区占据的块数(不包括启动区，内核区,超级块区，inode区,swap区)
		/// </summary>
        /// 
		private int _dataBlkNum;

		/// <summary>
		/// 数据区起始块
		/// </summary>
		private int _dataBlkStart;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="tsb">传入的超级块</param>
        /// <param name="diskFile">磁盘文件</param>
		public DataBlock(Superblock tsb,Disk diskFile)
		{
			_initSuper = tsb;
            _diskFile = diskFile;
			_dataBlkNum = MachinePara.GetDataSize();
            _dataBlkStart = MachinePara.GetDataStartAddr();
        }

        /// <summary>
        /// 初始化数据块
        /// </summary>
		public void InitDataBlock()
		{
			byte[] writeTo;

			_initSuper._s_nfree = 0;
			_initSuper._s_free[_initSuper._s_nfree++] = 0;
            _diskFile.OpenFile();

            //从最后一块往前走，每100块之前的一块为管理块
            //其内容包括
            //1：该管理块管理的块数
            //2：该管理块管理的所有块号
			for (int i = _dataBlkNum - 1; i >= 0; --i)
			{
				if (_initSuper._s_nfree >= 100)
				{
                    //找到要写入的位置,将s_nfree和s_free写入i + datablkstart号磁盘块
					_diskFile.SeekFilePosition(_diskFile.ConvertPosition(i + _dataBlkStart,0), System.IO.SeekOrigin.Begin);
                    
                    //转换成流，写入磁盘
					writeTo = Helper.Struct2Bytes(_initSuper._s_nfree);
					_diskFile.WriteFile(ref writeTo, 0, 4);

					for (int j = 0; j < _initSuper._s_nfree; ++j)
					{
						//转换成流，写入磁盘
						writeTo = Helper.Struct2Bytes(_initSuper._s_free[j]);
						_diskFile.WriteFile(ref writeTo, 0, 4);
					}
					_initSuper._s_nfree = 0;
				}
                //数组元素初始化(数组元素用于管理空闲块号)
				_initSuper._s_free[_initSuper._s_nfree++] = i + _dataBlkStart;
			}
            //关闭磁盘文件
            _diskFile.CloseFile();
            //将更新后的超级快信息写回
            _initSuper.UpdateSuperBlockToDisk();
		}

		/// <summary>
		/// 获取空闲块
		/// </summary>
		/// <returns></returns>
		public int GetFreeBlock()
		{
			int reValue;
			byte[] readIn = new byte[4];

			reValue = _initSuper._s_free[--_initSuper._s_nfree];

			//出错处理,已经分配完空闲盘块
			if(_initSuper._s_nfree <= 0 && reValue == 0)
			{
                throw (new Exception("空闲数据块已分配完")); 
			}

			//如果本批次直接管理的空闲块用完，则导入下一组
			if (_initSuper._s_nfree <= 0)
			{
                _diskFile.OpenFile();
                _diskFile.SeekFilePosition(512 * reValue, System.IO.SeekOrigin.Begin);
                _diskFile.ReadFile(ref readIn, 0, 4);
                _initSuper._s_nfree = (int)Helper.Bytes2Struct(readIn, typeof(int));

				for (int i = 0; i < 100; ++i)
				{
                    _diskFile.ReadFile(ref readIn, 0, 4);
                    _initSuper._s_free[i] = (int)Helper.Bytes2Struct(readIn, typeof(int));
				}
                _diskFile.CloseFile();
			}
			//最好在这里将新申请到的块全部清零

			CleanBlock(reValue);
            _initSuper.UpdateSuperBlockToDisk();
			return reValue;
		}

		/// <summary>
		/// 将磁盘块全部清零
		/// </summary>
		/// <param name="bno"></param>
		private void CleanBlock(int bno)
		{
			int value = 0;
			byte[] bvalue;
            bvalue = Helper.Struct2Bytes(value);

            _diskFile.OpenFile();
            _diskFile.SeekFilePosition(512 * bno, System.IO.SeekOrigin.Begin);

			for (int i = 0; i < MachinePara.Block_Size; i = i+4)
			{
                _diskFile.WriteFile(ref bvalue, 0, 4);
			}
            _diskFile.CloseFile();
		}

        /// <summary>
        /// 得到虚拟磁盘剩余空间大小(暂无引用)
        /// </summary>
        /// <returns></returns>
        public int returnFreeSize()
        {
            int size = _initSuper._s_nfree-1;
            int team_first_no = _initSuper._s_free[0];

            if (team_first_no == 0)
                return size ;
            else
            {
                
                while (true)
                {
                    _diskFile.OpenFile();
                    _diskFile.SeekFilePosition(512 * team_first_no, SeekOrigin.Begin);
                   
                    byte[] read = new byte[4];

                    _diskFile.ReadFile(ref read, 0, 4);

                    int temp_size = (int)Helper.Bytes2Struct(read, typeof(int));

                    read = new byte[4];

                    _diskFile.ReadFile(ref read, 0, 4);
                    _diskFile.CloseFile();
                    team_first_no = (int)Helper.Bytes2Struct(read, typeof(int));

                    if (team_first_no == 0)
                    {
                        size += (temp_size );
                        return size;
                    }

                    size += temp_size;
                }
                
            }
        }
	}
}
