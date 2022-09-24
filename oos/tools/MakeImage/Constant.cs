using System;
using System.Collections.Generic;
using System.Text;

namespace MakeImage
{
    class Constant
    {
        /* 定义一些常量 */
        
        /* 字符块大小 */
        public static int BLOCK_SIZE = 512;

        /* 定义SuperBlock位于磁盘上的扇区号，占据100，101两个扇区。 */
        public static int SUPER_BLOCK_SECTOR_NUMBER = 200;

        /* 文件系统根目录外存Inode编号 */
        public static int ROOTINO = 0;

        /* 外存INode对象长度为64字节，每个磁盘块可以存放512/64 = 8个外存Inode */
        public static int INODE_NUMBER_PER_SECTOR = 8;

        /* 外存Inode区位于磁盘上的起始扇区号 */
        public static int INODE_ZONE_START_SECTOR = 202;

	    /* 磁盘上外存Inode区占据的扇区数 */
        public static int INODE_ZONE_SIZE = 1024 - 202;

        /* 数据区的起始扇区号 */
	    public static int DATA_ZONE_START_SECTOR = 1024;

        /* C.img扇区总数 */
        public static int NSECTOR = 20160;

        /* exe Start Block */
        public static int EXEBLOCK = 19000;

	    /* 数据区的结束扇区号 */
        public static int DATA_ZONE_END_SECTOR = 18000 - 1;

        /* 数据区占据的扇区数量 */
        public static int DATA_ZONE_SIZE = 18000 - DATA_ZONE_START_SECTOR;
    }
}
