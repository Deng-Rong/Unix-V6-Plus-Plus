using System;
using System.Collections.Generic;
using System.Text;

namespace MakeImage
{
    class DataBlockManager
    {
        private SuperBlock spb;

        public DataBlockManager(ref SuperBlock aSpb)
        {
            spb = aSpb;
        }

        /* 建立“栈的栈”。用十六进制编辑器比较，
         * 与内核中MakeFS()生成的完全一致，可放心使用。
         */
        public void FreeAllBlock()
        {
            /* 最后一组用s_free[0] = 0作为结束标志 */
            spb.s_free[0] = 0;
            spb.s_nfree++;

            for(int blkno = Constant.DATA_ZONE_END_SECTOR; blkno >= Constant.DATA_ZONE_START_SECTOR; blkno--)
            {
                Free(blkno);
            }
        }

        /* 释放一个空闲盘块，效果等同于FileSystem::Free()函数 */
        public void Free(int blkno)
        {
            spb.s_fmod = 1;

            if(spb.s_nfree >= 100)
            {
                byte[] buffer = new byte[Constant.BLOCK_SIZE];

                Tool.WriteInt(spb.s_nfree, ref buffer, 0);

                for(int i = 0; i < 100; i++)
                {
                    Tool.WriteInt(spb.s_free[i], ref buffer, 4 + i * 4);
                }

                Tool.WriteSector(ref buffer, blkno);

                spb.s_nfree = 0;
            }

            spb.s_free[spb.s_nfree++] = blkno;
            spb.s_fmod = 1;
        }

        /* 分配一个空闲盘块，返回盘块号 */
        public int Alloc()
        {
            int blkno = -1;

            blkno = spb.s_free[--spb.s_nfree];

            /* 耗尽所有空闲盘块 */
            if(0 == blkno)
            {
                Console.WriteLine("No More Space!!\n");
                return 0;
            }

            /* 栈已空，需要读入下一组空闲盘块到spb.s_free[100]中 */
            if(spb.s_nfree <= 0)
            {
                byte[] buffer = Tool.ReadSector(blkno);

                spb.s_nfree = Tool.ReadInt(ref buffer, 0);

                for(int i = 0; i < 100; i++)
                {
                    spb.s_free[i] = Tool.ReadInt(ref buffer, 4 + i * 4);
                }
            }

            /* 将新申请到的盘块清零 */
            Tool.ClearBlock(blkno);
            spb.s_fmod = 1;
            return blkno;
        }

        public bool AllocAllBlock()
        {
            int blkno;

            for(int i = 0; i < Constant.DATA_ZONE_SIZE; i++)
            {
                blkno = Alloc();
                Console.WriteLine("Block {0} Allocated!", blkno);

                if( i + Constant.DATA_ZONE_START_SECTOR != blkno)
                {
                    Console.WriteLine("Allocate Free Block Test Failed!!");
                    return false;
                }
            }
            return true;
        }
    }
}
