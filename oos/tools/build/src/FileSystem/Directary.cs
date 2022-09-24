using System;

namespace Build
{
	/// <summary>
	/// 这是一个操作类
	/// </summary>
	public class Directary
	{
		/// <summary>
		/// 超级块管理
		/// </summary>
		private Superblock _superBlock;

		/// <summary>
		/// inode管理
		/// </summary>
		private InodeBlock _inode;

		/// <summary>
		/// 空闲数据块管理
		/// </summary>
		private DataBlock _dataBlock;

        /// <summary>
        /// 磁盘文件
        /// </summary>
        private Disk _diskFile;
        
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="tsb"></param>
        /// <param name="tid"></param>
        /// <param name="tdb"></param>
        /// <param name="diskFile"></param>
		public Directary(Superblock tsb,InodeBlock tid,DataBlock tdb,Disk diskFile)
		{
			_superBlock = tsb;
			_inode = tid;
			_dataBlock = tdb;
            _diskFile = diskFile;
		}

		/// <summary>
		/// 创建文件时的目录搜索
        /// 本函数共4个出口
        /// 出口1：目录搜索完毕，且满足三个条件：1.之前各级目录名都能找到,2.但没有符合的最后一级文件,3.且传入的flag为CREATFILE
        /// 出口2：目录搜索完毕，但不完全满足上述三个条件
        /// 出口3：文件（或目录）在系统中已存在，且传入flag为DELETEFILE
        /// 出口4：文件（或目录）在系统中已存在，但传入flag不为DELETEFILE
		/// </summary>
		/// <param name="path">创建文件的目录</param>
		/// <param name="DirInodeNo">父目录的inode号</param>
		/// <returns>目录项的位置</returns>
		public int NameI(string path,ref int dirInodeNo,ref InodeStr dirInode,ref int fileInodeNo,ref InodeStr fileInode,ref char[] fileName,int flag)
		{
            //用于记录在路径字符串中扫描到了哪里
			int curPos = 0;
            //用于记录inode号
			dirInodeNo = 0;
            //首先读入根目录inode
			_inode.GetInodeFromDisk(dirInode,dirInodeNo);

            //跳过开始的一个（或者多个）'/'字符
            for (; curPos < path.Length; ++curPos)
            {
                if (path[curPos] != '/')
                    break;
            }

            //主循环
			while(true)
			{
                //获取两个'/'当中的内容，存入dirPath中
                char[] tmp = new char[28];
				for(int i = 0;curPos < path.Length;++curPos,++i)
				{
					if(path[curPos] == '/')
						break;
                    tmp[i] = path[curPos];
				}
                tmp.CopyTo(fileName, 0);

                //跳过下一个（或者多个）'/'字符，为下一次循环(下一级目录，如果有的话)做准备
                for (; curPos < path.Length; ++curPos)
                {
                    if (path[curPos] != '/')
                        break;
                }
                //用于存储扫描当前目录文件的偏移量
				int offset = 0;
                //用于记录第一个空闲目录项的偏移量
                int firstFreeOffset=0;
                //记录是否又空闲目录项
                int freeFlag = 0;
				//目录项项数
				int dirSize = dirInode._i_size / 32;
                //用于读取一个磁盘块
				byte[] buffer = new byte[512];

				//（次循环）在当前目录文件中搜索
				while(true)
				{
					//出口1：对当前目录搜索完毕(用于查找是否有同名文件存在)
					if(dirSize == 0)
					{
						//若当前目录索搜索完毕(没有找到相应项)，且只是路径名的最后一段没有找到(要创建的文件名)，则创建文件
						if(flag == File.CREATFILE && curPos == path.Length)
						{
							//新分配一个inode控制块，返回该inode号
                            try
                            {
                                fileInodeNo = _inode.FetchFreeInode();
                            }
                            catch(Exception ex)
                            {
                                Console.WriteLine(ex.Message);
                            }
                            //根据inode号获取对应的inode（搜索到了哪一层就获取哪一层目录的inode）
                            _inode.GetInodeFromDisk(fileInode, fileInodeNo);
                            //若之前扫描途中有空闲项，则返回其偏移，否则就返回当前inode的大小(即要对当前inode进行扩充)
                            if (freeFlag == 0)
                                return dirInode._i_size;
                            else
                                return firstFreeOffset;
						}
                         //出口2：若当前目录搜索完毕(没有找到相应项)，且中间路径没有找到，则出错
						else
						{
							Error.ErrorType = Error.SEARCHDIR;
                            return -1;
						}
					}

					//搜索完一块，需要读入新块
					if(offset % 512 == 0)
					{
                        _diskFile.OpenFile();
						_diskFile.SeekFilePosition(this.BMap(dirInode,offset / 512) * 512, System.IO.SeekOrigin.Begin);
						_diskFile.ReadFile(ref buffer,0,512);
                        _diskFile.CloseFile();
					}

                    bool match = true;
					for(int j = 0;j < 28;++j)
					{
						if(Convert.ToByte(fileName[j]) != buffer[offset%512 + j + 4])
						{
                            match = false;
							break;
						}
					}

					//表示不匹配
					if(!match)
					{
						dirSize--;
                        //如果之前的目录项为空，则将其用first_free_offset标记
                        //+4是因为每个目录项的前四个字节是inode号
                        if (buffer[offset % 512 + 4] == '\0' && firstFreeOffset == 0)
                        {
                            firstFreeOffset = offset;
                            freeFlag = 1;
                        }
                        offset += 32;
						continue;
					}
                    //在当前目录中找到了和dirPath匹配的目录项
					else
					{
						break;
					}
				}

				byte[] ino = new byte[4];
				for(int k = 0;k < 4;++k)
				{
					ino[k] = buffer[offset%512 + k];
				}
                
				/* 出口3：
                 * 删除操作
                 * pathPoint == path.Length表示：目录项已经搜索完毕*/
				if(flag == File.DELETEFILE && curPos == path.Length)
				{
					//需删除文件的inode号
					fileInodeNo = (int)Helper.Bytes2Struct(ino,typeof(int));
					return offset;
				}
                
				/* 出口4：
                 * 同一目录下的文件名相同，且不是要做删除操作，则出错
                 * pathPoint == path.Length表示：目录项已经搜索完毕*/
				if(flag !=  File.DELETEFILE && curPos == path.Length)
				{
					Error.ErrorType = Error.SAMEFILENAME;
					return -1;
				}

                //如果还有下级目录，则更新dirInodeNo与dirInode，为当前目录
				dirInodeNo = (int)Helper.Bytes2Struct(ino,typeof(int));
				_inode.GetInodeFromDisk(dirInode,dirInodeNo);
			}
		}

