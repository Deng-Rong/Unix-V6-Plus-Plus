using System;
using System.Collections.Generic;
using System.Text;

namespace MakeImage
{
    class SuperBlock
    {
        public int	s_isize;		/* 外存Inode区占用的盘块数 */
	    public int	s_fsize;		/* 盘块总数 */
	
	    public int	s_nfree;		/* 直接管理的空闲盘块数量 */
	    public int[] s_free;	    /* 直接管理的空闲盘块索引表 */
	
	    public int	s_ninode;		/* 直接管理的空闲外存Inode数量 */
	    public int[] s_inode;       /* 直接管理的空闲外存Inode索引表 */
	
	    public int	s_flock;		/* 封锁空闲盘块索引表标志 */
	    public int  s_ilock;		/* 封锁空闲Inode表标志 */
	
	    public int	s_fmod;			/* 内存中super block副本被修改标志，意味着需要更新外存对应的Super Block */
	    public int	s_ronly;		/* 本文件系统只能读出 */
	    public int	s_time;			/* 最近一次更新时间 */
	    public int[] padding;       /* 填充使SuperBlock块大小等于1024字节，占据2个扇区 */

        public SuperBlock()
        {
            s_isize = Constant.INODE_ZONE_SIZE;
            s_fsize = Constant.NSECTOR;
            
            s_nfree = 0;
            s_free = new int[100];
            s_ninode = 0;
            s_inode = new int[100];

            s_flock = 0;
            s_ilock = 0;

            s_fmod = 0;
            s_ronly = 0;

            /* write some feature bytes of Superblock */
            s_time = 0x1ABBCC4D;
            padding = new int[47];
            padding[46] = 0x473C2B1A;
        }

        public void Update()
        {
            /* Super Block的大小为1024字节 */
            byte[] buffer = new byte[1024];
            int offset = 0;

            /*------------------------------------------*/
            /* 将SuperBlock各个字段依次写入buffer[1024] */
            /*------------------------------------------*/

            Tool.WriteInt(s_isize, ref buffer, offset);
            offset += sizeof(int);

            Tool.WriteInt(s_fsize, ref buffer, offset);
            offset += sizeof(int);

            Tool.WriteInt(s_nfree, ref buffer, offset);
            offset += sizeof(int);

            for(int i = 0; i < s_free.Length; i++)
            {
                Tool.WriteInt(s_free[i], ref buffer, offset + i * 4);
            }
            offset += s_free.Length * sizeof(int);

            Tool.WriteInt(s_ninode, ref buffer, offset);
            offset += sizeof(int);
            
            for(int i = 0; i < s_inode.Length; i++)
            {
                Tool.WriteInt(s_inode[i], ref buffer, offset + i * 4);
            }
            offset += s_inode.Length * sizeof(int);

            Tool.WriteInt(s_flock, ref buffer, offset);
            offset += sizeof(int);

            Tool.WriteInt(s_ilock, ref buffer, offset);
            offset += sizeof(int);

            Tool.WriteInt(s_fmod, ref buffer, offset);
            offset += sizeof(int);

            Tool.WriteInt(s_ronly, ref buffer, offset);
            offset += sizeof(int);

            Tool.WriteInt(s_time, ref buffer, offset);
            offset += sizeof(int);

            for (int i = 0; i < padding.Length; i++)
            {
                Tool.WriteInt(padding[i], ref buffer, offset + i * 4);
            }
            offset += padding.Length * sizeof(int);

            if (offset != 1024)
            {
                Console.WriteLine("offset wrong");
                while (true) ;
            }
            
            /* 将buffer中内容写到(100, 101)磁盘块中 */
            Tool.WriteSector(ref buffer, Constant.SUPER_BLOCK_SECTOR_NUMBER);
        }
    }
}