		/// <summary>
		/// 虚实地址转换
		/// </summary>\
        /// 
		/// <param name="DirInode"></param>
		/// <param name="no"></param>
		/// <returns></returns>
		public int BMap(InodeStr dirInode,int itemNum)
		{
			int blockNum;
            byte[] blockNumBuf = new byte[4];

			//直接索引0~5项，一次间接索引6~7项，二次间接索引8~9项
			//直接索引所引用的逻辑块号为0~5块，一次间接引用索引项6~261块
			//二次间接引用索引项128*2+6~128*128*2+128*2+6-1项
			
			//直接索引
			if(itemNum < 6)
			{
				blockNum = dirInode._i_addr[itemNum];
				//如果该逻辑块还没有相应的物理块与之对应，则分配一个物理块
				if(blockNum == 0)
				{
                    try
                    {
                        blockNum = _dataBlock.GetFreeBlock();
                    }
                    catch(Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
					dirInode._i_addr[itemNum] = blockNum;
				}
				return blockNum;
			}
            //间接引用bn>=6
			else
			{
                int firstIndex;
				//一次间接块
				if(itemNum - 262 < 0)
				{	
					firstIndex = ((itemNum - 6) / 128) + 6; //6或7
				}
				else//二次间接块
				{
					firstIndex = ((itemNum - 262) / (128*128)) + 8; //8或9
				}

				//该项为空,则到数据区分配一块空闲块
				if(dirInode._i_addr[firstIndex] == 0)
				{
                    try
                    {
                        dirInode._i_addr[firstIndex] = _dataBlock.GetFreeBlock();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
				}
				blockNum = dirInode._i_addr[firstIndex];

				//如果是二次间接索引，则再获取二级索引块的块号
				if(firstIndex >= 8)
				{
					int secondIndex = ((itemNum - 262) / 128)%128; //0-128
                    
                    _diskFile.OpenFile();
					_diskFile.SeekFilePosition(dirInode._i_addr[firstIndex]*512+secondIndex*4,System.IO.SeekOrigin.Begin);
					_diskFile.ReadFile(ref blockNumBuf,0,4);
                    _diskFile.CloseFile();
                    blockNum = (int)Helper.Bytes2Struct(blockNumBuf, typeof(int));

					if(blockNum == 0)
					{
                        try
                        {
                            blockNum = _dataBlock.GetFreeBlock();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }

                        blockNumBuf = Helper.Struct2Bytes(blockNum);
                        _diskFile.OpenFile();
						_diskFile.SeekFilePosition(dirInode._i_addr[firstIndex]*512+secondIndex*4,System.IO.SeekOrigin.Begin);
                        _diskFile.WriteFile(ref blockNumBuf, 0, 4);
                        _diskFile.CloseFile();
					}
				}
			}

            int directIndex;
			//一次间接块
			if(itemNum - 262 < 0)
			{
				directIndex = ((itemNum - 6) % 128);
			}
			else//二次间接块
			{
				directIndex = ((itemNum - 262) % 128);
			}

            _diskFile.OpenFile();
			_diskFile.SeekFilePosition(512 * blockNum + 4 * directIndex,System.IO.SeekOrigin.Begin);
			_diskFile.ReadFile(ref blockNumBuf,0,4);
            _diskFile.CloseFile();
            int tmpBlockNum = (int)Helper.Bytes2Struct(blockNumBuf, typeof(int));
            
            //若该目录索引项为空，则分配
			if(tmpBlockNum == 0)
			{
                try
                {
                    tmpBlockNum = _dataBlock.GetFreeBlock();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                blockNumBuf = Helper.Struct2Bytes(tmpBlockNum);
                _diskFile.OpenFile();
				_diskFile.SeekFilePosition(512 * blockNum + 4 * directIndex,System.IO.SeekOrigin.Begin);
                _diskFile.WriteFile(ref blockNumBuf, 0, 4);
                _diskFile.CloseFile();
			}
			blockNum = tmpBlockNum;
			return blockNum;
		}
	}
}